using System;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ubiq.Samples
{
    public class WallChange : MonoBehaviour
    {
        private GameManager gameManager;


        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        private void OnDestroy()
        {
 
        }

        private void Update()
        {

            if (gameManager == null)
            {
                return;
            }

            bool gameStarted = gameManager.gameStarted;

            gameObject.SetActive(!gameStarted);

        }

        
    }
}
