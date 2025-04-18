using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Random = UnityEngine.Random;

/// <summary>
/// This class listens to the select event of an XRI interactable, then sets the
/// avatar to a simple example avatar and gives it a random shape. It does not
/// need to send any messages or listen to peer events etc because it uses the
/// AvatarManager to swap prefab and the SimpleShapeAvatar class to swap shape.
///
/// SimpleShapeAvatar has extensive comments, go have a look!
/// </summary>
namespace Ubiq.Samples
{
    public class ChangeShapeMy : MonoBehaviour
    {
        public GameObject prefab;
        public float maxInteractionDistance = 2f;
        public AudioClip ClickSound;
        private AudioSource audioSource;

        private XRSimpleInteractable interactable;
        private RoomClient roomClient;
        private AvatarManager avatarManager;
        private IXRSelectInteractor currentInteractor;



        private void Start()
        {
            //AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;


            // Connect up the event for the XRI button.
            interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(Interactable_SelectEntered);

            var networkScene = NetworkScene.Find(this);
            roomClient = networkScene.GetComponentInChildren<RoomClient>();
            avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        }

        private void OnDestroy()
        {
            // Cleanup the event for the XRI button so it does not get called after
            // we have been destroyed.
            if (interactable)
            {
                interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
            }
        }

        private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
        {
            // The button has been pressed.

            // Change the local avatar prefab to the simple example prefab. The
            // AvatarManager will do the work of letting other peers know about the
            // prefab change.


            // Also, set the shape to a new, random one. We use a coroutine to
            // wait one frame to allow the AvatarManager time to spawn the new
            // prefab.
            var role = FindObjectOfType<GameManager>().myRole;
            if (role != "catcher")
            {
                currentInteractor = arg0.interactorObject;
                var interactorTransform = (currentInteractor as MonoBehaviour)?.transform;

                if (interactorTransform != null)
                {
                    float distance = Vector3.Distance(interactorTransform.position, transform.position);
                    if (distance > maxInteractionDistance)
                    {
                        Debug.Log("Too far to interact.");
                        return;
                    }
                }

                // play the sound
                if (ClickSound && audioSource)
                {
                    audioSource.PlayOneShot(ClickSound);
                }


                avatarManager.avatarPrefab = prefab;
                StartCoroutine(SetRandomShape());
            }
        }

        // This is a coroutine. They can be used in Unity to spread work out over 
        // multiple frames. They can be paused with a 'yield' instruction. When
        // the yield ends, they will pick up again wherever they left off.
        private IEnumerator SetRandomShape()
        {
            // We wait a few frames to allow the prefab to be spawned and to
            // initialise itself.
            yield return null;
            yield return null;

            while (true)
            {
                if (!avatarManager)
                {
                    // Yield break ends the coroutine.
                    yield break;
                }

                var avatar = avatarManager.FindAvatar(roomClient.Me);
                if (avatar)
                {
                    var shapeAvatar = avatar.GetComponentInChildren<SimpleShapeAvatar>();
                    if (shapeAvatar)
                    {
                        if (shapeAvatar.shapes.Length <= 1)
                        {
                            yield break;
                        }


                        // Set the random shape.
                        var randomShape = shapeAvatar.shapes[
                            Random.Range(0, shapeAvatar.shapes.Length)];
                        while (randomShape == shapeAvatar.currentShape)
                        {
                            randomShape = shapeAvatar.shapes[
                                Random.Range(0, shapeAvatar.shapes.Length)];
                        }

                        shapeAvatar.SetShape(randomShape);

                        // Hide local avatar to prevent self-occlusion
                        var avatarGO = avatar.gameObject;
                        int localLayer = LayerMask.NameToLayer("LocalAvatar");

                        void SetLayerRecursively(GameObject obj, int layer)
                        {
                            obj.layer = layer;
                            foreach (Transform child in obj.transform)
                            {
                                SetLayerRecursively(child.gameObject, layer);
                            }
                        }

                        SetLayerRecursively(avatarGO, localLayer);

                        // End the coroutine.
                        yield break;
                    }
                }

                // Yield return null pauses the coroutine until the next frame. We
                // wait a few frames to allow the prefab to be spawned and to
                // initialise itself.
                yield return null;
                yield return null;
            }
        }
    }
}
