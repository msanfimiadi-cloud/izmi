using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Izmi
{
    public sealed class GlobalOutbreakSystem : MonoBehaviour
    {
        [Serializable]
        public sealed class RegionState
        {
            public string Name;
            public long Population;
            public double Infected;
            public Transform Marker;
            public Renderer MarkerRenderer;
            public Color BaseColor;
        }

        private sealed class Flight
        {
            public Transform Visual;
            public Vector3 Start;
            public Vector3 End;
            public float Progress;
            public float Speed;
        }

        private readonly List<RegionState> regions = new List<RegionState>();
        private readonly List<Flight> flights = new List<Flight>();
        private Transform globe;
        private Material healthyMaterial;
        private Material warningMaterial;
        private Material criticalMaterial;
        private Material routeMaterial;
        private Material flightMaterial;
        private float spreadPulse;
        private int infectedRegions = 1;

        public IReadOnlyList<RegionState> Regions => regions;
        public long TotalPopulation { get; private set; }
        public long TotalInfected { get; private set; }
        public int InfectedRegions => infectedRegions;
        public RegionState SelectedRegion { get; private set; }

        public void Initialize(Transform globeTransform)
        {
            globe = globeTransform;
            CreateMaterials();
            RegisterRegion("Europe", 748000000L, 0d);
            RegisterRegion("Asia", 4780000000L, 0d);
            RegisterRegion("North America", 604000000L, 0d);
            RegisterRegion("First anomaly", 690000000L, 1200d);
            RegisterRegion("Africa", 1520000000L, 0d);
            RegisterRegion("South America", 440000000L, 0d);
            RegisterRegion("Australia", 46000000L, 0d);
            RegisterRegion("Middle East", 500000000L, 0d);

            CreateRoute(0, 1);
            CreateRoute(0, 2);
            CreateRoute(0, 4);
            CreateRoute(1, 3);
            CreateRoute(1, 6);
            CreateRoute(1, 7);
            CreateRoute(2, 5);
            CreateRoute(3, 6);
            CreateRoute(4, 5);
            CreateRoute(4, 7);
            SelectedRegion = regions.Count > 3 ? regions[3] : regions[0];
            RefreshTotals();
        }

        private void Update()
        {
            if (globe == null || regions.Count == 0)
            {
                return;
            }

            HandleRegionSelection();
            if (Time.timeScale <= 0f)
            {
                return;
            }

            SimulateLocalGrowth(Time.deltaTime);
            spreadPulse += Time.deltaTime;
            if (spreadPulse >= 2.4f)
            {
                spreadPulse = 0f;
                SimulateTravelSpread();
            }

            UpdateFlights();
            RefreshVisuals();
            RefreshTotals();
        }

        private void HandleRegionSelection()
        {
            Vector2? pointer = null;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointer = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null &&
                     Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                pointer = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            var camera = Camera.main;
            if (!pointer.HasValue || camera == null)
            {
                return;
            }

            RegionState nearest = null;
            var nearestDistance = 34f;
            foreach (var region in regions)
            {
                var screenPoint = camera.WorldToScreenPoint(region.Marker.position);
                if (screenPoint.z <= 0f)
                {
                    continue;
                }

                var distance = Vector2.Distance(pointer.Value, new Vector2(screenPoint.x, screenPoint.y));
                if (distance < nearestDistance)
                {
                    nearest = region;
                    nearestDistance = distance;
                }
            }

            if (nearest != null)
            {
                SelectedRegion = nearest;
            }
        }

        private void RegisterRegion(string regionName, long population, double infected)
        {
            var marker = globe.Find(regionName);
            if (marker == null)
            {
                return;
            }

            var renderer = marker.GetComponent<Renderer>();
            var state = new RegionState
            {
                Name = regionName,
                Population = population,
                Infected = infected,
                Marker = marker,
                MarkerRenderer = renderer,
                BaseColor = new Color(0.2f, 0.85f, 1f)
            };
            if (renderer != null)
            {
                renderer.sharedMaterial = infected > 0d ? warningMaterial : healthyMaterial;
            }
            regions.Add(state);
        }

        private void SimulateLocalGrowth(float deltaTime)
        {
            foreach (var region in regions)
            {
                if (region.Infected < 1d || region.Infected >= region.Population)
                {
                    continue;
                }

                var saturation = 1d - region.Infected / region.Population;
                var dailyGrowth = 0.24d * saturation;
                var gameDays = deltaTime / 240d;
                region.Infected = Math.Min(
                    region.Population,
                    region.Infected * Math.Exp(dailyGrowth * gameDays));
            }
        }

        private void SimulateTravelSpread()
        {
            foreach (var region in regions)
            {
                if (region.Infected >= 1d)
                {
                    continue;
                }

                RegionState source = null;
                foreach (var candidate in regions)
                {
                    if (candidate.Infected > 1000d)
                    {
                        source = candidate;
                        break;
                    }
                }

                if (source == null)
                {
                    return;
                }

                var pressure = Mathf.Clamp01((float)(source.Infected / 250000d));
                if (UnityEngine.Random.value < 0.08f + pressure * 0.32f)
                {
                    region.Infected = UnityEngine.Random.Range(8, 42);
                    return;
                }
            }
        }

        private void CreateRoute(int from, int to)
        {
            if (from >= regions.Count || to >= regions.Count)
            {
                return;
            }

            var start = regions[from].Marker.localPosition;
            var end = regions[to].Marker.localPosition;
            var routeObject = new GameObject("Air Route");
            routeObject.transform.SetParent(globe, false);
            var line = routeObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 18;
            line.startWidth = 0.004f;
            line.endWidth = 0.004f;
            line.sharedMaterial = routeMaterial;

            for (var index = 0; index < line.positionCount; index++)
            {
                var t = index / (float)(line.positionCount - 1);
                var direction = Vector3.Slerp(start.normalized, end.normalized, t);
                var arc = Mathf.Sin(t * Mathf.PI) * 0.12f;
                line.SetPosition(index, direction * (0.515f + arc));
            }

            var flight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flight.name = "Aircraft";
            flight.transform.SetParent(globe, false);
            flight.transform.localScale = Vector3.one * 0.009f;
            flight.GetComponent<Renderer>().sharedMaterial = flightMaterial;
            var collider = flight.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            flights.Add(new Flight
            {
                Visual = flight.transform,
                Start = start.normalized,
                End = end.normalized,
                Progress = UnityEngine.Random.value,
                Speed = UnityEngine.Random.Range(0.035f, 0.075f)
            });
        }

        private void UpdateFlights()
        {
            foreach (var flight in flights)
            {
                flight.Progress += Time.deltaTime * flight.Speed;
                if (flight.Progress > 1f)
                {
                    flight.Progress -= 1f;
                    var swap = flight.Start;
                    flight.Start = flight.End;
                    flight.End = swap;
                }

                var direction = Vector3.Slerp(flight.Start, flight.End, flight.Progress);
                var arc = Mathf.Sin(flight.Progress * Mathf.PI) * 0.12f;
                flight.Visual.localPosition = direction * (0.515f + arc);
            }
        }

        private void RefreshVisuals()
        {
            foreach (var region in regions)
            {
                if (region.MarkerRenderer == null)
                {
                    continue;
                }

                var ratio = region.Infected / Math.Max(1d, region.Population);
                if (ratio >= 0.01d)
                {
                    region.MarkerRenderer.sharedMaterial = criticalMaterial;
                }
                else if (region.Infected >= 1d)
                {
                    region.MarkerRenderer.sharedMaterial = warningMaterial;
                }
                else
                {
                    region.MarkerRenderer.sharedMaterial = healthyMaterial;
                }
            }
        }

        private void RefreshTotals()
        {
            long infected = 0L;
            long population = 0L;
            var affected = 0;
            foreach (var region in regions)
            {
                population += region.Population;
                var regionalInfected = (long)Math.Min(region.Population, Math.Floor(region.Infected));
                infected += regionalInfected;
                if (regionalInfected > 0)
                {
                    affected++;
                }
            }

            TotalPopulation = population;
            TotalInfected = infected;
            infectedRegions = affected;
        }

        private void CreateMaterials()
        {
            healthyMaterial = CreateEmissive("Stable Region", new Color(0.08f, 0.62f, 0.92f), 1.5f);
            warningMaterial = CreateEmissive("Affected Region", new Color(1f, 0.36f, 0.035f), 2f);
            criticalMaterial = CreateEmissive("Critical Region", new Color(0.95f, 0.025f, 0.015f), 2.4f);
            routeMaterial = CreateEmissive("Air Route", new Color(0.08f, 0.38f, 0.62f, 0.42f), 0.8f);
            flightMaterial = CreateEmissive("Aircraft Light", new Color(0.75f, 0.94f, 1f), 2.2f);
        }

        private static Material CreateEmissive(string materialName, Color color, float intensity)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = materialName, color = color };
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
            }
            return material;
        }
    }
}
