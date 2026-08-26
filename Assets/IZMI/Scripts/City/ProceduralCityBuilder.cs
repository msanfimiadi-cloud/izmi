using UnityEngine;

namespace Izmi
{
    public static class ProceduralCityBuilder
    {
        private static Material groundMaterial;
        private static Material roadMaterial;
        private static Material waterMaterial;
        private static Material[] buildingMaterials;
        private static Material[] vehicleMaterials;
        private static Material pedestrianMaterial;
        private static Material infectionMaterial;

        public static GameObject Build()
        {
            CreateMaterials();

            var randomState = Random.state;
            Random.InitState(26082026);

            var root = new GameObject("Test City");
            CreateGround(root.transform);
            CreateRoads(root.transform);
            CreateBuildings(root.transform);
            CreateVehicles(root.transform);
            CreatePedestrians(root.transform);
            CreateInfectionOrigin(root.transform);

            Random.state = randomState;
            return root;
        }

        private static void CreateGround(Transform root)
        {
            CreateCube(
                "City Ground",
                root,
                new Vector3(0f, -0.35f, 0f),
                new Vector3(46f, 0.6f, 42f),
                groundMaterial);

            CreateCube(
                "River",
                root,
                new Vector3(19.5f, -0.02f, 0f),
                new Vector3(7f, 0.08f, 42f),
                waterMaterial);
        }

        private static void CreateRoads(Transform root)
        {
            const int roadCount = 7;
            const float spacing = 5.5f;
            const float length = 39f;

            for (var index = 0; index < roadCount; index++)
            {
                var coordinate = (index - (roadCount - 1) / 2f) * spacing;

                CreateCube(
                    "East West Road",
                    root,
                    new Vector3(0f, 0.01f, coordinate),
                    new Vector3(length, 0.08f, 1.1f),
                    roadMaterial);

                if (coordinate < 16f)
                {
                    CreateCube(
                        "North South Road",
                        root,
                        new Vector3(coordinate, 0.015f, 0f),
                        new Vector3(1.1f, 0.08f, length),
                        roadMaterial);
                }
            }
        }

        private static void CreateBuildings(Transform root)
        {
            const int blocks = 6;
            const float spacing = 5.5f;

            for (var x = 0; x < blocks; x++)
            {
                for (var z = 0; z < blocks; z++)
                {
                    var centerX = (x - (blocks - 1) / 2f) * spacing + spacing * 0.5f;
                    var centerZ = (z - (blocks - 1) / 2f) * spacing + spacing * 0.5f;

                    if (centerX > 15.5f)
                    {
                        continue;
                    }

                    var height = Random.Range(2.3f, 10.5f);
                    var footprintX = Random.Range(2.4f, 3.7f);
                    var footprintZ = Random.Range(2.4f, 3.7f);
                    var material = buildingMaterials[
                        Random.Range(0, buildingMaterials.Length)];

                    CreateCube(
                        "Building",
                        root,
                        new Vector3(centerX, height * 0.5f, centerZ),
                        new Vector3(footprintX, height, footprintZ),
                        material);

                    if (height > 6f && Random.value > 0.5f)
                    {
                        CreateCube(
                            "Rooftop",
                            root,
                            new Vector3(centerX, height + 0.25f, centerZ),
                            new Vector3(footprintX * 0.42f, 0.5f, footprintZ * 0.42f),
                            roadMaterial);
                    }
                }
            }
        }

