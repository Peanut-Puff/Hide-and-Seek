
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Spawning;
using Ubiq.Geometry;
using UnityEngine.XR;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

#endif

namespace Ubiq.Samples
{

    public class Bullet : MonoBehaviour, INetworkSpawnable
    {
        public float bulletspeed;
        public bool ishitanything;
        public Vector3 hitonSpot;
        public float force;
        public AudioClip hitSound; 

        public float vibrationIntensity = 0.5f; 
        public float vibrationDuration = 0.2f; 

        public float knockbackForce = 3f; 
        public float knockbackDuration = 0.3f; 

        public float tiltAngle = 20f; 
        public float tiltDuration = 0.15f; 
        private bool isTilting = false; 
        private Quaternion originalRotation= Quaternion.identity; 

        public bool owner;
        public bool ishit = false;
        private NetworkContext context;

        public NetworkId NetworkId { get; set; }

        private float explodeTime;

        private void Start()
        {
            context = NetworkScene.Register(this);
        }
        public void shoot(Vector3 hitposition,Vector3 forward)
        {
            transform.position = hitposition;

            Rigidbody rb = GetComponent<Rigidbody>();
            Transform bulletTransform = GetComponent<Transform>();
            bulletTransform.rotation = Quaternion.LookRotation(forward);

            if (rb != null)
            {
                rb.AddForce(forward * bulletspeed, ForceMode.Impulse);
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
        private IEnumerator TiltBackRoutine(GameObject hitObject, Vector3 knockbackDirection)
        {
            float timer = 0f;
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, knockbackDirection).normalized;

            Debug.Log(originalRotation);
            Quaternion targetRotation = Quaternion.AngleAxis(tiltAngle, tiltAxis) * originalRotation;


            while (timer < tiltDuration)
            {
                hitObject.transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, timer / tiltDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            while (timer < tiltDuration)
            {
                hitObject.transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, timer / tiltDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            hitObject.transform.rotation = Quaternion.identity; 
            isTilting = false;
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
            SendMessage();
            if (ishitanything)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Collider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
            }
            if (ishit == true)
            {
                if (hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitSound, hitonSpot);
                }
                ishit = false;
            }

            if (ishitanything == true && Time.time > explodeTime)
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
            public Vector3 hitonSpot;
        }
        private void SendMessage()
        {
            var message = new Message();
            message.pose = Transforms.ToLocal(transform, context.Scene.transform);
            message.ishit = ishit;
            message.hitonSpot = hitonSpot;
            context.SendJson(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<Message>();
            var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            ishit = msg.ishit;
            hitonSpot = msg.hitonSpot;
        }

    }

}