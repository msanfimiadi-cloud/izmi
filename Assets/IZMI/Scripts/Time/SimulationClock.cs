using System;
using UnityEngine;

namespace Izmi
{
    public sealed class SimulationClock : MonoBehaviour
    {
        private const string DateTicksKey = "IZMI.World.DateTicks";
        private const string SavedUtcTicksKey = "IZMI.World.SavedUtcTicks";
        private const float OfflineGameMinutesPerRealMinute = 6f;
        private const double MaximumOfflineRealMinutes = 60d * 24d * 7d;
        private static readonly float[] SupportedSpeeds = { 0f, 1f, 5f, 20f };

        [SerializeField] private float gameMinutesPerRealSecond = 1f;
        [SerializeField] private int speedIndex = 1;

        private DateTime currentDate = new DateTime(2026, 8, 26, 8, 0, 0);
        private float autosaveTimer;

        public DateTime CurrentDate => currentDate;
        public float CurrentSpeed => SupportedSpeeds[speedIndex];
        public bool IsPaused => CurrentSpeed <= 0f;
        public double LastOfflineAdvanceMinutes { get; private set; }

        private void Awake()
        {
            LoadWorldTime();
            Time.timeScale = CurrentSpeed;
        }

        private void Update()
        {
            var gameMinutes =
                Time.unscaledDeltaTime *
                gameMinutesPerRealSecond *
                CurrentSpeed;
            currentDate = currentDate.AddMinutes(gameMinutes);

            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= 5f)
            {
                autosaveTimer = 0f;
                SaveWorldTime();
            }
        }

        public void SetSpeed(float requestedSpeed)
        {
            for (var index = 0; index < SupportedSpeeds.Length; index++)
            {
                if (Mathf.Approximately(SupportedSpeeds[index], requestedSpeed))
                {
                    speedIndex = index;
                    Time.timeScale = CurrentSpeed;
                    SaveWorldTime();
                    return;
                }
            }
        }

        private void LoadWorldTime()
        {
            var storedDate = PlayerPrefs.GetString(DateTicksKey, string.Empty);
            var storedUtc = PlayerPrefs.GetString(SavedUtcTicksKey, string.Empty);
            if (!long.TryParse(storedDate, out var dateTicks) ||
                !long.TryParse(storedUtc, out var utcTicks))
            {
                return;
            }

            try
            {
                currentDate = new DateTime(dateTicks);
                var savedUtc = new DateTime(utcTicks, DateTimeKind.Utc);
                var realMinutes = Math.Min(
                    Math.Max(0d, (DateTime.UtcNow - savedUtc).TotalMinutes),
                    MaximumOfflineRealMinutes);
                LastOfflineAdvanceMinutes = realMinutes * OfflineGameMinutesPerRealMinute;
                currentDate = currentDate.AddMinutes(LastOfflineAdvanceMinutes);
            }
            catch (ArgumentOutOfRangeException)
            {
                currentDate = new DateTime(2026, 8, 26, 8, 0, 0);
                LastOfflineAdvanceMinutes = 0d;
            }
        }

        private void SaveWorldTime()
        {
            PlayerPrefs.SetString(DateTicksKey, currentDate.Ticks.ToString());
            PlayerPrefs.SetString(SavedUtcTicksKey, DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveWorldTime();
            }
        }

        private void OnApplicationQuit()
        {
            SaveWorldTime();
        }
    }
}
