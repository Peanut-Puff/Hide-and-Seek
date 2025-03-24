using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Geometry;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{
    public class Gun : MonoBehaviour, INetworkSpawnable
    {
        //network
        private NetworkSpawnManager spawnManager;
        public float cooltime;
        private float lastFireTime;
        //shooting part
        public GameObject bulletPrefab;
        //public float bulletSpeed = 50f; // 
        public InputActionReference MyLeftTrigger;
        public InputActionReference MyRightTrigger;
        private InputAction spaceAction;

        private Rigidbody body;
        //private ParticleSystem particles;
        public NetworkId NetworkId { get; set; }
        public bool iscatched;
        public bool owner;
        public bool fired;

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
                var go = spawnManager.SpawnWithPeerScope(bulletPrefab);
                var bullet = go.GetComponent<Bullet>();
                bullet.shoot(transform.position + transform.forward * 0.6f,transform.forward);
                //bullet.transform.position = transform.position + transform.forward * 0.6f;
                //bullet.owner = true;
                //Rigidbody rb = bullet.GetComponent<Rigidbody>();
                //Transform bulletTransform = bullet.GetComponent<Transform>();
                //bulletTransform.rotation = Quaternion.LookRotation(transform.forward);

                //if (rb != null)
                //{
                //    rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
                //    //rb.linearVelocity = transform.forward * bulletSpeed;
                //}
                lastFireTime = Time.time;
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
            spawnManager = NetworkSpawnManager.Find(this);

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
            // No longer interactable
            //var interactable = (XRGrabInteractable)eventArgs.interactableObject;
            //interactable.enabled = false;
            //var spawner = NetworkSpawnManager.Find(this);
            //if (spawner == null)
            //{
            //    Debug.LogError("NetworkSpawnManager is null. Cannot despawn object.");
            //}
            //NetworkSpawnManager.Find(this).Despawn(gameObject);
        }

        private void FixedUpdate()
        {
            if (owner)
            {
                SendMessage();
            }
            //if (owner && fired)
            //{

            //    var spawner = NetworkSpawnManager.Find(this);
            //    if (spawner == null)
            //    {
            //        Debug.LogError("NetworkSpawnManager is null. Cannot despawn object.");
            //    }
            //    NetworkSpawnManager.Find(this).Despawn(gameObject);
            //    return;
            //}
            //if (!owner && fired)
            //{
            //    if (!particles.isPlaying)
            //    {
            //        particles.Play();
            //    }
            //}
        }

        private struct Message
        {
            public Pose pose;
            public bool iscatched;
        }

        private void SendMessage()
        {
            var message = new Message();
            message.pose = Transforms.ToLocal(transform, context.Scene.transform);
            message.iscatched = iscatched;
            context.SendJson(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<Message>();
            var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            iscatched = msg.iscatched;
        }

#endif
    }
}