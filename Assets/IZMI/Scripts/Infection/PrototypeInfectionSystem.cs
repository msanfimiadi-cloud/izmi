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
        private readonly HashSet<CityPedestrian> evacuated = new HashSet<CityPedestrian>();
        private Transform origin;
        private Material infectedMaterial;
        private float timer;
        private float quarantineTimer;
        private int newlyInfectedLastWave;
        private int initialInfectedCount = 1;

        public int InfectedCount => infected.Count;
        public int PopulationCount => civilians.Count;
        public int EvacuatedCount => evacuated.Count;
        public int HealthyCount => Mathf.Max(0, PopulationCount - InfectedCount - EvacuatedCount);
        public int NewlyInfectedLastWave => newlyInfectedLastWave;
        public bool IsQuarantineActive => quarantineTimer > 0f;
        public float QuarantineSeconds => Mathf.Max(0f, quarantineTimer);
        public float InfectionRatio => Mathf.Max(1, PopulationCount - EvacuatedCount) > 0
            ? InfectedCount / (float)Mathf.Max(1, PopulationCount - EvacuatedCount)
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

        public void ConfigureRegionalSeverity(double regionalInfected, long regionalPopulation)
        {
            var ratio = regionalPopulation > 0L
                ? regionalInfected / regionalPopulation
                : 0d;
            var absolutePressure = Mathf.Clamp01((float)(regionalInfected / 1000000d));
            initialInfectedCount = Mathf.Clamp(
                1 + Mathf.RoundToInt((float)ratio * 28f + absolutePressure * 8f),
                1,
                12);
        }

        public void DeployQuarantine(float duration)
        {
            quarantineTimer = Mathf.Max(quarantineTimer, duration);
        }

        public int TreatInfected(int requestedCount)
        {
            var treated = 0;
            var candidates = new List<CityPedestrian>(infected);
            for (var index = candidates.Count - 1; index >= 0 && treated < requestedCount; index--)
            {
                var civilian = candidates[index];
                if (civilian == null)
                {
                    infected.Remove(civilian);
                    continue;
                }

                infected.Remove(civilian);
                civilian.SetHealthy();
                treated++;
            }

            return treated;
        }

        public int EvacuateHealthy(int requestedCount)
        {
            var moved = 0;
            foreach (var civilian in civilians)
            {
                if (moved >= requestedCount)
                {
                    break;
                }

                if (civilian == null || infected.Contains(civilian) || evacuated.Contains(civilian))
                {
                    continue;
                }

                evacuated.Add(civilian);
                civilian.gameObject.SetActive(false);
                moved++;
            }

            return moved;
        }

        private void Start()
        {
            civilians.AddRange(GetComponentsInChildren<CityPedestrian>(true));
            infectedMaterial = CreateInfectedMaterial();

            if (origin != null)
            {
                for (var index = 0; index < initialInfectedCount; index++)
                {
                    var firstVictim = FindClosestHealthy(origin.position, float.MaxValue);
                    if (firstVictim == null)
                    {
                        break;
                    }
                    Infect(firstVictim);
                }
            }
        }

        private void Update()
        {
            if (quarantineTimer > 0f)
            {
                quarantineTimer -= Time.deltaTime;
            }

            if (infected.Count == 0 || Time.timeScale <= 0f)
            {
                return;
            }

            timer += Time.deltaTime;
            var interval = infectionCheckInterval * (IsQuarantineActive ? 3.5f : 1f);
            if (timer < interval)
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

                var distance = infectionDistance * (IsQuarantineActive ? 0.62f : 1f);
                var victim = FindClosestHealthy(carrier.transform.position, distance);
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
                if (civilian == null || infected.Contains(civilian) || evacuated.Contains(civilian))
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
            if (civilian == null || evacuated.Contains(civilian) || !infected.Add(civilian))
            {
                return;
            }

            civilian.SetInfected(infectedMaterial);
            civilian.transform.localScale *= 1.1f;
        }

        private static Material CreateInfectedMaterial()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader)
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
