using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Geometry;
using Ubiq.Avatars;
using UnityEngine.XR;
using Ubiq.Rooms;



#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{
    public class BulletTest : MonoBehaviour, INetworkSpawnable
    {
        private Rigidbody body;

        public NetworkId NetworkId { get; set; }
        public float bulletSpeed;
        public bool owner;
        public bool ishit;
        public bool isflying;
        public float Radius;
        private List<Avatars.Avatar> avatars;

        public Vector3 hitonSpot;
        public AudioClip hitSound;
        public AudioClip FireSound;
        public float knockbackForce = 3f; // HIT BACK
        public float knockbackDuration = 0.3f;
        private GameManager gameManager;

        public Vector3 LocalPosition;
        public Vector3 LocalRotation;
        public bool istrail;
        private float lastSoundTime = 0f;
        private List<Vector3> randomTransport = new List<Vector3>();
        private string hitAvatarName;
        private string myName;
        private RoomClient roomClient;

#if XRI_3_0_7_OR_NEWER

        private NetworkContext context;
        private float explodeTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            //GetComponent<TrailRenderer>().enabled = false;
            //istrail = false;
            owner = false;
        }

        private void Start()
        {
            var networkScene = NetworkScene.Find(this);
            roomClient = networkScene.GetComponentInChildren<RoomClient>();
            myName = roomClient.Me[DisplayNameManager.KEY];

            gameManager = FindObjectOfType<GameManager>();
            context = NetworkScene.Register(this);
            float yaxis = 1.69f;
            randomTransport.Add(new Vector3(23, yaxis, 40));
            randomTransport.Add(new Vector3(3.5f, yaxis, 5.5f));
            randomTransport.Add(new Vector3(-7f, yaxis, 26f));
            randomTransport.Add(new Vector3(6.5f, yaxis, 27f));
            randomTransport.Add(new Vector3(18f, yaxis, 32f));
            randomTransport.Add(new Vector3(5.5f, yaxis, 45f));
        }

        public void shoot(Vector3 hitposition, Vector3 forward)
        {
            transform.position = hitposition;
            //Debug.Log("bullet shoot is calling");
            Transform bulletTransform = GetComponent<Transform>();
            bulletTransform.rotation = Quaternion.LookRotation(forward);

            if (body != null)
            {
                body.AddForce(forward * bulletSpeed, ForceMode.Impulse);

            }
            explodeTime = Time.time+3f;
            owner = true;
            //GetComponent<TrailRenderer>().enabled = true;
            //istrail = true;
            FireLaser();
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
        private bool DistancePointToPointSegment(Vector3 pointA, Vector3 pointB)
        {
            float distance = Vector3.Distance(pointA, pointB);
            return distance <= Radius;
        }
        private IEnumerator CheckIfHiton()
        {
            istrail = false;
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
            
            while (isflying)
            {
                int avatarcount = 0;
                //Debug.Log("checking local");
                foreach (GameObject obj in objectList)
                {
                    if (obj == null) continue;
                    //Debug.Log(obj.transform.position+"    "+transform.position);

                    if (DistancePointToPointSegment(obj.transform.position, transform.position))
                    {
                        //Debug.Log(avatarcount);
                        hitAvatarName = avatars[avatarcount].Peer[DisplayNameManager.KEY];
                        GotHitReaction(obj.gameObject);
                        
                        isflying = false;
                        ishit = true;
                        break;
                    }
                    avatarcount += 1;
                }
                yield return null; // update every frame
            }
            ishit = false;
        }
        private void FireLaser()
        {
            if (FireSound != null)
            {
                AudioSource.PlayClipAtPoint(FireSound, transform.position);
            }
           // float startTime = Time.time;
            isflying = true;
            istrail = true;
            StartCoroutine(CheckIfHiton());
        }
        public void GotHitReaction(GameObject hitObject)
        {
            //Debug.Log("reaction");
            //Debug.Log("got hit and start to reaction");
            ishit = true;
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
        private void FixedUpdate()
        {
            if (owner)
            {
                SendMessage();
            }
            if (owner || isflying)
            {
                body.isKinematic = false;
                if (Time.time > explodeTime)
                {
                    NetworkSpawnManager.Find(this).Despawn(gameObject);
                    return;
                }
            }
            if (ishit == true && Time.time> lastSoundTime && hitAvatarName == myName)
            {
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
            if (isflying) //
            {
                //firePoint = transform.position;// + transform.forward * 0.6f;
            }
            if (ishit && !isflying)
            {
                body.isKinematic = false;
                if (Time.time > explodeTime)
                {
                    NetworkSpawnManager.Find(this).Despawn(gameObject);
                    return;
                }
            }
        }

        private struct Message
        {
            public Pose pose;
            public bool isflying;
            public bool ishit;
            public bool istrail;
            public string hitavatarName;

        }

        private void SendMessage()
        {
            var message = new Message();
            message.pose = Transforms.ToLocal(transform, context.Scene.transform);
            message.ishit = ishit;
            message.isflying = isflying;
            message.istrail = istrail;
            message.hitavatarName = hitAvatarName;
            context.SendJson(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<Message>();
            var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            istrail = msg.istrail;
            ishit = msg.ishit;
            isflying = msg.isflying;
            hitAvatarName = msg.hitavatarName;
        }

#endif
    }
}
