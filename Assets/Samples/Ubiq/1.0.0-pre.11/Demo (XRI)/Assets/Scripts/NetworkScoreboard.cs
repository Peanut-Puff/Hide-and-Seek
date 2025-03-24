using Ubiq.Messaging;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Ubiq.Spawning;
using Ubiq.Geometry;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

namespace Ubiq.Samples
{
    public class NetworkScoreboard : MonoBehaviour
    {
        private NetworkContext context;

        public TextMeshProUGUI hiderScoreText;
        public TextMeshProUGUI catcherScoreText;
        public TextMeshProUGUI timerText;

        private int hiderScore = 0;
        private int catcherScore = 0;
        private float timeLeft;


        private void Start()
        {
            enabled = false;
            context = NetworkScene.Register(this);
            UpdateDisplay();
        }

        void Update()
        {
            if (!enabled) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft < 0) timeLeft = 0;

            UpdateDisplay();
        }

        public void StartScoring(float duration = 300f)
        {
            Debug.Log("start scoring");
            hiderScore = 0;
            catcherScore = 0;
            timeLeft = duration;
            enabled = true;
            UpdateDisplay();
        }
        private void FinalDisplay()
        {
            var team = "Catcher";
            if (hiderScore>catcherScore) 
                team="Hider";
            timerText.text = $"{team} Team Wins!";
        }
        public void StopScoring()
        {
            enabled = false;
            FinalDisplay();
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            Debug.Log("receive score");
            var data = message.FromJson<ScoreMessage>();
            if (data.team == "hider") hiderScore += data.amount;
            else if (data.team == "catcher") catcherScore += data.amount;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!enabled) return;
            hiderScoreText.text = $"Hider\n{hiderScore}";
            catcherScoreText.text = $"Catcher\n{catcherScore}";

            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void AddScore(string team, int amount)
        {
            Debug.Log("score");
            if (team == "hider") hiderScore += amount;
            else if (team == "catcher") catcherScore += amount;

            var scoreMessage = new ScoreMessage
            {
                team = team,
                amount = amount
            };
            context.SendJson(scoreMessage);

            UpdateDisplay();
        }

        private struct ScoreMessage
        {
            public string team;
            public int amount;
        }
    }
}