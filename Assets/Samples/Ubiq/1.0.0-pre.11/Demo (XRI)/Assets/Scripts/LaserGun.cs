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

        private Rigidbody body;
        //private ParticleSystem particles;
        public NetworkId NetworkId { get; set; }
        public bool iscatched;
        public bool owner;
        public bool fired;

        //laser
        private bool ishitanything;
        private bool ishit;
        public Vector3 hitonSpot;
        public float force;
        public AudioClip hitSound;
        public float knockbackForce = 3f; // HIT BACK
        public float knockbackDuration = 0.3f; 

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
            }

            var MyRightButton_X_Action = GetInputAction(MyRightTrigger);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed -= OnMyRightButton_X_Action;
            }
        }
        private void CylinderChangeBack()
        {
            laserBeam.transform.position = transform.position;
            float laserLength = 0.02f;
            laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);
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

                StartCoroutine(FireLaser());
                //bulletTransform.rotation = Quaternion.LookRotation(transform.forward);

                lastFireTime = Time.time;
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

        private List<GameObject> GetObjectsWithName(string keyword)
        {
            return FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.Contains(keyword))
                .ToList();
        }

        private IEnumerator FireLaser()
        {
            //laserBeam.SetActive(true);
            isfiring = true;
            float startTime = Time.time;
            avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            while (Time.time < startTime + laserDuration)
            {
                // ned point of ray
                laserEnd = transform.position + transform.forward * laserRange;
                firePoint = transform.position;// + transform.forward * 0.6f;
                                               //laserLine.SetPosition(0, firePoint);
                laserBeam.transform.position = (firePoint + laserEnd) / 2; 
                laserBeam.transform.rotation = Quaternion.LookRotation(laserEnd - firePoint) * Quaternion.Euler(90, 0, 0);;


                float laserLength = Vector3.Distance(firePoint, laserEnd);
                laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);

                bool ishitAvatar = false;
                
                // Raycast
                //foreach (var avatar in avatars)
                //{
                //    if (avatar == null) continue;

                //    if (DistancePointToLineSegment(avatar.transform.position,firePoint,laserEnd))
                //    {
                //        Debug.Log($"Avatar {avatar.Peer[DisplayNameManager.KEY]} is hit by laser!");
                //        ishitAvatar = true;
                //    }
                //}
                List<GameObject> originHandObjects = GetObjectsWithName("Origin Hand");
                for (int i = 0; i < originHandObjects.Count; i++)
                {
                    Debug.Log(originHandObjects[i].name);
                    //Debug.Log(originHandObjects[i].transform.position);
                }
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, laserRange))
                {
                    laserEnd = hit.point; //end of ray

                    GameObject hitObject = hit.collider.gameObject;

                    Transform hitObjectTransform = hitObject.GetComponent<Transform>();

                    if (hitObjectTransform != null)
                    {
                        ishitanything = true;
                        Debug.Log("hit on :" + hitObject.name);
                    }
                    if (hitObject.name == "XR Origin Hands (XR Rig)")//(hitObject.name == "shell")
                    {
                        ishit = true;
                        Debug.Log("hit on avatar");
                        CharacterController rb = hitObject.GetComponent<CharacterController>();
                        XRDirectInteractor[] controllers = hitObject.GetComponentsInChildren<XRDirectInteractor>();
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
                        if (rb != null)
                        {
                            Vector3 knockbackDirection = (hitObject.transform.position - transform.position).normalized;
                            knockbackDirection.y = 0; 

                            StartCoroutine(KnockbackRoutine(rb, knockbackDirection));
                            //StartCoroutine(TiltBackRoutine(hitObject,knockbackDirection));

                        }
                    }
                }

                ////`LineRenderer
                //laserLine.SetPosition(1, laserEnd);

                //yield return new WaitForSeconds(laserDuration); 
                //laserLine.enabled = false; 
                //laserLine.SetPosition(0, firePoint);
                //laserLine.SetPosition(1, laserEnd);
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
            //network
            //laserBeam.SetActive(false);
            //laserLine.enabled = false; //hide
            body.isKinematic = true;
            context = NetworkScene.Register(this);
            var grab = GetComponent<XRGrabInteractable>();
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

        private void FixedUpdate()
        {
            if (isfiring) //
            {
                firePoint = transform.position;// + transform.forward * 0.6f;
                laserEnd = firePoint + transform.forward * laserRange;
                float laserLength = Vector3.Distance(firePoint, laserEnd);
                laserBeam.transform.rotation = Quaternion.LookRotation(laserEnd - firePoint) * Quaternion.Euler(90, 0, 0); ;
                laserBeam.transform.position = (firePoint + laserEnd) / 2;
                laserBeam.transform.localScale = new Vector3(laserBeam.transform.localScale.x, laserLength / 2, laserBeam.transform.localScale.z);

                //laserLine.SetPosition(0, firePoint);
                //laserLine.SetPosition(1, laserEnd);
            }
            else
            {
                CylinderChangeBack();
            }
            if (owner)
            {
                SendMessage();
            }
            if (ishit == true)
            {
                if (hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitSound, hitonSpot);
                }
                ishit = false;
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
        }

#endif
    }
}