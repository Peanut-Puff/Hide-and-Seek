
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
        public bool ishitanything;
        public Vector3 hitonSpot;
        public float force;
        public AudioClip hitSound; // ������Ч

        public float vibrationIntensity = 0.5f; // ��ǿ�� (0~1)
        public float vibrationDuration = 0.2f; // ��ʱ�� (��)

        public float knockbackForce = 3f; // ��������
        public float knockbackDuration = 0.3f; // ���˳���ʱ��

        public float tiltAngle = 20f; // �����Ƕ�
        public float tiltDuration = 0.15f; // ��������ʱ��
        private bool isTilting = false; // ����Ƿ����ں���
        private Quaternion originalRotation= Quaternion.identity; // ��¼��ʼ��ת�Ƕ�

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
                ishitanything = true;
                Debug.Log("you do hit on :"+ hitObject.name);
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

                //FindFirstObjectByType<NetworkScoreboard>().AddScore("hider", 1);
                FindFirstObjectByType<NetworkScoreboard>().AddScore("catcher", 1);
                if (controllers.Length > 0)
                {
                    foreach (XRDirectInteractor controller in controllers)
                    {
                        // ��ȡ��Ӧ���ֱ��豸
                        UnityEngine.XR.InputDevice device = GetXRDevice(controller);

                        // �����ֱ��𶯣���ǿ�� 0.5������ 0.2 �룩
                        SendHapticFeedback(device, 0.5f, 0.2f);
                    }
                }
                if (rb != null)
                {
                    Vector3 knockbackDirection = (hitObject.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0; // ����ˮƽ���򣬲��ý�ɫ������

                    StartCoroutine(KnockbackRoutine(rb,knockbackDirection));
                    //StartCoroutine(TiltBackRoutine(hitObject,knockbackDirection));

                }
            }

            explodeTime = Time.time+7f;
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
            // **Ŀ������Ƕ�**
            Quaternion targetRotation = Quaternion.AngleAxis(tiltAngle, tiltAxis) * originalRotation;


            // **ǰ��Σ�����**
            while (timer < tiltDuration)
            {
                hitObject.transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, timer / tiltDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            // **���Σ��ָ�**
            timer = 0f;
            while (timer < tiltDuration)
            {
                hitObject.transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, timer / tiltDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            hitObject.transform.rotation = Quaternion.identity; // ȷ����ȫ�ָ�
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

        //private IEnumerator KnockbackRotation(Transform target)
        //{
        //    Quaternion originalRotation = target.rotation; // ��¼ԭʼ��ת
        //    Quaternion knockbackRotation = originalRotation * Quaternion.Euler(-20f, 0f, 0f); // ����� 20 ��

        //    // 0.3s ���������
        //    float duration = hittime/2;
        //    float elapsedTime = 0f;
        //    while (elapsedTime < duration)
        //    {
        //        target.rotation = Quaternion.Slerp(originalRotation, knockbackRotation, elapsedTime / duration);
        //        elapsedTime += Time.deltaTime;
        //        yield return null;
        //    }
        //    target.rotation = knockbackRotation; // ȷ����ȫ��ת��Ŀ��Ƕ�

        //    yield return new WaitForSeconds(hittime/2); // ͣ�� 0.3s

        //    // 0.3s �ڻָ���ԭʼ�Ƕ�
        //    elapsedTime = 0f;
        //    while (elapsedTime < duration)
        //    {
        //        target.rotation = Quaternion.Slerp(knockbackRotation, originalRotation, elapsedTime / duration);
        //        elapsedTime += Time.deltaTime;
        //        yield return null;
        //    }
        //    target.rotation = originalRotation; // ȷ����ȫ�ָ�
        //}

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
                Debug.Log("destory");
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