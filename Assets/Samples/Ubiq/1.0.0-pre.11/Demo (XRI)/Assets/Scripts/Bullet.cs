
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Spawning;
using Ubiq.Geometry;
using UnityEngine.InputSystem.Utilities;

#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{

    public class Bullet : MonoBehaviour, INetworkSpawnable
    {
        public float force;
        public bool owner;
        public bool ishit = false;
        private NetworkContext context;

        public NetworkId NetworkId { get; set; }

        private float explodeTime;

        private void Start()
        {
            context = NetworkScene.Register(this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameObject hitObject = collision.gameObject;

            Transform hitObjectTransform = hitObject.GetComponent<Transform>();

            if (hitObjectTransform != null)
            {
                ishit = true;
                Debug.Log("you do hit on something");
            }
            if(hitObject.name == "XR Origin Hands (XR Rig)")
            {
                Debug.Log("hit on avatar");
                Rigidbody rb = hitObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Debug.Log("fly back");
                    Vector3 hitDirection = (hitObject.transform.position - transform.position).normalized;
                    rb.AddForce(hitDirection * force, ForceMode.Impulse);
                }
            }

            explodeTime = Time.time + 1.0f;
        }

        private void FixedUpdate()
        {
            SendMessage();
            if(ishit == true && Time.time > explodeTime)
            {
                var spawner = NetworkSpawnManager.Find(this);
                if (spawner == null)
                {
                    Debug.LogError("NetworkSpawnManager is null. Cannot despawn object.");
                }
                NetworkSpawnManager.Find(this).Despawn(gameObject);

            }
        }
        private struct Message
        {
            public Pose pose;
            public bool ishit;
        }
        private void SendMessage()
        {
            var message = new Message();
            message.pose = Transforms.ToLocal(transform, context.Scene.transform);
            message.ishit = ishit;
            context.SendJson(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<Message>();
            var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            ishit = msg.ishit;
        }

    }

}