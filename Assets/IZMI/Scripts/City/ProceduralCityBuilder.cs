using UnityEngine;

namespace Izmi
{
    public static class ProceduralCityBuilder
    {
        private static Material groundMaterial;
        private static Material roadMaterial;
        private static Material sidewalkMaterial;
        private static Material laneMaterial;
        private static Material waterMaterial;
        private static Material grassMaterial;
        private static Material treeTrunkMaterial;
        private static Material treeLeafMaterial;
        private static Material windowMaterial;
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
            CreateRoadNetwork(root.transform);
            CreateCityBlocks(root.transform);
            CreateStreetFurniture(root.transform);
            CreateVehicles(root.transform);
            CreatePedestrians(root.transform);
            var infectionOrigin = CreateInfectionOrigin(root.transform);
            root.AddComponent<PrototypeInfectionSystem>().Configure(infectionOrigin);

            Random.state = randomState;
            return root;
        }

        private static void CreateGround(Transform root)
        {
            CreateCube("City Ground", root, new Vector3(0f, -0.35f, 0f),
                new Vector3(46f, 0.6f, 42f), groundMaterial);
            CreateCube("River", root, new Vector3(19.5f, -0.01f, 0f),
                new Vector3(7f, 0.1f, 42f), waterMaterial);

            CreateCube("River Walk", root, new Vector3(15.7f, 0.02f, 0f),
                new Vector3(0.9f, 0.12f, 42f), sidewalkMaterial);
            for (var z = -17f; z <= 17f; z += 5.5f)
            {
                CreateTree(root, new Vector3(15.65f, 0.1f, z + 1.8f), 0.8f);
            }
        }

        private static void CreateRoadNetwork(Transform root)
        {
            const int roadCount = 7;
            const float spacing = 5.5f;
            const float length = 39f;

            for (var index = 0; index < roadCount; index++)
            {
                var coordinate = (index - 3f) * spacing;
                CreateCube("East West Road", root, new Vector3(0f, 0.01f, coordinate),
                    new Vector3(length, 0.08f, 1.25f), roadMaterial);
                CreateDashedLine(root, true, coordinate, length);

                if (coordinate < 16f)
                {
                    CreateCube("North South Road", root, new Vector3(coordinate, 0.015f, 0f),
                        new Vector3(1.25f, 0.08f, length), roadMaterial);
                    CreateDashedLine(root, false, coordinate, length);
                }
            }
        }

        private static void CreateDashedLine(Transform root, bool horizontal, float coordinate, float length)
        {
            for (var offset = -length * 0.5f + 1f; offset < length * 0.5f; offset += 2.1f)
            {
                var position = horizontal
                    ? new Vector3(offset, 0.065f, coordinate)
                    : new Vector3(coordinate, 0.07f, offset);
                var scale = horizontal
                    ? new Vector3(0.85f, 0.015f, 0.045f)
                    : new Vector3(0.045f, 0.015f, 0.85f);
                CreateCube("Lane Marking", root, position, scale, laneMaterial);
            }
        }

        private static void CreateCityBlocks(Transform root)
        {
            const float spacing = 5.5f;
            for (var x = 0; x < 6; x++)
            {
                for (var z = 0; z < 6; z++)
                {
                    var center = new Vector3(
                        (x - 2.5f) * spacing + spacing * 0.5f,
                        0f,
                        (z - 2.5f) * spacing + spacing * 0.5f);
                    if (center.x > 15.2f)
                    {
                        continue;
                    }

                    if (center.x < -2.2f && center.z < -2.2f)
                    {
                        CreateCube("Crisis Response District", root,
                            new Vector3(center.x, 0.07f, center.z),
                            new Vector3(4.15f, 0.18f, 4.15f), sidewalkMaterial);
                        continue;
                    }

                    CreateCube("Sidewalk Block", root, new Vector3(center.x, 0.06f, center.z),
                        new Vector3(4.15f, 0.16f, 4.15f), sidewalkMaterial);

                    var districtRoll = Random.value;
                    if (districtRoll < 0.15f)
                    {
                        CreatePark(root, center);
                    }
                    else if (districtRoll < 0.32f)
                    {
                        CreateResidentialBlock(root, center);
                    }
                    else
                    {
                        CreateTower(root, center);
                    }
                }
            }
        }

