using Ubiq.Messaging;
using UnityEngine;
using TMPro;

public class NetworkScoreboard : MonoBehaviour
{
    private NetworkContext context;

    public TextMeshProUGUI hiderScoreText;
    public TextMeshProUGUI catcherScoreText;
    public TextMeshProUGUI timerText;

    private int hiderScore = 0;
    private int catcherScore = 0;
    private float timeLeft = 300f;

    public bool isHost = true;

    private void Start()
    {
        context = NetworkScene.Register(this);
        UpdateDisplay();
    }

    void Update()
    {
        if (!isHost) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0) timeLeft = 0;

        SendScore(); // host 同步分数
        UpdateDisplay();
    }

    private void SendScore()
    {
        var message = new ScoreMessage
        {
            hider = hiderScore,
            catcher = catcherScore,
            time = timeLeft
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var data = message.FromJson<ScoreMessage>();

        hiderScore = data.hider;
        catcherScore = data.catcher;
        timeLeft = data.time;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        hiderScoreText.text = $"Hider\n{hiderScore}";
        catcherScoreText.text = $"Catcher\n{catcherScore}";

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void AddScore(string team, int amount)
    {
        if (team == "hider") hiderScore += amount;
        else if (team == "catcher") catcherScore += amount;
    }

    private struct ScoreMessage
    {
        public int hider;
        public int catcher;
        public float time;
    }
}