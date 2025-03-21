using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Ubiq.Samples
{
    public class LongPress : MonoBehaviour
    {
        public GameObject fireworkPrefab;
        public UnityEngine.UI.Image holdProgressImage;
        private float selectTimer = 0f;
        private float selectThreshold = 2f;
        private bool isSelecting = false;
        private NetworkSpawnManager spawnManager;
        private IXRSelectInteractor currentInteractor;
        private XRSimpleInteractable interactable;
        private XRInteractionManager interactionManager;

        private void Start()
        {
            spawnManager = NetworkSpawnManager.Find(this);
            interactable = GetComponent<XRSimpleInteractable>();
            interactionManager = interactable.interactionManager;

            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }

        private void OnDestroy()
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Debug.Log("1111");
            isSelecting = true;
            selectTimer = 0f;
            currentInteractor = args.interactorObject as IXRSelectInteractor;
            if (currentInteractor != null)
                print("get!");

        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            Debug.Log("2222");
            isSelecting = false;
            selectTimer = 0f;
            currentInteractor = null;
        }

        private void Update()
        {
            if (isSelecting)
            {
                selectTimer += Time.deltaTime;
                if (holdProgressImage != null)
                {
                    holdProgressImage.fillAmount = selectTimer / selectThreshold;
                }

                if (selectTimer >= selectThreshold)
                {
                    Debug.Log("Longpressed");

                    isSelecting = false;
                    selectTimer = 0f;

                    if (holdProgressImage != null)
                    {
                        holdProgressImage.fillAmount = 0f;
                    }

                    if (spawnManager != null && fireworkPrefab != null)
                    {
                        var go = spawnManager.SpawnWithPeerScope(fireworkPrefab);
                        var firework = go.GetComponent<Firework>();
                        firework.transform.position = transform.position + Vector3.up * 0.2f;
                        firework.owner = true;
                        if (currentInteractor != null)
                        {
                            interactionManager.SelectEnter(
                                currentInteractor,
                                firework.GetComponent<XRGrabInteractable>());
                        }
                    }
                }
            }
            else
            {
                if (holdProgressImage != null)
                {
                    holdProgressImage.fillAmount = 0f;
                }
            }
        }
    }
}
