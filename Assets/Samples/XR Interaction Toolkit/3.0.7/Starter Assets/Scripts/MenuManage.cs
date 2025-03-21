
using UnityEngine;
using UnityEngine.InputSystem;

namespace TWQ
{
    public class MenuManage : MonoBehaviour
    {
        public GameObject bulletPrefab; 
        //public Transform muzzleTransform; // ǹ��λ��
        public float bulletSpeed = 50f; // 
        public InputActionReference MyLeftButton_X;
        public InputActionReference MyLeftButton_Y;

        public InputActionReference MyRightButton_X;
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
            var MyLeftButton_X_Action = GetInputAction(MyLeftButton_X);
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
            var MyRightButton_X_Action = GetInputAction(MyRightButton_X);
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
            var MyLeftButton_X_Action = GetInputAction(MyLeftButton_X);
            if (MyLeftButton_X_Action != null)
            {
                MyLeftButton_X_Action.performed -= OnMyLeftButton_X_Action;
            }
            var MyLeftButton_Y_Action = GetInputAction(MyLeftButton_Y);
            if (MyLeftButton_Y_Action != null)
            {
                MyLeftButton_Y_Action.performed -= OnMyLeftButton_Y_Action;
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
            Quaternion bulletRotation = Quaternion.LookRotation(transform.forward);
            Debug.Log("fired started");
            // ��ǹ��λ�������ӵ�
            GameObject bullet = Instantiate(bulletPrefab, transform.position+ transform.forward * 0.6f, bulletRotation);
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

            // һ��ʱ��������ӵ�����ֹռ���ڴ�
            Destroy(bullet, 3f);
        }

        
        static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
    }
}
