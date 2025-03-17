using UnityEngine;

namespace PixelGunsPack
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Vector3[] camPositions;
        [SerializeField] private float lerpSpeed = 4f;
        [SerializeField] private int index;

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, camPositions[index], lerpSpeed * Time.deltaTime);
        }

        public void IncreaseIndex()
        {
            if (index >= 5)
                index = 5;
            else
                index++;
        }
        public void DecreaseIndex()
        {
            if (index <= 0)
                index = 0;
            else
                index--;
        }
    }
}
