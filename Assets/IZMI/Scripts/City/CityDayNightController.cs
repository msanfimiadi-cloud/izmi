using System;
using System.Collections.Generic;
using UnityEngine;

namespace Izmi
{
    public sealed class CityDayNightController : MonoBehaviour
    {
        private readonly List<Renderer> windows = new List<Renderer>();
        private readonly List<Renderer> streetLights = new List<Renderer>();
        private readonly List<Renderer> emergencyLights = new List<Renderer>();

        private SimulationClock clock;
        private Light sun;
        private Camera worldCamera;
        private float refreshTimer;
        private bool cachedRenderers;

        private static readonly Color NightAmbient = new Color(0.018f, 0.028f, 0.055f);
        private static readonly Color DayAmbient = new Color(0.29f, 0.31f, 0.34f);
        private static readonly Color NightSky = new Color(0.003f, 0.006f, 0.018f);
        private static readonly Color DaySky = new Color(0.32f, 0.48f, 0.64f);

        private void OnEnable()
        {
            clock = FindAnyObjectByType<SimulationClock>();
            worldCamera = Camera.main;
            var sunObject = GameObject.Find("Sun");
            sun = sunObject != null ? sunObject.GetComponent<Light>() : null;
            CacheRenderers();
            ApplyLighting();
        }

        private void Update()
        {
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer < 0.12f)
            {
                return;
            }

            refreshTimer = 0f;
            ApplyLighting();
        }

        private void CacheRenderers()
        {
            windows.Clear();
            streetLights.Clear();
            emergencyLights.Clear();

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                var objectName = renderer.gameObject.name;
                if (objectName.Contains("Window") || objectName.Contains("Medical Cross"))
                {
                    windows.Add(renderer);
                }
                else if (objectName.Contains("Lamp Glow"))
                {
                    streetLights.Add(renderer);
                }
                else if (objectName.Contains("Beacon"))
                {
                    emergencyLights.Add(renderer);
                }
            }

            cachedRenderers = true;
        }

        private void ApplyLighting()
        {
            if (clock == null)
            {
                return;
            }

            if (!cachedRenderers)
            {
                CacheRenderers();
            }

            var hour = (float)clock.CurrentDate.TimeOfDay.TotalHours;
            var solarAngle = (hour - 6f) / 24f * Mathf.PI * 2f;
            var daylight = Mathf.Clamp01(Mathf.Sin(solarAngle) * 1.35f + 0.08f);
            var dusk = 1f - daylight;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(NightAmbient, DayAmbient, daylight);

            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(0.08f, 0.92f, daylight);
                sun.color = Color.Lerp(
                    new Color(0.42f, 0.52f, 0.78f),
                    new Color(1f, 0.92f, 0.78f),
                    Mathf.Clamp01(daylight * 1.8f));
            }

            if (worldCamera != null)
            {
                worldCamera.backgroundColor = Color.Lerp(NightSky, DaySky, daylight);
            }

            var showStreetLights = daylight < 0.38f;
            var showWindows = daylight < 0.68f;
            foreach (var renderer in streetLights)
            {
                if (renderer != null) renderer.enabled = showStreetLights;
            }
            foreach (var renderer in windows)
            {
                if (renderer != null) renderer.enabled = showWindows;
            }
            foreach (var renderer in emergencyLights)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                    var pulse = 0.72f + Mathf.Sin(Time.unscaledTime * 7f) * 0.28f;
                    renderer.transform.localScale = Vector3.one * (0.15f * pulse);
                }
            }
        }

        private void OnDisable()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.025f, 0.035f, 0.06f);
            if (sun != null)
            {
                sun.intensity = 0.82f;
                sun.color = new Color(1f, 0.94f, 0.82f);
            }
            if (worldCamera != null)
            {
                worldCamera.backgroundColor = new Color(0.001f, 0.002f, 0.008f);
            }
        }
    }
}
