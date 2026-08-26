using UnityEngine;

namespace Izmi
{
    public sealed class CityResponseVisualizer : MonoBehaviour
    {
        private GameObject responseRoot;
        private Material darkMaterial;
        private Material militaryMaterial;
        private Material medicalMaterial;
        private Material shelterMaterial;
        private Material warningMaterial;
        private Material whiteMaterial;

        public void Apply(GlobalOutbreakSystem outbreak)
        {
            if (outbreak == null)
            {
                return;
            }

            if (responseRoot != null)
            {
                Destroy(responseRoot);
            }

            CreateMaterials();
            responseRoot = new GameObject("Visible Crisis Response");
            responseRoot.transform.SetParent(transform, false);

            var strongest = Mathf.Max(
                outbreak.WarReadiness,
                Mathf.Max(outbreak.CureResearch, outbreak.ShelterReadiness));

            if (strongest < 10)
            {
                CreateObservationPost();
            }
            else if (strongest == outbreak.WarReadiness)
            {
                CreateMilitaryCheckpoint(outbreak.WarReadiness);
            }
            else if (strongest == outbreak.CureResearch)
            {
                CreateFieldHospital(outbreak.CureResearch);
            }
            else
            {
                CreateSurvivorSettlement(outbreak.ShelterReadiness);
            }
        }

        private void CreateObservationPost()
        {
            CreateCube("Observation Van", new Vector3(-10f, 0.35f, -10f),
                new Vector3(1.7f, 0.7f, 0.85f), whiteMaterial);
            CreateCube("Observation Cabin", new Vector3(-10f, 0.82f, -10f),
                new Vector3(0.75f, 0.28f, 0.72f), medicalMaterial);
            CreateBeacon(new Vector3(-9.35f, 1.05f, -10f));
        }

        private void CreateMilitaryCheckpoint(int readiness)
        {
            for (var x = -12f; x <= 12f; x += 3f)
            {
                CreateCube("Concrete Barrier", new Vector3(x, 0.38f, -11f),
                    new Vector3(2.2f, 0.72f, 0.55f), darkMaterial);
                CreateCube("Warning Stripe", new Vector3(x, 0.77f, -11.29f),
                    new Vector3(1.8f, 0.08f, 0.03f), warningMaterial);
            }

            CreateCube("Command Post", new Vector3(-10f, 1.3f, -7.8f),
                new Vector3(3.8f, 2.6f, 2.8f), militaryMaterial);
            CreateCube("Watch Tower", new Vector3(10.5f, 2.4f, -8.5f),
                new Vector3(1.5f, 4.8f, 1.5f), darkMaterial);
            CreateCube("Tower Cabin", new Vector3(10.5f, 5.05f, -8.5f),
                new Vector3(2.2f, 0.9f, 2.2f), militaryMaterial);

            var vehicles = Mathf.Clamp(1 + readiness / 25, 1, 5);
            for (var index = 0; index < vehicles; index++)
            {
                var position = new Vector3(-5f + index * 2.3f, 0.42f, -8.2f);
                CreateCube("Armored Vehicle", position,
                    new Vector3(1.75f, 0.72f, 0.95f), militaryMaterial);
                CreateCube("Vehicle Turret", position + Vector3.up * 0.55f,
                    new Vector3(0.65f, 0.32f, 0.58f), darkMaterial);
            }
        }

        private void CreateFieldHospital(int research)
        {
            CreateCube("Hospital Platform", new Vector3(-8f, 0.18f, -8f),
                new Vector3(10f, 0.25f, 8f), whiteMaterial);

            for (var index = 0; index < 3; index++)
            {
                var x = -10.7f + index * 2.8f;
                CreateCube("Medical Tent", new Vector3(x, 0.85f, -8.3f),
                    new Vector3(2.35f, 1.45f, 3.2f), whiteMaterial);
                CreateCube("Tent Roof", new Vector3(x, 1.72f, -8.3f),
                    new Vector3(2.5f, 0.32f, 3.35f), medicalMaterial);
                CreateMedicalCross(new Vector3(x, 1.05f, -6.66f));
            }

            var ambulances = Mathf.Clamp(1 + research / 34, 1, 4);
            for (var index = 0; index < ambulances; index++)
            {
                var position = new Vector3(-11f + index * 2.1f, 0.48f, -3.5f);
                CreateCube("Ambulance", position,
                    new Vector3(1.55f, 0.82f, 0.8f), whiteMaterial);
                CreateCube("Ambulance Stripe", position + new Vector3(0f, 0.08f, -0.41f),
                    new Vector3(1.2f, 0.16f, 0.025f), medicalMaterial);
                CreateBeacon(position + Vector3.up * 0.52f);
            }
        }

