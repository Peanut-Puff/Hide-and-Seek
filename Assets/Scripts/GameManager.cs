using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;
using System.Collections.Generic;
using TMPro;

namespace Ubiq.Samples
{
    public class GameManager : MonoBehaviour
    {
        private XRSimpleInteractable startGameButton;
        private XRSimpleInteractable resetGameButton;
        public TeamAssigner teamAssigner;
        public ShowName showname;
        public NetworkScoreboard networkScoreboard;
        public GunSpawner gunSpawner;
        public LaserGunSpawner laserGunSpawner;
        public LongPress fixMachine1;
        public LongPress fixMachine2;
        public LongPress fixMachine3;
        public LongPress fixMachine4;
        public bool gameStarted = false;
        //Audio
        public AudioClip GameOverSound;
        private AudioSource audioSource;

        //return effect
        public GameObject TeleportEffectPrefab;
        //wall
        public GameObject WallPrefab;

        private RoomClient roomClient;

        public float gameDuration;
        public float waitTime;
        private NetworkContext context;
        public string myRole;
        public float maxInteractionDistance = 1.5f;

        private struct GameMessage
        {
            public string type;
            public float duration;
        }


        public GetPosition AvatrPositionEnd;
        private AvatarManager avatarManager;
        public GameObject PrefabOrigin;

        private void Start()
        {
            context = NetworkScene.Register(this);

            var interactables = GetComponentsInChildren<XRSimpleInteractable>();
            foreach (var interactable in interactables)
            {
                if (interactable.gameObject.name == "StartButton")
                {
                    startGameButton = interactable;
                    startGameButton.selectEntered.AddListener(OnStartButtonPressed);
                }
                if (interactable.gameObject.name == "ResetButton")
                {
                    resetGameButton = interactable;
                    resetGameButton.selectEntered.AddListener(OnResetButtonPressed);
                }
            }
            var networkScene = NetworkScene.Find(this);
            avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
            roomClient = networkScene.GetComponentInChildren<RoomClient>();
            //AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

        }

        private void OnStartButtonPressed(SelectEnterEventArgs args)
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            if (args.interactorObject is MonoBehaviour mono)
            {
                var interactorTransform = mono.transform;
                float distance = Vector3.Distance(interactorTransform.position, startGameButton.transform.position);
                if (distance > maxInteractionDistance)
                {
                    Debug.Log("Too far to start the game.");
                    return;
                }
            }
            if (avatars.Count > 4)
                return;
            Debug.Log("Start");
            if (gameStarted) return;
            teamAssigner.AssignTeams();
            context.SendJson(new GameMessage { type = "start", duration = gameDuration });
            StartGame();
        }

