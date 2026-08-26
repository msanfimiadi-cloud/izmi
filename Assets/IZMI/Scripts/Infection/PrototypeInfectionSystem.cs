using System.Collections.Generic;
using UnityEngine;

namespace Izmi
{
    public sealed class PrototypeInfectionSystem : MonoBehaviour
    {
        [SerializeField] private float infectionCheckInterval = 0.75f;
        [SerializeField] private float infectionDistance = 1.45f;

        private readonly List<CityPedestrian> civilians = new List<CityPedestrian>();
        private readonly HashSet<CityPedestrian> infected = new HashSet<CityPedestrian>();
        private Transform origin;
        private Material infectedMaterial;
        private float timer;
        private int newlyInfectedLastWave;

        public int InfectedCount => infected.Count;
        public int PopulationCount => civilians.Count;
        public int HealthyCount => Mathf.Max(0, PopulationCount - InfectedCount);
        public int NewlyInfectedLastWave => newlyInfectedLastWave;
        public float InfectionRatio => PopulationCount > 0
            ? InfectedCount / (float)PopulationCount
            : 0f;

        public string AlertLevel
        {
            get
            {
                if (InfectionRatio >= 0.66f) return "КРИТИЧЕСКИЙ";
                if (InfectionRatio >= 0.30f) return "ВЫСОКИЙ";
                if (InfectionRatio >= 0.10f) return "ПОВЫШЕННЫЙ";
                return "НАБЛЮДЕНИЕ";
            }
        }

        public void Configure(Transform infectionOrigin)
        {
            origin = infectionOrigin;
        }

        private void Start()
        {
            civilians.AddRange(GetComponentsInChildren<CityPedestrian>(true));
            infectedMaterial = CreateInfectedMaterial();

            if (origin != null)
            {
                var firstVictim = FindClosestHealthy(origin.position, float.MaxValue);
                if (firstVictim != null)
                {
                    Infect(firstVictim);
                }
            }
        }

        private void Update()
        {
            if (infected.Count == 0 || Time.timeScale <= 0f)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer < infectionCheckInterval)
            {
                return;
            }

            timer = 0f;
            var newlyInfected = new List<CityPedestrian>();

            foreach (var carrier in infected)
            {
                if (carrier == null)
                {
                    continue;
                }

                var victim = FindClosestHealthy(carrier.transform.position, infectionDistance);
                if (victim != null && !newlyInfected.Contains(victim))
                {
                    newlyInfected.Add(victim);
                }
            }

            newlyInfectedLastWave = newlyInfected.Count;
            foreach (var victim in newlyInfected)
            {
                Infect(victim);
            }
        }

        private CityPedestrian FindClosestHealthy(Vector3 position, float maxDistance)
        {
            CityPedestrian closest = null;
            var closestDistance = maxDistance;

            foreach (var civilian in civilians)
            {
                if (civilian == null || infected.Contains(civilian))
                {
                    continue;
                }

                var distance = Vector3.Distance(position, civilian.transform.position);
                if (distance < closestDistance)
                {
                    closest = civilian;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private void Infect(CityPedestrian civilian)
        {
            if (civilian == null || !infected.Add(civilian))
            {
                return;
            }

            civilian.SetInfected(infectedMaterial);
            civilian.transform.localScale *= 1.1f;
        }

        private static Material CreateInfectedMaterial()
        {
            var material = new Material(Shader.Find("Standard"))
            {
                name = "Infected Civilian",
                color = new Color(0.33f, 0.055f, 0.035f)
            };

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.28f, 0.012f, 0.003f));
            }

            return material;
        }
    }
}
