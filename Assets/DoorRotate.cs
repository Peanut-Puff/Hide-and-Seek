using System;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ubiq.Samples
{
    public class DoorRotate : MonoBehaviour
    {
        private GameManager gameManager;
        private Quaternion initialRotation;
        private Quaternion targetRotation;
        private bool rotating = false;

        public float rotationSpeed = 90f;
        public float rotationAngle = -150f;

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            initialRotation = transform.rotation;
            targetRotation = initialRotation * Quaternion.Euler(0, rotationAngle, 0); 
        }

        private void Update()
        {
            if (!gameManager)
            {
                return;
            }

            if (gameManager.gameStarted && !rotating)
            {
                rotating = true;
            }

            if (rotating)
            {

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);


                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
                {
                    transform.rotation = targetRotation;
                    rotating = false;
                }
            }
        }
    }
}
