using System;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Rendering;
using Ubiq.Rooms;

namespace Ubiq.Samples
{

    public class GetPosition : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public Vector3 targetPosition;
        void Start()
        {
            targetPosition = transform.position;

        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public Vector3 getTargetPosition()
        {
            targetPosition = transform.position;
            Debug.Log($"I am {targetPosition}");
            return targetPosition;
        }

    }
}