        private static void CreateTower(Transform root, Vector3 center)
        {
            var height = Random.Range(4.2f, 11.5f);
            var footprintX = Random.Range(2.5f, 3.45f);
            var footprintZ = Random.Range(2.5f, 3.45f);
            var material = buildingMaterials[Random.Range(0, buildingMaterials.Length)];

            CreateCube("Office Tower", root, new Vector3(center.x, height * 0.5f + 0.14f, center.z),
                new Vector3(footprintX, height, footprintZ), material);

            var windowRows = Mathf.Clamp(Mathf.FloorToInt(height / 1.45f), 2, 7);
            for (var row = 0; row < windowRows; row++)
            {
                var y = 1f + row * 1.35f;
                if (y >= height - 0.35f) break;
                for (var side = -1; side <= 1; side += 2)
                {
                    CreateCube("Lit Windows", root,
                        new Vector3(center.x + side * (footprintX * 0.505f), y, center.z),
                        new Vector3(0.025f, 0.38f, footprintZ * 0.62f), windowMaterial);
                }
            }

            if (height > 7f)
            {
                CreateCube("Rooftop Plant", root, new Vector3(center.x, height + 0.38f, center.z),
                    new Vector3(footprintX * 0.46f, 0.62f, footprintZ * 0.46f), roadMaterial);
                CreateCube("Antenna", root, new Vector3(center.x, height + 1.15f, center.z),
                    new Vector3(0.08f, 1.25f, 0.08f), laneMaterial);
            }
        }

        private static void CreateResidentialBlock(Transform root, Vector3 center)
        {
            var rotation = Random.value > 0.5f;
            for (var index = -1; index <= 1; index += 2)
            {
                var height = Random.Range(2.5f, 5.2f);
                var offset = rotation
                    ? new Vector3(index * 1.05f, 0f, 0f)
                    : new Vector3(0f, 0f, index * 1.05f);
                var scale = rotation
                    ? new Vector3(1.55f, height, 3.25f)
                    : new Vector3(3.25f, height, 1.55f);
                CreateCube("Apartment", root, center + offset + Vector3.up * (height * 0.5f + 0.14f),
                    scale, buildingMaterials[1]);
            }
        }

        private static void CreatePark(Transform root, Vector3 center)
        {
            CreateCube("Pocket Park", root, center + Vector3.up * 0.16f,
                new Vector3(3.65f, 0.16f, 3.65f), grassMaterial);
            CreateCube("Park Path", root, center + Vector3.up * 0.26f,
                new Vector3(3.4f, 0.035f, 0.42f), sidewalkMaterial);
            CreateTree(root, center + new Vector3(-1.15f, 0.2f, 1.05f), 0.72f);
            CreateTree(root, center + new Vector3(1.12f, 0.2f, -1.05f), 0.62f);
            if (Random.value > 0.45f)
            {
                CreateTree(root, center + new Vector3(1.1f, 0.2f, 1.08f), 0.55f);
            }
        }

        private static void CreateStreetFurniture(Transform root)
        {
            for (var z = -14f; z <= 14f; z += 5.5f)
            {
                for (var x = -14f; x <= 14f; x += 5.5f)
                {
                    if (x > 14.5f) continue;
                    CreateLamp(root, new Vector3(x + 0.82f, 0.12f, z + 0.82f));
                }
            }
        }

        private static void CreateTree(Transform root, Vector3 position, float size)
        {
            CreateCube("Tree Trunk", root, position + Vector3.up * (0.48f * size),
                new Vector3(0.16f, 0.96f, 0.16f) * size, treeTrunkMaterial);
            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Tree Crown";
            crown.transform.SetParent(root, false);
            crown.transform.position = position + Vector3.up * (1.18f * size);
            crown.transform.localScale = new Vector3(0.92f, 1.08f, 0.92f) * size;
            crown.GetComponent<Renderer>().sharedMaterial = treeLeafMaterial;
        }

        private static void CreateLamp(Transform root, Vector3 position)
        {
            CreateCube("Street Lamp", root, position + Vector3.up * 0.75f,
                new Vector3(0.055f, 1.5f, 0.055f), roadMaterial);
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Lamp Glow";
            lamp.transform.SetParent(root, false);
            lamp.transform.position = position + Vector3.up * 1.52f;
            lamp.transform.localScale = Vector3.one * 0.16f;
            lamp.GetComponent<Renderer>().sharedMaterial = windowMaterial;
        }

        private static void CreateVehicles(Transform root)
        {
            const float routeLength = 36f;
            for (var index = 0; index < 28; index++)
            {
                var horizontal = index % 2 == 0;
                var laneCoordinate = Random.Range(-3, 4) * 5.5f;
                var laneOffset = index % 4 < 2 ? -0.3f : 0.3f;
                Vector3 start;
                Vector3 end;

                if (horizontal)
                {
                    start = new Vector3(-routeLength * 0.5f, 0.25f, laneCoordinate + laneOffset);
                    end = new Vector3(routeLength * 0.5f, 0.25f, laneCoordinate + laneOffset);
                }
                else
                {
                    start = new Vector3(laneCoordinate + laneOffset, 0.25f, -routeLength * 0.5f);
                    end = new Vector3(laneCoordinate + laneOffset, 0.25f, routeLength * 0.5f);
                }

                var vehicleRoot = new GameObject("Moving Vehicle");
                vehicleRoot.transform.SetParent(root, false);
                var bodyScale = horizontal
                    ? new Vector3(0.82f, 0.28f, 0.4f)
                    : new Vector3(0.4f, 0.28f, 0.82f);
                CreateCube("Car Body", vehicleRoot.transform, Vector3.zero, bodyScale,
                    vehicleMaterials[Random.Range(0, vehicleMaterials.Length)]);
                var cabinScale = horizontal
                    ? new Vector3(0.38f, 0.18f, 0.34f)
                    : new Vector3(0.34f, 0.18f, 0.38f);
                CreateCube("Car Cabin", vehicleRoot.transform, new Vector3(0f, 0.2f, 0f),
                    cabinScale, windowMaterial);
                vehicleRoot.AddComponent<CityVehicle>().Configure(
                    start, end, Random.Range(2.2f, 4.8f), Random.value);
            }
        }

