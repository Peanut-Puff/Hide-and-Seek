
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Spawning;
using Ubiq.Geometry;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{
    public class Shooting : MonoBehaviour
    {
        public GameObject bulletPrefab;
        //public Transform muzzleTransform; // ǹ��λ��
        public float bulletSpeed = 50f; // 
        public InputActionReference MyLeftTrigger;
        public InputActionReference MyLeftButton_Y;

        public InputActionReference MyRightTrigger;
        public InputActionReference MyRightButton_Y;


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
            //����
            var MyLeftButton_X_Action = GetInputAction(MyLeftTrigger);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed += OnMyLeftButton_X_Action;
            }
            var MyLeftButton_Y_Action = GetInputAction(MyLeftButton_Y);
            if (MyLeftButton_Y_Action != null)
            {
                MyLeftButton_Y_Action.performed += OnMyLeftButton_Y_Action;
            }
            //����
            var MyRightButton_X_Action = GetInputAction(MyRightTrigger);
            if (MyRightButton_X_Action != null)
            {
                MyRightButton_X_Action.performed += OnMyRightButton_X_Action;
            }
            var MyRightButton_Y_Action = GetInputAction(MyRightButton_Y);
            if (MyRightButton_Y_Action != null)
            {
                MyRightButton_Y_Action.performed += OnMyRightButton_Y_Action;
            }

        }
        void TeardownInteractorEvents()
        {
            var MyLeftButton_X_Action = GetInputAction(MyLeftTrigger);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed -= OnMyLeftButton_X_Action;
            }
            var MyLeftButton_Y_Action = GetInputAction(MyLeftButton_Y);
            if (MyLeftButton_Y_Action != null)
            {
                MyLeftButton_Y_Action.performed -= OnMyLeftButton_Y_Action;
            }
            var MyRightButton_X_Action = GetInputAction(MyRightTrigger);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed -= OnMyRightButton_X_Action;
            }
            var MyRightButton_Y_Action = GetInputAction(MyLeftButton_Y);
            if (MyLeftButton_Y_Action != null)
            {
                MyLeftButton_Y_Action.performed -= OnMyRightButton_Y_Action;
            }
        }

        /// <summary>
        /// ����X��
        /// </summary>
        /// <param name="context"></param>
        private void OnMyLeftButton_X_Action(InputAction.CallbackContext context)
        {
            Fire();
        }
        /// <summary>
        /// ����Y��
        /// </summary>
        /// <param name="context"></param>
        private void OnMyLeftButton_Y_Action(InputAction.CallbackContext context)
        {
            Debug.Log("��������Y��--------------------");
        }


        /// <summary>
        /// ����X��
        /// </summary>
        /// <param name="context"></param>
        private void OnMyRightButton_X_Action(InputAction.CallbackContext context)
        {
            Fire();
        }
        /// <summary>
        /// ����Y��
        /// </summary>
        /// <param name="context"></param>
        private void OnMyRightButton_Y_Action(InputAction.CallbackContext context)
        {
            Debug.Log("��������B��--------------------");
        }

        private void Fire()
        {
            if (transform == null)
            {
                Debug.Log("over");
            }
            Quaternion bulletRotation = Quaternion.LookRotation(transform.forward);
            Debug.Log("fired started");
            GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward * 0.6f, bulletRotation);
            if (bulletPrefab == null)
            {
                Debug.LogError("no bullet");
                return;
            }
            // ���ӵ���ǰ����
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * bulletSpeed;
            }

            //Destroy(bullet, 3f);
        }
        
        static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
    }
}
