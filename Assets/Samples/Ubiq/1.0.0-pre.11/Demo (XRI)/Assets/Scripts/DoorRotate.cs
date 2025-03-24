using System;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Rendering;
using Ubiq.Rooms;

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


        private void Start()
        {
            closedRotation = transform.rotation;
            openRotation = closedRotation * Quaternion.Euler(0f, rotationAngle, 0f);

            gameManager = FindObjectOfType<GameManager>();
            avatarManager = FindObjectOfType<AvatarManager>();


        }

        private void Update()
        {
            if (gameManager == null || avatarManager == null)
            {
                return;
            }
            bool currentGameStarted = gameManager.gameStarted;

            Quaternion targetRotation = gameManager.gameStarted ? openRotation : closedRotation;
            if (gameManager.gameStarted)
            {
                count = 1;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            lastGameStarted = currentGameStarted;


        }


    }
}