        private static void CreatePedestrians(Transform root)
        {
            for (var index = 0; index < 44; index++)
            {
                var pedestrian = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                pedestrian.name = "Civilian";
                pedestrian.transform.SetParent(root, false);
                pedestrian.transform.localScale = new Vector3(0.2f, 0.38f, 0.2f);
                pedestrian.GetComponent<Renderer>().sharedMaterial = pedestrianMaterial;

                var center = new Vector3(Random.Range(-14f, 14f), 0f, Random.Range(-14f, 14f));
                pedestrian.transform.position = center + Vector3.right;
                pedestrian.AddComponent<CityPedestrian>().Configure(
                    center, Random.Range(0.55f, 1.65f), Random.Range(0.42f, 0.86f),
                    Random.Range(0f, Mathf.PI * 2f));
            }
        }

        private static Transform CreateInfectionOrigin(Transform root)
        {
            var origin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            origin.name = "First Infected";
            origin.transform.SetParent(root, false);
            origin.transform.position = new Vector3(2.8f, 0.55f, -2.8f);
            origin.transform.localScale = Vector3.one * 0.48f;
            origin.GetComponent<Renderer>().sharedMaterial = infectionMaterial;
            origin.AddComponent<MarkerPulse>();
            return origin.transform;
        }

        private static GameObject CreateCube(string objectName, Transform parent,
            Vector3 position, Vector3 scale, Material material)
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
            groundMaterial = MaterialFor("Ground", new Color(0.12f, 0.17f, 0.13f), 0.04f);
            roadMaterial = MaterialFor("Road", new Color(0.035f, 0.042f, 0.05f), 0.1f);
            sidewalkMaterial = MaterialFor("Sidewalk", new Color(0.36f, 0.37f, 0.36f), 0.08f);
            laneMaterial = EmissiveMaterial("Lane Marking", new Color(0.82f, 0.77f, 0.52f), 0.12f);
            waterMaterial = MaterialFor("River", new Color(0.02f, 0.2f, 0.34f), 0.72f);
            grassMaterial = MaterialFor("Park Grass", new Color(0.12f, 0.29f, 0.14f), 0.05f);
            treeTrunkMaterial = MaterialFor("Tree Trunk", new Color(0.22f, 0.11f, 0.055f), 0.02f);
            treeLeafMaterial = MaterialFor("Tree Leaves", new Color(0.08f, 0.31f, 0.12f), 0.05f);
            windowMaterial = EmissiveMaterial("Warm Windows", new Color(0.2f, 0.43f, 0.58f), 0.42f);

            buildingMaterials = new[]
            {
                MaterialFor("Concrete", new Color(0.31f, 0.34f, 0.37f), 0.16f),
                MaterialFor("Warm Stone", new Color(0.46f, 0.35f, 0.27f), 0.1f),
                MaterialFor("Glass", new Color(0.09f, 0.23f, 0.32f), 0.58f),
                MaterialFor("Light Facade", new Color(0.55f, 0.53f, 0.47f), 0.18f)
            };
            vehicleMaterials = new[]
            {
                MaterialFor("White Vehicle", new Color(0.78f, 0.81f, 0.83f), 0.35f),
                MaterialFor("Red Vehicle", new Color(0.56f, 0.045f, 0.03f), 0.3f),
                MaterialFor("Blue Vehicle", new Color(0.035f, 0.17f, 0.42f), 0.3f),
                MaterialFor("Dark Vehicle", new Color(0.025f, 0.03f, 0.04f), 0.28f)
            };
            pedestrianMaterial = MaterialFor("Civilian", new Color(0.86f, 0.66f, 0.47f), 0.1f);
            infectionMaterial = EmissiveMaterial("Infection", new Color(0.82f, 0.025f, 0.015f), 1.8f);
        }

        private static Material EmissiveMaterial(string name, Color color, float intensity)
        {
            var material = MaterialFor(name, color, 0.22f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
            }
            return material;
        }

        private static Material MaterialFor(string materialName, Color color, float smoothness)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = materialName, color = color };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }
    }
}
