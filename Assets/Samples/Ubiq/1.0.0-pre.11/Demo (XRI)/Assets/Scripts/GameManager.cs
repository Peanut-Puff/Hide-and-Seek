using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;
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
        public LaserGunSpawner laserGunSpawner;
        public LongPress fixMachine1;
        public LongPress fixMachine2;
        public LongPress fixMachine3;
        public LongPress fixMachine4;
        public bool gameStarted = false;

        private float duration = 300f;
        private NetworkContext context;
        public string myRole;
        private struct GameStartMessage
        {
            public float duration;
        }
        private struct GameResetMessage { }

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
        }

        private void OnStartButtonPressed(SelectEnterEventArgs args)
        {
            var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
            if (avatars.Count > 4)
                return;
            Debug.Log("Start");
            if (gameStarted) return;
            gameStarted = true;
            StartCoroutine(DisableButtonTemporarily());
            StartCoroutine(EnableGunPick());
            StartCoroutine(EnableFix());
            teamAssigner.AssignTeams();
            showname.StartLink();
            networkScoreboard.StartScoring(duration);
            context.SendJson(new GameStartMessage { duration = duration });
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

        private void OnResetButtonPressed(SelectEnterEventArgs args)
        {
            Debug.Log("Resetting game state...");
            context.SendJson(new GameResetMessage());
            StopAllCoroutines();
            gameStarted = false;
            networkScoreboard.StopScoring(false);

            startGameButton.enabled = true;
            gunSpawner.enabled = false;
            laserGunSpawner.enabled = false;
            fixMachine1.enabled = false;
            fixMachine2.enabled = false;
            fixMachine3.enabled = false;
            fixMachine4.enabled = false;
        }

        private IEnumerator EnableFix()
        {
            yield return new WaitForSeconds(20f);
            fixMachine1.enabled = true;
            fixMachine2.enabled = true;
            fixMachine3.enabled = true;
            fixMachine4.enabled = true;
        }
        private IEnumerator EnableGunPick()
        {
            yield return new WaitForSeconds(20f);
            gunSpawner.enabled = true;
            laserGunSpawner.enabled = true;
        }

        private IEnumerator DisableButtonTemporarily()
        {
            startGameButton.enabled = false;
            yield return new WaitForSeconds(duration);
            gameStarted = false;
            startGameButton.enabled = true;
            networkScoreboard.StopScoring();
            gunSpawner.enabled = false;
            laserGunSpawner.enabled = false;
            fixMachine1.enabled = false;
            fixMachine2.enabled = false;
            fixMachine3.enabled = false;
            fixMachine4.enabled = false;
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
            var msg = message.FromJson<GameStartMessage>();
            var json = message.ToString();
            if (!gameStarted)
            {
                var avatars = new List<Ubiq.Avatars.Avatar>(FindObjectsOfType<Ubiq.Avatars.Avatar>());
                if (avatars.Count > 4)
                    return;
                gameStarted = true;
                StartCoroutine(DisableButtonTemporarily());
                StartCoroutine(EnableGunPick());
                StartCoroutine(EnableFix());
                // teamAssigner.AssignTeams();
                showname.StartLink();
                networkScoreboard.StartScoring(msg.duration);
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
            else if (json.Contains("GameResetMessage"))
            {
                StopAllCoroutines();
                gameStarted = false;
                networkScoreboard.StopScoring();

                startGameButton.enabled = true;
                gunSpawner.enabled = false;
                laserGunSpawner.enabled = false;
                fixMachine1.enabled = false;
                fixMachine2.enabled = false;
                fixMachine3.enabled = false;
                fixMachine4.enabled = false;

                Debug.Log("Game reset by network message");
                return;
            }
        }
    }
}