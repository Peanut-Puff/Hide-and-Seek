using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRGunController : MonoBehaviour
{
    public GameObject bulletPrefab;  // 子弹预制体
    public Transform bulletSpawnPoint; // 子弹生成位置（枪口）
    public float bulletSpeed = 50f;  // 子弹速度
    public XRRayInteractor rayInteractor; // XR射线交互器
    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        // 获取 XRGrabInteractable 组件
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // 监听被抓取时
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // 获取 XRRayInteractor，确保使用的控制器支持射线
        rayInteractor = args.interactorObject.transform.GetComponent<XRRayInteractor>();

        if (rayInteractor != null)
        {
            // 监听按键（Trigger）
            rayInteractor.selectEntered.AddListener(Fire);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (rayInteractor != null)
        {
            // 取消监听
            rayInteractor.selectEntered.RemoveListener(Fire);
            rayInteractor = null;
        }
    }

    private void Fire(SelectEnterEventArgs args)
    {
        if (bulletPrefab == null || bulletSpawnPoint == null || rayInteractor == null)
            return;

        // **直接使用射线方向**
        Vector3 fireDirection = rayInteractor.transform.forward;

        // 生成子弹
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(fireDirection));

        // 给子弹添加刚体，并施加力
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = fireDirection * bulletSpeed;
        }

        // 3秒后销毁子弹
        Destroy(bullet, 3f);
    }
}
