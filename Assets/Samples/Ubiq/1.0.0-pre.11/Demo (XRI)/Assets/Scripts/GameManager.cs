using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using Ubiq.Messaging;


public class GameManager : MonoBehaviour
{
    private XRSimpleInteractable startGameButton;
    public TeamAssigner teamAssigner;
    public Ubiq.Samples.NetworkScoreboard networkScoreboard;
    public bool gameStarted = false;

    private float duration=60f;
    private NetworkContext context;

    private struct GameStartMessage
    {
        public float duration;
    }

    private void Start()
    {
        context = NetworkScene.Register(this);
        startGameButton = GetComponent<XRSimpleInteractable>();
        startGameButton.selectEntered.AddListener(OnStartButtonPressed);
    }

    private void OnStartButtonPressed(SelectEnterEventArgs args)
    {
        Debug.Log("Start");
        StartGame();
    }

    public void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        StartCoroutine(DisableButtonTemporarily());
        teamAssigner.AssignTeams();
        networkScoreboard.StartScoring(duration);
        context.SendJson(new GameStartMessage { duration = duration });
    }

    private IEnumerator DisableButtonTemporarily()
    {
        startGameButton.enabled = false;
        yield return new WaitForSeconds(duration);
        gameStarted=false;
        startGameButton.enabled = true;
        networkScoreboard.StopScoring();
    }

    private void OnDestroy()
    {
        startGameButton.selectEntered.RemoveListener(OnStartButtonPressed);
    }

    public void ProcessMessage(Ubiq.Messaging.ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<GameStartMessage>();
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(DisableButtonTemporarily());
            teamAssigner.AssignTeams();
            networkScoreboard.StartScoring(msg.duration);
        }
    }
}