        private static void CreateVehicles(Transform root)
        {
            const float routeLength = 36f;

            for (var index = 0; index < 26; index++)
            {
                var horizontal = index % 2 == 0;
                var laneIndex = Random.Range(-3, 4);
                var laneCoordinate = laneIndex * 5.5f;
                var laneOffset = index % 4 < 2 ? -0.28f : 0.28f;

                Vector3 start;
                Vector3 end;

                if (horizontal)
                {
                    start = new Vector3(-routeLength * 0.5f, 0.28f, laneCoordinate + laneOffset);
                    end = new Vector3(routeLength * 0.5f, 0.28f, laneCoordinate + laneOffset);
                }
                else
                {
                    start = new Vector3(laneCoordinate + laneOffset, 0.28f, -routeLength * 0.5f);
                    end = new Vector3(laneCoordinate + laneOffset, 0.28f, routeLength * 0.5f);
                }

                var vehicle = CreateCube(
                    "Vehicle",
                    root,
                    start,
                    new Vector3(0.42f, 0.32f, 0.82f),
                    vehicleMaterials[Random.Range(0, vehicleMaterials.Length)]);

                vehicle.AddComponent<CityVehicle>().Configure(
                    start,
                    end,
                    Random.Range(2.2f, 4.8f),
                    Random.value);
            }
        }

        private static void CreatePedestrians(Transform root)
        {
            for (var index = 0; index < 34; index++)
            {
                var pedestrian = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                pedestrian.name = "Civilian";
                pedestrian.transform.SetParent(root, false);
                pedestrian.transform.localScale = new Vector3(0.18f, 0.36f, 0.18f);
                pedestrian.GetComponent<Renderer>().sharedMaterial = pedestrianMaterial;

                var center = new Vector3(
                    Random.Range(-14f, 14f),
                    0f,
                    Random.Range(-14f, 14f));

                pedestrian.transform.position = center + Vector3.right;
                pedestrian.AddComponent<CityPedestrian>().Configure(
                    center,
                    Random.Range(0.5f, 1.5f),
                    Random.Range(0.35f, 0.85f),
                    Random.Range(0f, Mathf.PI * 2f));
            }
        }

        private static void CreateInfectionOrigin(Transform root)
        {
            var origin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            origin.name = "First Infected";
            origin.transform.SetParent(root, false);
            origin.transform.position = new Vector3(2.8f, 0.55f, -2.8f);
            origin.transform.localScale = Vector3.one * 0.48f;
            origin.GetComponent<Renderer>().sharedMaterial = infectionMaterial;
            origin.AddComponent<MarkerPulse>();
        }

        private static GameObject CreateCube(
            string objectName,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static void CreateMaterials()
        {
            groundMaterial = MaterialFor("Ground", new Color(0.15f, 0.22f, 0.16f), 0.05f);
            roadMaterial = MaterialFor("Road", new Color(0.055f, 0.065f, 0.075f), 0.12f);
            waterMaterial = MaterialFor("River", new Color(0.025f, 0.24f, 0.38f), 0.7f);

            buildingMaterials = new[]
            {
                MaterialFor("Concrete", new Color(0.34f, 0.38f, 0.42f), 0.18f),
                MaterialFor("Warm Stone", new Color(0.48f, 0.39f, 0.31f), 0.12f),
                MaterialFor("Glass", new Color(0.12f, 0.28f, 0.38f), 0.62f),
                MaterialFor("Light Facade", new Color(0.58f, 0.57f, 0.52f), 0.2f)
            };

            vehicleMaterials = new[]
            {
                MaterialFor("White Vehicle", new Color(0.8f, 0.82f, 0.84f), 0.35f),
                MaterialFor("Red Vehicle", new Color(0.6f, 0.055f, 0.04f), 0.3f),
                MaterialFor("Blue Vehicle", new Color(0.04f, 0.2f, 0.48f), 0.3f),
                MaterialFor("Dark Vehicle", new Color(0.035f, 0.04f, 0.05f), 0.28f)
            };

            pedestrianMaterial = MaterialFor(
                "Civilian",
                new Color(0.86f, 0.66f, 0.47f),
                0.1f);

            infectionMaterial = MaterialFor(
                "Infection",
                new Color(0.82f, 0.025f, 0.015f),
                0.22f);

            if (infectionMaterial.HasProperty("_EmissionColor"))
            {
                infectionMaterial.EnableKeyword("_EMISSION");
                infectionMaterial.SetColor(
                    "_EmissionColor",
                    new Color(1f, 0.015f, 0.005f) * 2.2f);
            }
        }

        private static Material MaterialFor(
            string materialName,
            Color color,
            float smoothness)
        {
            var shader = Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            return material;
        }
    }
}