        private void CreateSurvivorSettlement(int readiness)
        {
            CreateCube("Settlement Ground", new Vector3(-8f, 0.14f, -8f),
                new Vector3(11f, 0.2f, 9f), shelterMaterial);

            for (var x = -13f; x <= -3f; x += 2f)
            {
                CreateCube("Settlement Fence", new Vector3(x, 0.75f, -12.3f),
                    new Vector3(1.7f, 1.35f, 0.12f), darkMaterial);
            }

            var tents = Mathf.Clamp(2 + readiness / 18, 2, 7);
            for (var index = 0; index < tents; index++)
            {
                var row = index / 3;
                var column = index % 3;
                var position = new Vector3(-11f + column * 3f, 0.65f, -9.8f + row * 3f);
                CreateCube("Survivor Tent", position,
                    new Vector3(2.35f, 1.15f, 2.2f), shelterMaterial);
                CreateCube("Tent Roof", position + Vector3.up * 0.72f,
                    new Vector3(2.55f, 0.28f, 2.4f), warningMaterial);
            }

            for (var index = 0; index < 5; index++)
            {
                CreateCube("Supply Crate", new Vector3(-3.8f, 0.38f + index % 2 * 0.65f, -10f + index * 0.85f),
                    new Vector3(0.85f, 0.7f, 0.72f), militaryMaterial);
            }

            CreateCube("Water Tank", new Vector3(-4.2f, 1.15f, -5.2f),
                new Vector3(1.6f, 2.3f, 1.6f), medicalMaterial);
        }

        private void CreateMedicalCross(Vector3 position)
        {
            CreateCube("Medical Cross", position,
                new Vector3(0.72f, 0.18f, 0.04f), medicalMaterial);
            CreateCube("Medical Cross", position,
                new Vector3(0.18f, 0.72f, 0.04f), medicalMaterial);
        }

        private void CreateBeacon(Vector3 position)
        {
            var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "Emergency Beacon";
            beacon.transform.SetParent(responseRoot.transform, false);
            beacon.transform.position = position;
            beacon.transform.localScale = Vector3.one * 0.18f;
            beacon.GetComponent<Renderer>().sharedMaterial = warningMaterial;
            beacon.AddComponent<MarkerPulse>();
        }

        private GameObject CreateCube(string objectName, Vector3 position,
            Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(responseRoot.transform, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private void CreateMaterials()
        {
            darkMaterial = MaterialFor("Crisis Dark", new Color(0.045f, 0.055f, 0.06f), 0.12f);
            militaryMaterial = MaterialFor("Military", new Color(0.16f, 0.24f, 0.12f), 0.08f);
            medicalMaterial = Emissive("Medical Blue", new Color(0.05f, 0.5f, 0.78f), 1.5f);
            shelterMaterial = MaterialFor("Shelter", new Color(0.48f, 0.37f, 0.19f), 0.06f);
            warningMaterial = Emissive("Emergency", new Color(0.95f, 0.12f, 0.035f), 1.8f);
            whiteMaterial = MaterialFor("Medical White", new Color(0.78f, 0.82f, 0.82f), 0.3f);
        }

        private static Material Emissive(string name, Color color, float intensity)
        {
            var material = MaterialFor(name, color, 0.22f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
            }
            return material;
        }

        private static Material MaterialFor(string name, Color color, float smoothness)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }
    }
}
