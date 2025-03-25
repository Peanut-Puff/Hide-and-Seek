using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;
using System.Collections.Generic;

namespace Ubiq.Samples
{
    public class DoorRotate : MonoBehaviour
    {
        private Quaternion closedRotation;          
        private Quaternion openRotation;          
        public float rotationAngle = -150f;        
        public float rotationSpeed = 90f;        

        private GameManager gameManager;
        private AvatarManager avatarManager;

        private bool lastGameStarted = true;
        private List<Ubiq.Avatars.Avatar> avatars;
        private int count = 0;
        private RoomClient roomClient;
        private float timer = 2f;

        private void Start()
        {
            closedRotation = transform.rotation;
            openRotation = closedRotation * Quaternion.Euler(0f, rotationAngle, 0f);

            gameManager = FindObjectOfType<GameManager>();
            avatarManager = FindObjectOfType<AvatarManager>();


        }

        private void Update()
        {
            //if (gameManager == null || avatarManager == null)
            //{
            //    return;
            //}
            //bool currentGameStarted = gameManager.gameStarted;

            //Quaternion targetRotation = gameManager.gameStarted ? openRotation : closedRotation;
            //if (gameManager.gameStarted)
            //{
            //    count = 1;
            //}

            //transform.rotation = Quaternion.RotateTowards(
            //    transform.rotation,
            //    targetRotation,
            //    rotationSpeed * Time.deltaTime
            //);

            //lastGameStarted = currentGameStarted;

            if (gameManager == null || avatarManager == null)
            {
                return;
            }

            bool currentGameStarted = gameManager.gameStarted;

            // 监听游戏结束（下降沿）：从 true → false
            if (lastGameStarted && !currentGameStarted)
            {
                // 游戏刚刚结束，开始延迟旋转
                StartCoroutine(RotateAfterDelay1());
            }

            // 如果游戏开始了，立即旋转
            if (gameManager.gameStarted)
            {
                count = 1;
                Quaternion targetRotation = openRotation;

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            lastGameStarted = currentGameStarted;

        }
        private IEnumerator EnableWallTemporarily()
        {
            
            yield return new WaitForSeconds(timer);
        }

        private IEnumerator RotateAfterDelay1()
        {
            yield return new WaitForSeconds(timer);

            Quaternion targetRotation = closedRotation;

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
                yield return null; 
            }
        }




    }
}
