using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Geometry;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System.Linq;
using UnityEngine.UIElements;
using Ubiq.Rooms;
using Ubiq.Avatars;
using Ubiq.Avatars;






#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{
    public class LaserGun : MonoBehaviour, INetworkSpawnable
    {
        //network
        public float cooltime;
        private float lastFireTime;
        //shooting part
        public float laserRadius;
        Vector3 laserEnd;
        public bool isfiring;
        public Vector3 firePoint;
        public GameObject laserBeam; 
        //public LineRenderer laserLine; 
        public float laserDuration = 0.1f; 
        public float laserRange = 50f;
        private List<Avatars.Avatar> avatars;

        public InputActionReference MyLeftTrigger;
        public InputActionReference MyRightTrigger;
        private InputAction spaceAction;
        private InputAction yAction;

        private Rigidbody body;
        //private ParticleSystem particles;
        public NetworkId NetworkId { get; set; }
        public bool iscatched;
        public bool owner;
        public bool fired;

        //laser
        public bool laserScoreCool;
        public float laserHitCoolTime;
        public float laserHitCoolRange;
        private bool ishit;
        public Vector3 hitonSpot;
        public float force;
        public AudioClip hitSound;
        public AudioClip SoundOfLaser;
        public float knockbackForce = 3f; // HIT BACK
        public float knockbackDuration = 0.3f; 
        private GameManager gameManager;
        public GetPosition AvatrPositionEnd;
        private float lastSoundTime = 0f;
        private List<Vector3> randomTransport = new List<Vector3>();
        private string hitAvatarName;
        private string myName;
        private RoomClient roomClient;
        private XRGrabInteractable grab;

        private void OnEnable()
        {
            SetupInteractorEvents();
        }
        private void OnDisable()
        {
            TeardownInteractorEvents();
        }
        void SetupInteractorEvents()
        {
            var MyLeftButton_X_Action = GetInputAction(MyLeftTrigger);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed += OnMyLeftButton_X_Action;
            }
            var MyRightButton_X_Action = GetInputAction(MyRightTrigger);
            if (MyRightButton_X_Action != null)
            {
                MyRightButton_X_Action.performed += OnMyRightButton_X_Action;
            }

        }
        void TeardownInteractorEvents()
        {
            var MyLeftButton_X_Action = GetInputAction(MyLeftTrigger);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed -= OnMyLeftButton_X_Action;
                MyLeftButton_X_Action.Disable();
                MyLeftButton_X_Action.Dispose();
            }

            var MyRightButton_X_Action = GetInputAction(MyRightTrigger);
            if (MyRightButton_X_Action != null)
            {
                MyRightButton_X_Action.performed -= OnMyRightButton_X_Action;
                MyRightButton_X_Action.Disable();
                MyRightButton_X_Action.Dispose();
            }
        }
        private void CylinderChangeBack()
        {
            laserBeam.transform.position = transform.position;
            float laserLength = 0.02f;
            laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);
            ishit = false;
        }
        private void OnMyLeftButton_X_Action(InputAction.CallbackContext context)
        {
            Fire();
        }
        private void OnMyRightButton_X_Action(InputAction.CallbackContext context)
        {
            Fire();
        }
        private void Fire()
        {
            if (Time.time - lastFireTime < cooltime) // cooling
            {
                return;
            }
            if (iscatched)
            {
                //bulletPrefab.transform.position = transform.position + transform.forward * 0.6f;

                FireLaser();
                //bulletTransform.rotation = Quaternion.LookRotation(transform.forward);

                
            }
        }

        private bool DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineVec = lineEnd - lineStart;
            Vector3 pointVec = point - lineStart;

            float lineLength = lineVec.magnitude;
            if (lineLength == 0) return Vector3.Distance(point, lineStart) <= laserRadius; 

            float t = Mathf.Clamp01(Vector3.Dot(pointVec, lineVec) / (lineLength * lineLength));
            Vector3 projection = lineStart + t * lineVec;
            float distance = Vector3.Distance(point, projection);
            return  distance <= laserRadius;
        }

        Transform GetActiveChild(GameObject parent)
        {
            foreach (Transform child in parent.transform) 
            {
                if (child.gameObject.activeSelf) 
                {
                    return child;
                }
            }
            return null; 
        }
        Transform GetFloatingBody(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                if (child.name.Contains("Floating_Torso_A"))
                {
                    return child;
                }
            }
            return null;
        }
        private IEnumerator CheckIfHiton()
        {
            avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            List<GameObject> objectList = new List<GameObject>();

            foreach (var avatar in avatars)
            {
                //Debug.Log($"Found Avatar on: {avatar.gameObject.transform.position}");
                Transform activeChild = GetActiveChild(avatar.gameObject);
                //if (activeChild.name.Contains("Body"))
                Transform floatingBody = GetFloatingBody(activeChild.gameObject);
                //Debug.Log($"Found Avatar {avatar.Peer[DisplayNameManager.KEY]}: {floatingBody.position}");
                objectList.Add(floatingBody.gameObject);
            }
            while (Time.time < lastFireTime + laserDuration && laserScoreCool)
            {

                // need point of ray
                laserEnd = transform.position + transform.forward * laserRange;
                firePoint = transform.position + transform.forward * 0.6f;
                                               //laserLine.SetPosition(0, firePoint);
                laserBeam.transform.position = (firePoint + laserEnd) / 2;
                laserBeam.transform.rotation = Quaternion.LookRotation(laserEnd - firePoint) * Quaternion.Euler(90, 0, 0); ;


                float laserLength = Vector3.Distance(firePoint, laserEnd);
                laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);

                int avatarcount = 0;
                // Raycast
                foreach (GameObject obj in objectList)
                {
                    if (obj == null) continue;
                    //Debug.Log("avatar name:"+ obj.transform.position);

                    if (DistancePointToLineSegment(obj.transform.position, firePoint, laserEnd))
                    {
                        //Debug.Log($"Avatar {obj.name} is hit by laser!");
                        hitAvatarName= avatars[avatarcount].Peer[DisplayNameManager.KEY];
                        StartCoroutine( GotHitReaction(obj.gameObject));
                        laserScoreCool = false;
                        laserHitCoolTime = Time.time;
                    }
                    avatarcount += 1;
                }

                laserLength = Vector3.Distance(firePoint, laserEnd);
                laserBeam.transform.position = (firePoint + laserEnd) / 2;
                laserBeam.transform.rotation = Quaternion.LookRotation(laserEnd - firePoint) * Quaternion.Euler(90, 0, 0); ;
                laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);
                yield return null; // update every frame
            }
            CylinderChangeBack();
            //laserBeam.SetActive(false);
            //laserLine.enabled = false;
            isfiring = false;
            owner = true;
        }
        private void FireLaser()
        {
            //laserBeam.SetActive(true);
            lastFireTime = Time.time;
            if (SoundOfLaser != null)
            {
                AudioSource.PlayClipAtPoint(SoundOfLaser,transform.position);
            }
            isfiring = true;
            owner = true;
            StartCoroutine(CheckIfHiton());

        }
        public IEnumerator GotHitReaction(GameObject hitObject)
        {
            ishit = true;
            //CharacterController rb = hitObject.GetComponent<CharacterController>();
            //shake mine controllers
            GameObject myself = GameObject.Find("XR Origin Hands (XR Rig)");
            XRDirectInteractor[] controllers = myself.GetComponentsInChildren<XRDirectInteractor>();
            hitonSpot = hitObject.transform.position;
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, hitObject.transform.position);
            }

            FindFirstObjectByType<NetworkScoreboard>().AddScore("catcher", 1);
            if (controllers.Length > 0)
            {
                foreach (XRDirectInteractor controller in controllers)
                {
                    // get xr
                    UnityEngine.XR.InputDevice device = GetXRDevice(controller);

                    // shake controller
                    SendHapticFeedback(device, 0.5f, 0.2f);
                }
            }
            yield return null;
        }



        private UnityEngine.XR.InputDevice GetXRDevice(XRDirectInteractor interactor)
        {
            string name = interactor.gameObject.name.ToLower();
            XRNode node = name.Contains("left") ? XRNode.LeftHand : XRNode.RightHand;
            return InputDevices.GetDeviceAtXRNode(node);
        }
        private void SendHapticFeedback(UnityEngine.XR.InputDevice device, float amplitude, float duration)
        {
            if (device.isValid)
            {
                HapticCapabilities capabilities;
                if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0, amplitude, duration);
                }
            }
        }
        private IEnumerator KnockbackRoutine(CharacterController characterController, Vector3 knockbackDirection)
        {
            float timer = 0f;

            while (timer < knockbackDuration)
            {
                characterController.Move(knockbackDirection * knockbackForce * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
#if XRI_3_0_7_OR_NEWER

        private NetworkContext context;
        //private Vector3 flightForce;
        //private float explodeTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            // particles = GetComponentInChildren<ParticleSystem>();
            owner = false;
        }

        private void Start()
        {
            var networkScene = NetworkScene.Find(this);
            roomClient = networkScene.GetComponentInChildren<RoomClient>();
            myName = roomClient.Me[DisplayNameManager.KEY];
            //network
            gameManager = FindObjectOfType<GameManager>();
            body.isKinematic = true;
            context = NetworkScene.Register(this);
            grab = GetComponent<XRGrabInteractable>();
            grab.selectExited.AddListener(Interactable_SelectExited);
            grab.selectEntered.AddListener(Interactable_SelectEntered);
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
                if (body == null)
                {
                    Debug.LogError("Rigidbody is missing on " + gameObject.name);
                }
            }

            //space controll
            spaceAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
            spaceAction.performed += OnSpacePressed;
            spaceAction.Enable();

            yAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/y");
            yAction.performed += OnYPressed;
            yAction.Enable();

            //set random transport spot
            float yaxis = 1.69f;
            randomTransport.Add(new Vector3(23, yaxis, 40));
            randomTransport.Add(new Vector3(3.5f, yaxis,5.5f));
            randomTransport.Add(new Vector3(-7f, yaxis, 26f));
            randomTransport.Add(new Vector3(6.5f, yaxis, 27f));
            randomTransport.Add(new Vector3(18f, yaxis, 32f));
            randomTransport.Add(new Vector3(5.5f, yaxis, 45f));
        }
        private void OnDestroy()
        {
            Debug.Log("destory gun event");
            TeardownInteractorEvents();
            grab.selectExited.RemoveListener(Interactable_SelectExited);
            grab.selectEntered.RemoveListener(Interactable_SelectEntered);
            if (spaceAction != null)
            {
                spaceAction.Disable();  // 禁用 InputAction
                spaceAction.performed -= OnSpacePressed;  // 解绑事件
                spaceAction.Dispose();  // 彻底释放资源
            }

            if (yAction != null)
            {
                yAction.Disable();
                yAction.performed -= OnYPressed;
                yAction.Dispose();
            }
        }
        private void OnYPressed(InputAction.CallbackContext context)
        {
            ForceToDrop(GetComponent<XRGrabInteractable>());

        }
        private void OnSpacePressed(InputAction.CallbackContext context)
        {
            Fire();
        }

        private void Interactable_SelectEntered(SelectEnterEventArgs eventArgs)
        {
            iscatched = true;
        }
        private void Interactable_SelectExited(SelectExitEventArgs eventArgs)
        {
            fired = true;
            Debug.Log("fired: " + fired);


            iscatched = false;
        }
        private void ForceToDrop(XRGrabInteractable interactable)
        {
            if (interactable.interactionManager && interactable.isSelected)
            {
                interactable.interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
            }
        }
        private IEnumerator Wait2Second()
        {
            yield return new WaitForSeconds(2f);
            var spawner = NetworkSpawnManager.Find(this);
            if (spawner == null)
            {
                Debug.LogError("NetworkSpawnManager is null. Cannot despawn object.");
            }
            NetworkSpawnManager.Find(this).Despawn(gameObject);
        }
        private void FixedUpdate()
        {
            if (Time.time>laserHitCoolTime+laserHitCoolRange)
            {
                laserScoreCool = true;
            }
            if (isfiring) //
            {
                firePoint = transform.position + transform.forward * 0.6f;
                laserEnd = firePoint + transform.forward * laserRange;
                float laserLength = Vector3.Distance(firePoint, laserEnd);
                laserBeam.transform.rotation = Quaternion.LookRotation(laserEnd - firePoint) * Quaternion.Euler(90, 0, 0); ;
                laserBeam.transform.position = (firePoint + laserEnd) / 2;
                laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);

            }
            else
            {
                CylinderChangeBack();
            }
            if (owner)
            {
                SendMessage();
            }
            if (ishit == true && Time.time > lastSoundTime && hitAvatarName==myName)
            {
                Debug.Log("IVE BEEN HURTED");
                if (hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitSound, hitonSpot);
                }
                GameObject myself = GameObject.Find("XR Origin Hands (XR Rig)");
                int randomIndex = Random.Range(0, randomTransport.Count);
                myself.transform.position = randomTransport[randomIndex];
                ishit = false;
                hitAvatarName = " ";
                lastSoundTime = Time.time + 2f;
            }
            if (!gameManager.gameStarted)
            {
                ForceToDrop(GetComponent<XRGrabInteractable>());
                //StartCoroutine(Wait2Second());
                transform.position = new Vector3(-6,3,54);
               return;
            }
        }

        private struct Message
        {
            public Pose pose;
            public bool iscatched;
            public bool ishit;
            public Vector3 hitonSpot;
            public bool isfiring;
            public Vector3 firePoint;
            public Vector3 laserEnd;
            public string hitavatarName;
        }

        private void SendMessage()
        {
            var message = new Message();
            message.pose = Transforms.ToLocal(transform, context.Scene.transform);
            message.iscatched = iscatched;
            message.ishit = ishit;
            message.isfiring = isfiring;
            message.hitonSpot = hitonSpot;
            message.firePoint = firePoint;
            message.laserEnd = laserEnd;
            message.hitavatarName = hitAvatarName;
            context.SendJson(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<Message>();
            var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            iscatched = msg.iscatched;
            ishit = msg.ishit;
            isfiring = msg.isfiring;
            hitonSpot = msg.hitonSpot;
            firePoint = msg.firePoint;
            laserEnd = msg.laserEnd;
            hitAvatarName = msg.hitavatarName;
        }

#endif
    }
}