        private void OnResetButtonPressed(SelectEnterEventArgs args)
        {
            Debug.Log("Resetting game state...");
            if (args.interactorObject is MonoBehaviour mono)
            {
                var interactorTransform = mono.transform;
                float distance = Vector3.Distance(interactorTransform.position, resetGameButton.transform.position);
                if (distance > maxInteractionDistance)
                {
                    Debug.Log("Too far to reset the game.");
                    return;
                }
            }
            context.SendJson(new GameMessage { type = "reset" });
            StopAllCoroutines();
            DisableAll();
            networkScoreboard.StopScoring(false);
        }
        private void StartGame()
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            gameStarted = true;
            StartCoroutine(DisableButtonTemporarily());
            StartCoroutine(EnableGunPick());
            StartCoroutine(EnableFix());
            showname.StartLink();
            networkScoreboard.StartScoring(gameDuration);
            var myUuid = RoomClient.Find(this).Me.uuid;
            foreach (var avatar in avatars)
            {
                if (avatar.Peer?.uuid == myUuid)
                {
                    var roleComp = avatar.GetComponent<AvatarRole>();
                    Debug.Log($"I am {roleComp.role}");
                    myRole = roleComp.role;
                    // here

                    if (myRole == "catcher")
                    {
                        StartCoroutine(EnableWallTemporarily());
                    }

                    break;
                }
            }
        }
        private IEnumerator EnableWallTemporarily()
        {
            WallPrefab.SetActive(true);

            TextMeshProUGUI hintText = WallPrefab.transform.Find("Collider/Canvas/CatcherHint").GetComponent<TextMeshProUGUI>();

            hintText.gameObject.SetActive(true);
            float remainingTime = waitTime;

            startGameButton.enabled = false;

            while (remainingTime > 0)
            {
                if (remainingTime>=0.5)
                {
                    hintText.text = "You are the catcher!\n\nYou can pick the gun\nor enter the park\nafter " + remainingTime.ToString("F0") + " s!";
                    
                }
                else
                {
                    hintText.text = "Go!";
                }
                yield return null;
                remainingTime -= Time.deltaTime;
            }

            startGameButton.enabled = true;

            hintText.text = "";
            hintText.gameObject.SetActive(false);
            WallPrefab.SetActive(false);
        }

        private IEnumerator EnableFix()
        {
            yield return new WaitForSeconds(waitTime);
            fixMachine1.enabled = true;
            fixMachine2.enabled = true;
            fixMachine3.enabled = true;
            fixMachine4.enabled = true;
        }
        private IEnumerator EnableGunPick()
        {
            yield return new WaitForSeconds(waitTime);
            gunSpawner.enabled = true;
            laserGunSpawner.enabled = true;
        }
        private void DisableAll()
        {
            gameStarted = false;
            WallPrefab.SetActive(false);

            showname.ResetNameBoard();
            startGameButton.enabled = true;

            // play the sound
            if (GameOverSound && audioSource)
            {
                audioSource.volume = 1.0f;
                audioSource.PlayOneShot(GameOverSound);
            }

            StartCoroutine(ReturnBack());

            gunSpawner.enabled = false;
            laserGunSpawner.enabled = false;
            fixMachine1.enabled = false;
            fixMachine2.enabled = false;
            fixMachine3.enabled = false;
            fixMachine4.enabled = false;
            

        }

        private IEnumerator ReturnBack()
        {
            string myName = roomClient.Me[DisplayNameManager.KEY];
            var avatars_all = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            for (int i = 0; i < avatars_all.Count; i++)
            {
                var avatar0 = avatars_all[i];
                var name = avatar0.Peer[DisplayNameManager.KEY];
                if (name == myName)
                {
                    Vector3 effectPosition = AvatrPositionEnd.transform.position;

                    float timer = 0f;
                    GameObject effect = Instantiate(TeleportEffectPrefab, effectPosition, Quaternion.Euler(-90f, 0f, 0f));
                    resetGameButton.enabled=false;
                    while (timer < 2f)
                    {
                        startGameButton.enabled = false;
                        AvatrPositionEnd.transform.position = effectPosition;
                        timer += Time.deltaTime;
                        yield return null;
                    }
                    startGameButton.enabled = true;
                    Destroy(effect);
                    resetGameButton.enabled=true;

                    float fi = (float)i - 3f;
                    AvatrPositionEnd.transform.position = new Vector3(fi, 0f, -3.25f);
                    avatarManager.avatarPrefab = PrefabOrigin;
                    break;
                }
            }     
        }


        private IEnumerator DisableButtonTemporarily()
        {
            startGameButton.enabled = false;
            yield return new WaitForSeconds(gameDuration);
            DisableAll();
            networkScoreboard.StopScoring();
        }

        private void OnDestroy()
        {
            startGameButton.selectEntered.RemoveListener(OnStartButtonPressed);
            if (resetGameButton != null)
            {
                resetGameButton.selectEntered.RemoveListener(OnResetButtonPressed);
            }
        }

        public void ProcessMessage(Ubiq.Messaging.ReferenceCountedSceneGraphMessage message)
        {
            var msg = message.FromJson<GameMessage>();

            if (msg.type == "start" && !gameStarted)
            {
                var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
                if (avatars.Count > 4)
                    return;
                StartGame();
            }
            else if (msg.type == "reset")
            {
                StopAllCoroutines();
                DisableAll();
                networkScoreboard.StopScoring(false);
                Debug.Log("Game reset by network message");
            }
        }
    }
}