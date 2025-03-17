using UnityEngine;

namespace PixelGunsPack
{
    public class GunRotationDemo : MonoBehaviour
    {
        [Range(0, 100)]
        [SerializeField] private float rotateSpeed = 50;

        private void Update()
        {
            transform.Rotate(transform.up * -rotateSpeed * Time.deltaTime);
        }
    }
}
