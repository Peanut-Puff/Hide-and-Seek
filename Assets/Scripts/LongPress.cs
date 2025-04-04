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
        public float maxInteractionDistance = 2.5f;
        private float selectTimer = 0f;
        private float selectThreshold = 5f;
        private bool isSelecting = false;
        private NetworkSpawnManager spawnManager;
        private IXRSelectInteractor currentInteractor;
        private XRSimpleInteractable interactable;
        private XRInteractionManager interactionManager;
        public float duration=20f;

        private void Start()
        {
            enabled = false;
            holdProgressImage.fillAmount = 0f;
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
            if (!enabled)
                return;
            // Debug.Log("select");
            isSelecting = true;
            selectTimer = 0f;
            currentInteractor = args.interactorObject as IXRSelectInteractor;
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (!enabled)
                return;
            // Debug.Log("select exit");
            isSelecting = false;
            selectTimer = 0f;
            currentInteractor = null;
        }

        private void Update()
        {
            if (!enabled)
                return;
            var role = FindObjectOfType<GameManager>().myRole;
            if (role == "catcher")
                return;
            if (currentInteractor != null)
            {
                var interactorTransform = (currentInteractor as MonoBehaviour)?.transform;
                if (interactorTransform != null)
                {
                    float distance = Vector3.Distance(interactorTransform.position, transform.position);
                    if (distance > maxInteractionDistance)
                    {
                        isSelecting = false;
                        selectTimer = 0f;
                        holdProgressImage.fillAmount = 0f;
                        return;
                    }
                    // Debug.Log("close");
                }
            }
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
                    FindObjectOfType<NetworkScoreboard>().AddScore("hider", 1);
                    isSelecting = false;
                    selectTimer = 0f;
                    StartCoroutine(DisableMachineTemporarily());

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
        private IEnumerator DisableMachineTemporarily()
        {
            enabled = false;
            yield return new WaitForSeconds(duration);
            enabled = true;
        }
    }
}
