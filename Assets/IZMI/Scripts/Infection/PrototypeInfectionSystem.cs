using System.Collections.Generic;
using UnityEngine;

namespace Izmi
{
    public sealed class PrototypeInfectionSystem : MonoBehaviour
    {
        [SerializeField] private float infectionCheckInterval = 0.75f;
        [SerializeField] private float infectionDistance = 1.7f;

        private readonly List<CityPedestrian> civilians = new List<CityPedestrian>();
        private readonly HashSet<CityPedestrian> infected = new HashSet<CityPedestrian>();
        private Transform origin;
        private Material infectedMaterial;
        private float timer;

        public int InfectedCount => infected.Count;
        public int PopulationCount => civilians.Count;

        public void Configure(Transform infectionOrigin)
        {
            origin = infectionOrigin;
        }

        private void Start()
        {
            civilians.AddRange(GetComponentsInChildren<CityPedestrian>(true));
            infectedMaterial = CreateInfectedMaterial();

            var firstVictim = FindClosestHealthy(origin.position, float.MaxValue);
            if (firstVictim != null)
            {
                Infect(firstVictim);
            }
        }

        private void Update()
        {
            if (infected.Count == 0)
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
                var victim = FindClosestHealthy(
                    carrier.transform.position,
                    infectionDistance);

                if (victim != null && !newlyInfected.Contains(victim))
                {
                    newlyInfected.Add(victim);
                }
            }

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

            var renderer = civilian.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = infectedMaterial;
            }

            civilian.transform.localScale *= 1.12f;
        }

        private static Material CreateInfectedMaterial()
        {
            var material = new Material(Shader.Find("Standard"))
            {
                name = "Infected Civilian",
                color = new Color(0.42f, 0.08f, 0.055f)
            };

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.32f, 0.015f, 0.005f));
            }

            return material;
        }
    }
}
