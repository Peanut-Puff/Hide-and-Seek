using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ubiq.Spawning;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
#endif

namespace Ubiq.Samples
{
    /// <summary>
    /// The Fireworks Box is a basic interactive object. This object uses the
    /// NetworkSpawner to create shared objects (fireworks). The Box can be
    /// grasped and moved around, but note that the Box itself is *not* network
    /// enabled. Each player has their own copy.
    /// </summary>
    public class LaserGunSpawner : MonoBehaviour
    {
        public GameObject gunPrefeb;

#if XRI_3_0_7_OR_NEWER
        private NetworkSpawnManager spawnManager;
        private XRSimpleInteractable interactable;
        private XRInteractionManager interactionManager;

        private void Start()
        {
            enabled=false;
            spawnManager = NetworkSpawnManager.Find(this);
            interactable = GetComponent<XRSimpleInteractable>();
            interactionManager = interactable.interactionManager;

            interactable.selectEntered.AddListener(Gun_XRGrabInteractable_SelectEntered);
        }

        private void OnDestroy()
        {
            interactable.selectEntered.RemoveListener(Gun_XRGrabInteractable_SelectEntered);
        }

        private void Gun_XRGrabInteractable_SelectEntered(SelectEnterEventArgs eventArgs)
        {
            if(!enabled)
                return;
            var role=FindObjectOfType<GameManager>().myRole;
            if (role=="hider"){
                return;
            }
            var go = spawnManager.SpawnWithPeerScope(gunPrefeb);
            var gun = go.GetComponent<LaserGun>();
            gun.transform.position = transform.position;
            gun.owner = true;            
            gun.iscatched = true;

            if (!interactionManager)
            {
                return;
            }

            // Force the interactor(hand) to stop selecting the box and select the gun
            var selectInteractor = eventArgs.interactorObject;
            if (selectInteractor != null)
            {
                interactionManager.SelectExit(
                    selectInteractor,
                    this.interactable);
                interactionManager.SelectEnter(
                    selectInteractor,
                    gun.GetComponent<XRGrabInteractable>());
            }
        }
#endif
    }
}