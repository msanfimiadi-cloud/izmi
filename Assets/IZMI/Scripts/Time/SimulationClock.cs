using System;
using UnityEngine;

namespace Izmi
{
    public sealed class SimulationClock : MonoBehaviour
    {
        private static readonly float[] SupportedSpeeds = { 0f, 1f, 5f, 20f };

        [SerializeField] private float gameMinutesPerRealSecond = 1f;
        [SerializeField] private int speedIndex = 1;

        private DateTime currentDate = new DateTime(2026, 8, 26, 8, 0, 0);

        public DateTime CurrentDate => currentDate;
        public float CurrentSpeed => SupportedSpeeds[speedIndex];
        public bool IsPaused => CurrentSpeed <= 0f;

        private void Awake()
        {
            Time.timeScale = CurrentSpeed;
        }

        private void Update()
        {
            var gameMinutes =
                Time.unscaledDeltaTime *
                gameMinutesPerRealSecond *
                CurrentSpeed;

            currentDate = currentDate.AddMinutes(gameMinutes);
        }

        public void SetSpeed(float requestedSpeed)
        {
            for (var index = 0; index < SupportedSpeeds.Length; index++)
            {
                if (Mathf.Approximately(SupportedSpeeds[index], requestedSpeed))
                {
                    speedIndex = index;
                    Time.timeScale = CurrentSpeed;
                    return;
                }
            }
        }
    }
}
