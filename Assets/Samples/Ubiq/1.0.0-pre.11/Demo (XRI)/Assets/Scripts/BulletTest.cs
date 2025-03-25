using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    public class BulletTest : MonoBehaviour, INetworkSpawnable
    {
        private Rigidbody body;

        public NetworkId NetworkId { get; set; }
        public float bulletSpeed;
        public bool owner;
        public bool ishit;
        public bool isflying;

#if XRI_3_0_7_OR_NEWER

        private NetworkContext context;
        private Vector3 flightForce;
        private float explodeTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            //particles = GetComponentInChildren<ParticleSystem>();
            owner = false;
        }

        private void Start()
        {
            context = NetworkScene.Register(this);
            //flightForce = new Vector3(
            //    x: (Random.value - 0.5f) * 0.05f,
            //    y: 3.0f,
            //    z: (Random.value - 0.5f) * 0.05f);
            //explodeTime = Time.time + 10.0f;
            //body.AddForce(flightForce, ForceMode.Force);
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
            isflying = true;
            owner = true;

        }
        private void FixedUpdate()
        {
            if (owner)
            {
                SendMessage();
            }
            if (owner && isflying)
            {
                body.isKinematic = false;
                //body.AddForce(flightForce, ForceMode.Force); /

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

        }

        private void SendMessage()
        {
            var message = new Message();
            message.pose = Transforms.ToLocal(transform, context.Scene.transform);
            message.ishit = ishit;
            message.isflying = isflying;
            context.SendJson(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<Message>();
            var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            ishit = msg.ishit;
            isflying = msg.isflying;
        }

#endif
    }
}
