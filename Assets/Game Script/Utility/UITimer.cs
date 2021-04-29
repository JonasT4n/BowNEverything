using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace BNEGame
{
    public class UITimer : MonoBehaviour
    {
        public static event Action OnTimerEnd;

        [SerializeField] private Text mainTimerText = null;
        [SerializeField] private float startingDuration = 300f;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float currentTimeLeft;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isRunning = false;

        public float StartingDuration { set => startingDuration = value; }

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Initial display time
            UpdateDisplay();

            // Subscribe events
            EventHandler.OnGameStartedEvent += HandleStartTimerEvent;
            EventHandler.OnGameEndedEvent += HandleStopTimerEvent;
        }

        private void FixedUpdate()
        {
            // Check running timer
            if (isRunning)
            {
                currentTimeLeft -= Time.deltaTime;
                UpdateDisplay();

                // Check timer ended
                if (currentTimeLeft <= 0f)
                {
                    ActivateTimer(false);
                    OnTimerEnd?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            EventHandler.OnGameStartedEvent -= HandleStartTimerEvent;
            EventHandler.OnGameEndedEvent -= HandleStopTimerEvent;
        }
        #endregion

        #region Event Methods
        private void HandleStartTimerEvent(GameStartedEventArgs args)
        {
            currentTimeLeft = startingDuration;
            ActivateTimer(true);
        }

        private void HandleStopTimerEvent(GameEndedEventArgs args)
        {
            currentTimeLeft = 0f;
            ActivateTimer(false);
        }
        #endregion

        private void UpdateDisplay()
        {
            int minutes = currentTimeLeft > 60f ? Mathf.CeilToInt(currentTimeLeft / 60f) : 0;
            int seconds = Mathf.CeilToInt(currentTimeLeft % 60f);

            // Display timer
            if (mainTimerText != null)
            {
                if (minutes == 0)
                {
                    mainTimerText.text = $"{seconds}";
                }
                else
                {
                    mainTimerText.text = $"{minutes}:{seconds}";
                }
            }
        }

        public void ResetTimer()
        {
            currentTimeLeft = startingDuration;
            ActivateTimer(false);
            UpdateDisplay();
        }

        public void ActivateTimer(bool active)
        {
            isRunning = active;
        }
    }

}