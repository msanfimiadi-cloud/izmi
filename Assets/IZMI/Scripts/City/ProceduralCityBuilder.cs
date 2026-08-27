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
        private static Material clothingMaterial;
        private static Material darkClothingMaterial;
        private static Material wheelMaterial;
        private static Material infectionMaterial;
        private static int citySeed = 26082026;
        private static int vehicleCount = 46;
        private static int pedestrianCount = 86;
        private static float towerHeightScale = 1f;
        private static float parkChance = 0.15f;
        private static float residentialChance = 0.32f;
        private static string cityName = "Сингапур";

        public static GameObject Build(string regionName = "First anomaly")
        {
            ConfigureRegion(regionName);
            if (groundMaterial == null)
            {
                CreateMaterials();
            }
            var randomState = Random.state;
            Random.InitState(citySeed);

            var root = new GameObject(cityName);
            CreateGround(root.transform);
            CreateRoadNetwork(root.transform);
            CreateCityBlocks(root.transform);
            CreateStreetFurniture(root.transform);
            CreateVehicles(root.transform);
            CreateAircraft(root.transform);
            CreatePedestrians(root.transform);
            var infectionOrigin = CreateInfectionOrigin(root.transform);
            root.AddComponent<PrototypeInfectionSystem>().Configure(infectionOrigin);

            Random.state = randomState;
            return root;
        }

        private static void CreateGround(Transform root)
        {
            CreateCube("City Ground", root, new Vector3(0f, -0.35f, 0f),
                new Vector3(86f, 0.6f, 78f), groundMaterial);
            CreateCube("River", root, new Vector3(36f, -0.01f, 0f),
                new Vector3(10f, 0.1f, 78f), waterMaterial);

            CreateCube("River Walk", root, new Vector3(30.6f, 0.02f, 0f),
                new Vector3(1.2f, 0.12f, 78f), sidewalkMaterial);
            for (var z = -34f; z <= 34f; z += 6.2f)
            {
                CreateTree(root, new Vector3(30.55f, 0.1f, z + 1.8f), 0.8f);
            }
        }

        private static void CreateRoadNetwork(Transform root)
        {
            const int roadCount = 11;
            const float spacing = 6.2f;
            const float length = 72f;

            for (var index = 0; index < roadCount; index++)
            {
                var coordinate = (index - 5f) * spacing;
                CreateCube("East West Road", root, new Vector3(0f, 0.01f, coordinate),
                    new Vector3(length, 0.08f, 1.25f), roadMaterial);
                CreateDashedLine(root, true, coordinate, length);

                if (coordinate < 31f)
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
            const float spacing = 6.2f;
            for (var x = 0; x < 10; x++)
            {
                for (var z = 0; z < 10; z++)
                {
                    var center = new Vector3(
                        (x - 4.5f) * spacing + spacing * 0.5f,
                        0f,
                        (z - 4.5f) * spacing + spacing * 0.5f);
                    if (center.x > 30.2f)
                    {
                        continue;
                    }

                    if (center.x < -18f && center.z < -18f)
                    {
                        CreateCube("Crisis Response District", root,
                            new Vector3(center.x, 0.07f, center.z),
                            new Vector3(4.8f, 0.18f, 4.8f), sidewalkMaterial);
                        continue;
                    }

                    CreateCube("Sidewalk Block", root, new Vector3(center.x, 0.06f, center.z),
                        new Vector3(4.8f, 0.16f, 4.8f), sidewalkMaterial);

                    var districtRoll = Random.value;
                    if (districtRoll < parkChance)
                    {
                        CreatePark(root, center);
                    }
                    else if (districtRoll < residentialChance)
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
            var height = Random.Range(6.5f, 15.5f) * towerHeightScale;
            var footprintX = Random.Range(2.8f, 4.1f);
            var footprintZ = Random.Range(2.8f, 4.1f);
            var material = buildingMaterials[Random.Range(0, buildingMaterials.Length)];
            var style = Random.Range(0, 4);

            CreateCube("Tower Podium", root,
                center + Vector3.up * 0.65f,
                new Vector3(footprintX + 0.55f, 1.05f, footprintZ + 0.55f),
                buildingMaterials[Random.Range(0, buildingMaterials.Length)]);

            if (style == 0)
            {
                var lowerHeight = height * 0.58f;
                CreateCube("Setback Tower Lower", root,
                    center + Vector3.up * (1.05f + lowerHeight * 0.5f),
                    new Vector3(footprintX, lowerHeight, footprintZ), material);
                CreateCube("Setback Tower Upper", root,
                    center + Vector3.up * (1.05f + lowerHeight + (height - lowerHeight) * 0.5f),
                    new Vector3(footprintX * 0.72f, height - lowerHeight, footprintZ * 0.72f),
                    buildingMaterials[2]);
            }
            else if (style == 1)
            {
                CreateCylinder("Rounded Glass Tower", root,
                    center + Vector3.up * (1.05f + height * 0.5f),
                    new Vector3(footprintX * 0.52f, height * 0.5f, footprintZ * 0.52f),
                    buildingMaterials[2]);
                CreateCylinder("Tower Crown", root,
                    center + Vector3.up * (height + 1.35f),
                    new Vector3(footprintX * 0.36f, 0.3f, footprintZ * 0.36f),
                    windowMaterial);
            }
            else if (style == 2)
            {
                var wingWidth = footprintX * 0.44f;
                for (var side = -1; side <= 1; side += 2)
                {
                    var wingHeight = height * (side < 0 ? 0.82f : 1f);
                    CreateCube("Twin Tower Wing", root,
                        center + new Vector3(side * footprintX * 0.27f,
                            1.05f + wingHeight * 0.5f, 0f),
                        new Vector3(wingWidth, wingHeight, footprintZ),
                        side < 0 ? material : buildingMaterials[2]);
                }
                CreateCube("Sky Bridge", root,
                    center + Vector3.up * (height * 0.52f),
                    new Vector3(footprintX * 0.7f, 0.35f, footprintZ * 0.65f),
                    windowMaterial);
            }
            else
            {
                var levels = 4;
                for (var level = 0; level < levels; level++)
                {
                    var levelHeight = height / levels;
                    var setback = 1f - level * 0.13f;
                    CreateCube("Terraced Tower", root,
                        center + new Vector3(
                            level * 0.11f,
                            1.05f + levelHeight * (level + 0.5f),
                            level * 0.08f),
                        new Vector3(
                            footprintX * setback,
                            levelHeight - 0.08f,
                            footprintZ * setback),
                        level % 2 == 0 ? material : buildingMaterials[3]);
                }
            }

            AddFacadeBands(root, center, footprintX, footprintZ, height);
            CreateCube("Rooftop Plant", root,
                center + Vector3.up * (height + 1.45f),
                new Vector3(footprintX * 0.36f, 0.48f, footprintZ * 0.36f),
                roadMaterial);
        }

        private static void AddFacadeBands(
            Transform root,
            Vector3 center,
            float footprintX,
            float footprintZ,
            float height)
        {
            var rows = Mathf.Clamp(Mathf.FloorToInt(height / 1.5f), 3, 10);
            for (var row = 0; row < rows; row++)
            {
                var y = 1.45f + row * 1.35f;
                if (y > height) break;
                CreateCube("Facade Window Band", root,
                    center + new Vector3(0f, y, -footprintZ * 0.505f),
                    new Vector3(footprintX * 0.72f, 0.22f, 0.035f),
                    windowMaterial);
                CreateCube("Facade Window Band", root,
                    center + new Vector3(footprintX * 0.505f, y, 0f),
                    new Vector3(0.035f, 0.22f, footprintZ * 0.72f),
                    windowMaterial);
            }
        }

        private static void CreateResidentialBlock(Transform root, Vector3 center)
        {
            var alongX = Random.value > 0.5f;
            for (var index = -1; index <= 1; index++)
            {
                var height = Random.Range(3.4f, 6.4f);
                var offset = alongX
                    ? new Vector3(index * 1.35f, 0f, 0f)
                    : new Vector3(0f, 0f, index * 1.35f);
                var scale = alongX
                    ? new Vector3(1.18f, height, 3.45f)
                    : new Vector3(3.45f, height, 1.18f);
                var buildingCenter = center + offset;

                CreateCube("Residential Building", root,
                    buildingCenter + Vector3.up * (height * 0.5f + 0.14f),
                    scale,
                    buildingMaterials[index == 0 ? 3 : 1]);

                for (var floor = 1; floor < Mathf.FloorToInt(height); floor++)
                {
                    var balconyScale = alongX
                        ? new Vector3(0.9f, 0.08f, 3.65f)
                        : new Vector3(3.65f, 0.08f, 0.9f);
                    CreateCube("Residential Balcony", root,
                        buildingCenter + Vector3.up * (floor + 0.25f),
                        balconyScale,
                        sidewalkMaterial);
                }

                CreatePitchedRoof(root, buildingCenter, scale, height + 0.2f);
            }
        }

        private static void CreatePitchedRoof(
            Transform root,
            Vector3 center,
            Vector3 buildingScale,
            float y)
        {
            var alongX = buildingScale.x < buildingScale.z;
            for (var side = -1; side <= 1; side += 2)
            {
                var roof = CreateCube("Pitched Roof", root,
                    center + new Vector3(
                        alongX ? side * 0.26f : 0f,
                        y + 0.22f,
                        alongX ? 0f : side * 0.26f),
                    alongX
                        ? new Vector3(0.72f, 0.12f, buildingScale.z + 0.2f)
                        : new Vector3(buildingScale.x + 0.2f, 0.12f, 0.72f),
                    roadMaterial);
                roof.transform.rotation = Quaternion.Euler(
                    alongX ? 0f : side * 24f,
                    0f,
                    alongX ? side * 24f : 0f);
            }
        }

        private static GameObject CreateCylinder(
            string objectName,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = objectName;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.position = position;
            cylinder.transform.localScale = scale;
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            var collider = cylinder.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            return cylinder;
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
            const float routeLength = 69f;
            for (var index = 0; index < vehicleCount; index++)
            {
                var horizontal = index % 2 == 0;
                var laneCoordinate = Random.Range(-5, 6) * 6.2f;
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
                CreateCube("Car Body", vehicleRoot.transform, Vector3.zero,
                    new Vector3(0.44f, 0.28f, 0.92f),
                    vehicleMaterials[Random.Range(0, vehicleMaterials.Length)]);
                CreateCube("Car Cabin", vehicleRoot.transform, new Vector3(0f, 0.2f, -0.04f),
                    new Vector3(0.36f, 0.2f, 0.46f), windowMaterial);
                CreateVehicleWheels(vehicleRoot.transform);
                CreateCube("Vehicle Headlights", vehicleRoot.transform, new Vector3(-0.13f, 0.02f, 0.43f),
                    new Vector3(0.09f, 0.09f, 0.035f), laneMaterial);
                CreateCube("Vehicle Headlights", vehicleRoot.transform, new Vector3(0.13f, 0.02f, 0.43f),
                    new Vector3(0.09f, 0.09f, 0.035f), laneMaterial);
                vehicleRoot.AddComponent<CityVehicle>().Configure(
                    start, end, Random.Range(2.2f, 4.8f), Random.value);
            }
        }

        private static void CreateAircraft(Transform root)
        {
            for (var index = 0; index < 2; index++)
            {
                var aircraft = new GameObject("Emergency Helicopter");
                aircraft.transform.SetParent(root, false);
                var body = CreateCube("Helicopter Body", aircraft.transform, Vector3.zero,
                    new Vector3(0.75f, 0.42f, 1.35f),
                    index == 0 ? vehicleMaterials[1] : vehicleMaterials[2]);
                CreateCube("Helicopter Cabin", aircraft.transform, new Vector3(0f, 0.18f, 0.42f),
                    new Vector3(0.58f, 0.32f, 0.52f), windowMaterial);
                CreateCube("Helicopter Tail", aircraft.transform, new Vector3(0f, 0.05f, -1.05f),
                    new Vector3(0.18f, 0.18f, 1.15f), vehicleMaterials[3]);
                var rotor = CreateCube("Main Rotor", aircraft.transform, new Vector3(0f, 0.5f, 0f),
                    new Vector3(2.5f, 0.045f, 0.12f), roadMaterial).transform;

                aircraft.AddComponent<CityAircraft>().Configure(
                    Vector3.zero,
                    12f + index * 3.5f,
                    8f + index * 1.8f,
                    0.22f + index * 0.035f,
                    index * Mathf.PI,
                    true,
                    rotor);
            }

            var plane = new GameObject("Passing Aircraft");
            plane.transform.SetParent(root, false);
            CreateCube("Aircraft Body", plane.transform, Vector3.zero,
                new Vector3(0.42f, 0.38f, 2.2f), vehicleMaterials[0]);
            CreateCube("Aircraft Wings", plane.transform, new Vector3(0f, 0f, 0.15f),
                new Vector3(3.1f, 0.08f, 0.6f), vehicleMaterials[0]);
            CreateCube("Aircraft Tail", plane.transform, new Vector3(0f, 0.35f, -0.82f),
                new Vector3(0.1f, 0.78f, 0.5f), vehicleMaterials[2]);
            plane.AddComponent<CityAircraft>().Configure(
                Vector3.zero, 23f, 13f, 0.12f, 1.2f, false, null);
        }

        private static void CreatePedestrians(Transform root)
        {
            for (var index = 0; index < pedestrianCount; index++)
            {
                var pedestrian = CreateHumanoid(
                    "Civilian",
                    root,
                    pedestrianMaterial,
                    index % 3 == 0 ? darkClothingMaterial : clothingMaterial);

                var horizontalSidewalk = Random.value > 0.5f;
                var roadCoordinate = Random.Range(-5, 6) * 6.2f;
                var sidewalkSide = Random.value > 0.5f ? 1.15f : -1.15f;
                var center = horizontalSidewalk
                    ? new Vector3(Random.Range(-31f, 29f), 0f, roadCoordinate + sidewalkSide)
                    : new Vector3(roadCoordinate + sidewalkSide, 0f, Random.Range(-32f, 32f));
                pedestrian.transform.position = center;
                pedestrian.AddComponent<CityPedestrian>().Configure(
                    center,
                    Random.Range(1.2f, 3.4f),
                    Random.Range(0.65f, 1.05f),
                    Random.Range(0f, Mathf.PI * 2f));
            }
        }

        private static GameObject CreateHumanoid(
            string objectName,
            Transform parent,
            Material skin,
            Material clothing)
        {
            var human = new GameObject(objectName);
            human.transform.SetParent(parent, false);

            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(human.transform, false);
            torso.transform.localPosition = new Vector3(0f, 0.92f, 0f);
            torso.transform.localScale = new Vector3(0.28f, 0.42f, 0.22f);
            torso.GetComponent<Renderer>().sharedMaterial = clothing;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(human.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            head.transform.localScale = Vector3.one * 0.31f;
            head.GetComponent<Renderer>().sharedMaterial = skin;

            CreateLimb("Left Arm", human.transform, new Vector3(-0.27f, 1.08f, 0f), skin);
            CreateLimb("Right Arm", human.transform, new Vector3(0.27f, 1.08f, 0f), skin);
            CreateLimb("Left Leg", human.transform, new Vector3(-0.12f, 0.36f, 0f), darkClothingMaterial);
            CreateLimb("Right Leg", human.transform, new Vector3(0.12f, 0.36f, 0f), darkClothingMaterial);
            return human;
        }

        private static void CreateLimb(
            string limbName,
            Transform parent,
            Vector3 localPosition,
            Material material)
        {
            var limb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            limb.name = limbName;
            limb.transform.SetParent(parent, false);
            limb.transform.localPosition = localPosition;
            limb.transform.localScale = new Vector3(0.11f, 0.3f, 0.11f);
            limb.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateVehicleWheels(Transform vehicle)
        {
            for (var side = -1; side <= 1; side += 2)
            {
                for (var axle = -1; axle <= 1; axle += 2)
                {
                    var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Wheel";
                    wheel.transform.SetParent(vehicle, false);
                    wheel.transform.localScale = new Vector3(0.13f, 0.07f, 0.13f);
                    wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    wheel.transform.localPosition =
                        new Vector3(side * 0.23f, -0.12f, axle * 0.29f);
                    wheel.GetComponent<Renderer>().sharedMaterial = wheelMaterial;
                }
            }
        }

        private static Transform CreateInfectionOrigin(Transform root)
        {
            var origin = CreateHumanoid(
                "First Infected",
                root,
                infectionMaterial,
                infectionMaterial);
            origin.transform.position = new Vector3(2.8f, 0f, -2.8f);
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
            var collider = cube.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            return cube;
        }

        private static void ConfigureRegion(string regionName)
        {
            vehicleCount = 46;
            pedestrianCount = 86;
            towerHeightScale = 1f;
            parkChance = 0.15f;
            residentialChance = 0.32f;

            switch (regionName)
            {
                case "Europe":
                    cityName = "Берлин";
                    citySeed = 104729;
                    vehicleCount = 42;
                    pedestrianCount = 92;
                    towerHeightScale = 0.78f;
                    parkChance = 0.24f;
                    residentialChance = 0.52f;
                    break;
                case "Asia":
                    cityName = "Токио";
                    citySeed = 130363;
                    vehicleCount = 58;
                    pedestrianCount = 118;
                    towerHeightScale = 1.35f;
                    parkChance = 0.09f;
                    residentialChance = 0.22f;
                    break;
                case "North America":
                    cityName = "Нью-Йорк";
                    citySeed = 155921;
                    vehicleCount = 56;
                    pedestrianCount = 104;
                    towerHeightScale = 1.42f;
                    parkChance = 0.12f;
                    residentialChance = 0.22f;
                    break;
                case "Africa":
                    cityName = "Найроби";
                    citySeed = 196613;
                    vehicleCount = 38;
                    pedestrianCount = 88;
                    towerHeightScale = 0.72f;
                    parkChance = 0.18f;
                    residentialChance = 0.48f;
                    break;
                case "South America":
                    cityName = "Сан-Паулу";
                    citySeed = 228017;
                    vehicleCount = 50;
                    pedestrianCount = 108;
                    towerHeightScale = 1.05f;
                    parkChance = 0.17f;
                    residentialChance = 0.43f;
                    break;
                case "Australia":
                    cityName = "Сидней";
                    citySeed = 263167;
                    vehicleCount = 40;
                    pedestrianCount = 78;
                    towerHeightScale = 0.92f;
                    parkChance = 0.28f;
                    residentialChance = 0.5f;
                    break;
                case "Middle East":
                    cityName = "Дубай";
                    citySeed = 299993;
                    vehicleCount = 54;
                    pedestrianCount = 84;
                    towerHeightScale = 1.58f;
                    parkChance = 0.07f;
                    residentialChance = 0.2f;
                    break;
                default:
                    cityName = "Сингапур";
                    citySeed = 26082026;
                    vehicleCount = 52;
                    pedestrianCount = 100;
                    towerHeightScale = 1.22f;
                    parkChance = 0.14f;
                    residentialChance = 0.28f;
                    break;
            }
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
            pedestrianMaterial = MaterialFor("Skin", new Color(0.72f, 0.5f, 0.34f), 0.12f);
            clothingMaterial = MaterialFor("Civilian Clothing", new Color(0.12f, 0.28f, 0.46f), 0.18f);
            darkClothingMaterial = MaterialFor("Dark Clothing", new Color(0.045f, 0.065f, 0.09f), 0.12f);
            wheelMaterial = MaterialFor("Rubber Wheels", new Color(0.012f, 0.014f, 0.018f), 0.04f);
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
            var material = new Material(shader)
            {
                name = materialName,
                color = color,
                enableInstancing = true
            };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }
    }
}
