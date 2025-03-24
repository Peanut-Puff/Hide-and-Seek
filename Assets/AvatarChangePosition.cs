using System;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ubiq.Samples
{
    public class AvatarChangePosition : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private GameManager gameManager;
       
        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
