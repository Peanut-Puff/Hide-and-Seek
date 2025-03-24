using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;
using Ubiq.Spawning;
using System.Linq;
using System.Collections.Generic;

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
        public GetPosition AvatrPositionEnd;
        public LaserGunSpawner laserGunSpawner;
        public LongPress fixMachine1;
        public LongPress fixMachine2;
        public LongPress fixMachine3;
        public LongPress fixMachine4;
        public bool gameStarted = false;

        private float duration = 23f;
        private NetworkContext context;
        public string myRole;
        
        //
        private AvatarManager avatarManager;
        public GameObject PrefabOrigin;


        private struct GameStartMessage
        public float gameDuration;
        public float waitTime;
        private NetworkContext context;
        public string myRole;
        private struct GameMessage
        {
            public string type;
            public float duration;
        }

        private void Start()
        {
            context = NetworkScene.Register(this);
            startGameButton = GetComponent<XRSimpleInteractable>();
            startGameButton.selectEntered.AddListener(OnStartButtonPressed);

            //
            var networkScene = NetworkScene.Find(this);
            avatarManager = networkScene.GetComponentInChildren<AvatarManager>();

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
        }

        private void OnStartButtonPressed(SelectEnterEventArgs args)
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
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
                    break;
                }
            }
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

            //AvatrPositionEnd.targetPosition = transform.position;
            //Vector3 mm = AvatrPositionEnd.targetPosition
            //Vector3 NowPosition = AvatrPositionEnd.getTargetPosition();
            //(0,0,-2.25)

            AvatrPositionEnd.transform.position = new Vector3(0f, 0f, -2.25f);
            avatarManager.avatarPrefab = PrefabOrigin;


            showname.ResetNameBoard();
            startGameButton.enabled = true;
            gunSpawner.enabled = false;
            laserGunSpawner.enabled = false;
            fixMachine1.enabled = false;
            fixMachine2.enabled = false;
            fixMachine3.enabled = false;
            fixMachine4.enabled = false;

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