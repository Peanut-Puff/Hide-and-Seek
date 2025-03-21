using TMPro;
using UnityEngine;

public class ScoreboardController : MonoBehaviour
{
    public TextMeshProUGUI hiderScoreText;
    public TextMeshProUGUI catcherScoreText;
    public TextMeshProUGUI timerText;

    public int hiderScore = 0;
    public int catcherScore = 0;
    public float gameTime = 300f; // 5 minutes

    void Update()
    {
        gameTime -= Time.deltaTime;
        if (gameTime < 0f) gameTime = 0f;

        // 格式化时间
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        timerText.text = $"Time Left: {minutes:00}:{seconds:00}";

        // 显示分数
        hiderScoreText.text = $"Hider: {hiderScore}";
        catcherScoreText.text = $"Catcher: {catcherScore}";
    }

    public void AddScore(string team, int amount)
    {
        if (team == "hider") hiderScore += amount;
        else if (team == "catcher") catcherScore += amount;
    }
}