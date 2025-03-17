using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bulletPrefab;  // 子弹预制体
    public Transform shooterHead;    // 子弹发射点
    public float bulletSpeed = 20f;  // 子弹速度
    private bool isGunVisible = true; // 记录枪械是否可见

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 监听空格键发射
        {
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.Z)) // 监听 Z 键切换枪械可见状态
        {
            ToggleGunVisibility();
        }

    }

    void Fire()
    {
        if (bulletPrefab == null || shooterHead == null)
        {
            Debug.LogError("Bullet Prefab or Shooter Head is not assigned!");
            return;
        }

        Quaternion bulletRotation = Quaternion.LookRotation(transform.forward);
        GameObject bullet = Instantiate(bulletPrefab, shooterHead.position, bulletRotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }

        Destroy(bullet, 3f);
    }

    void ToggleGunVisibility()
    {
        // 切换状态

        isGunVisible = !isGunVisible;
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.enabled = isGunVisible; // 仅隐藏可见部分
        }


        // 方法 2：仅隐藏枪械的 Mesh，而不影响脚本
        // Renderer renderer = GetComponent<Renderer>();
        // if (renderer != null) renderer.enabled = isGunVisible;
    }
}
