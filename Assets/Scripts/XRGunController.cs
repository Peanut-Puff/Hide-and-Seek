using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRGunController : MonoBehaviour
{
    public GameObject bulletPrefab;  // �ӵ�Ԥ����
    public Transform bulletSpawnPoint; // �ӵ�����λ�ã�ǹ�ڣ�
    public float bulletSpeed = 50f;  // �ӵ��ٶ�
    public XRRayInteractor rayInteractor; // XR���߽�����
    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        // ��ȡ XRGrabInteractable ���
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // ������ץȡʱ
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // ��ȡ XRRayInteractor��ȷ��ʹ�õĿ�����֧������
        rayInteractor = args.interactorObject.transform.GetComponent<XRRayInteractor>();

        if (rayInteractor != null)
        {
            // ����������Trigger��
            rayInteractor.selectEntered.AddListener(Fire);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (rayInteractor != null)
        {
            // ȡ������
            rayInteractor.selectEntered.RemoveListener(Fire);
            rayInteractor = null;
        }
    }

    private void Fire(SelectEnterEventArgs args)
    {
        if (bulletPrefab == null || bulletSpawnPoint == null || rayInteractor == null)
            return;

        // **ֱ��ʹ�����߷���**
        Vector3 fireDirection = rayInteractor.transform.forward;

        // �����ӵ�
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(fireDirection));

        // ���ӵ���Ӹ��壬��ʩ����
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = fireDirection * bulletSpeed;
        }

        // 3��������ӵ�
        Destroy(bullet, 3f);
    }
}
