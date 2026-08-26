using System;
using System.Collections.Generic;
using System.Globalization;
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
            public double Dead;
            public double Recovered;
            public float WaterRisk;
            public float AnimalRisk;
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
        private Material reservoirMaterial;
        private Material routeMaterial;
        private Material flightMaterial;
        private float spreadPulse;
        private int infectedRegions = 1;
        private float autosaveTimer;
        private float responsePoints = 75f;
        private float travelRestrictionTimer;
        private float messageTimer;
        private float warReadiness;
        private float cureResearch;
        private float shelterReadiness;
        private string responseMessage = "СОВЕТ ОЖИДАЕТ РЕШЕНИЯ";
        private int crisisEventIndex;
        private bool crisisEventVisible;
        private float resourcePulse;
        private float foodSupply = 82f;
        private float medicalSupply = 74f;
        private float security = 68f;
        private float publicTrust = 76f;
        private int safeSettlementCount;
        private long protectedPopulation;

        public IReadOnlyList<RegionState> Regions => regions;
        public long TotalPopulation { get; private set; }
        public long TotalInfected { get; private set; }
        public long TotalDead { get; private set; }
        public long TotalRecovered { get; private set; }
        public int InfectedRegions => infectedRegions;
        public RegionState SelectedRegion { get; private set; }
        public int ResponsePoints => Mathf.FloorToInt(responsePoints);
        public int WarReadiness => Mathf.RoundToInt(warReadiness);
        public int CureResearch => Mathf.RoundToInt(cureResearch);
        public int ShelterReadiness => Mathf.RoundToInt(shelterReadiness);
        public string ResponseMessage => responseMessage;
        public bool AreFlightsRestricted => travelRestrictionTimer > 0f;
        public bool HasCrisisEvent => crisisEventVisible;
        public int FoodSupply => Mathf.RoundToInt(foodSupply);
        public int MedicalSupply => Mathf.RoundToInt(medicalSupply);
        public int Security => Mathf.RoundToInt(security);
        public int PublicTrust => Mathf.RoundToInt(publicTrust);
        public int SafeSettlementCount => safeSettlementCount;
        public long ProtectedPopulation => protectedPopulation;
        public long LivingPopulation => Math.Max(0L, TotalPopulation - TotalDead);
        public string HumanityStatus
        {
            get
            {
                if (LivingPopulation <= 100L) return "ПОСЛЕДНИЕ 100";
                if (LivingPopulation <= 1000000L) return "ОСТАТКИ ЧЕЛОВЕЧЕСТВА";
                if (TotalPopulation > 0L && LivingPopulation < TotalPopulation / 2L) return "ГЛОБАЛЬНЫЙ КОЛЛАПС";
                return "ЦИВИЛИЗАЦИЯ ДЕРЖИТСЯ";
            }
        }
        public string WorldCondition
        {
            get
            {
                var weakest = Mathf.Min(Mathf.Min(foodSupply, medicalSupply), Mathf.Min(security, publicTrust));
                if (weakest < 15f) return "КОЛЛАПС";
                if (weakest < 35f) return "КРИТИЧЕСКОЕ";
                if (weakest < 60f) return "НАПРЯЖЁННОЕ";
                return "СТАБИЛЬНОЕ";
            }
        }
        public string CrisisEventTitle
        {
            get
            {
                switch (crisisEventIndex)
                {
                    case 0: return "ПЕРВЫЙ ПОДТВЕРЖДЁННЫЙ ОЧАГ";
                    case 1: return "ЗАРАЖЕНИЕ В МЕЖДУНАРОДНОМ АЭРОПОРТУ";
                    case 2: return "МИЛЛИОНЫ ЛЮДЕЙ ПОКИДАЮТ ГОРОДА";
                    case 3: return "ВИРУС ОБНАРУЖЕН В ВОДОСНАБЖЕНИИ";
                    case 4: return "ЗАРАЖЕНИЕ ПЕРЕШЛО НА ЖИВОТНЫХ";
                    default: return string.Empty;
                }
            }
        }
        public string CrisisEventDescription
        {
            get
            {
                switch (crisisEventIndex)
                {
                    case 0: return "Неизвестная инфекция передаётся людям. Мир ждёт вашего первого приказа.";
                    case 1: return "Самолёты уже разлетелись по миру. Запасы еды и медикаментов зависят от открытых границ.";
                    case 2: return "Дороги переполнены. Военные, врачи и гражданские требуют разных решений.";
                    case 3: return "Даже после лечения людей анализы воды остаются положительными. Очаг может возникнуть снова.";
                    case 4: return "Домашние и дикие животные переносят инфекцию между безопасными зонами.";
                    default: return string.Empty;
                }
            }
        }
        public string StrategicDirection
        {
            get
            {
                var best = Mathf.Max(warReadiness, cureResearch, shelterReadiness);
                if (best < 15f) return "НЕ ОПРЕДЕЛЕНО";
                if (Mathf.Approximately(best, warReadiness)) return "ПОДГОТОВКА К ВОЙНЕ";
                if (Mathf.Approximately(best, cureResearch)) return "ПОИСК ЛЕЧЕНИЯ";
                return "СОХРАНЕНИЕ ЛЮДЕЙ";
            }
        }

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
            LoadRegionalState();
            LoadPolicyState();
            SelectedRegion = regions.Count > 3 ? regions[3] : regions[0];
            RefreshTotals();
            CheckCrisisEvent();
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

            responsePoints = Mathf.Min(100f, responsePoints + Time.deltaTime * 0.16f);
            travelRestrictionTimer = Mathf.Max(0f, travelRestrictionTimer - Time.deltaTime);
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f) responseMessage = "СОВЕТ ОЖИДАЕТ РЕШЕНИЯ";
            }

            SimulateLocalGrowth(Time.deltaTime);
            resourcePulse += Time.deltaTime;
            if (resourcePulse >= 12f)
            {
                resourcePulse = 0f;
                ApplyResourcePressure();
            }

            spreadPulse += Time.deltaTime;
            if (spreadPulse >= 2.4f)
            {
                spreadPulse = 0f;
                SimulateTravelSpread();
            }

            UpdateFlights();
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= 5f)
            {
                autosaveTimer = 0f;
                SaveRegionalState();
            }
            RefreshVisuals();
            RefreshTotals();
            CheckCrisisEvent();
        }

        public bool PurifySelectedWater()
        {
            if (SelectedRegion == null ||
                !HasResources(0f, 6f, 0f, 0f) ||
                !SpendResponsePoints(20f))
            {
                return false;
            }

            medicalSupply -= 6f;
            SelectedRegion.WaterRisk = Mathf.Max(0f, SelectedRegion.WaterRisk - 24f);
            cureResearch = Mathf.Min(100f, cureResearch + 2f);
            SetResponseMessage("ВОДОСНАБЖЕНИЕ РЕГИОНА ОЧИЩЕНО");
            SaveRegionalState();
            return true;
        }

        public bool ControlSelectedAnimals()
        {
            if (SelectedRegion == null ||
                !HasResources(0f, 0f, 6f, 3f) ||
                !SpendResponsePoints(20f))
            {
                return false;
            }

            security -= 6f;
            publicTrust -= 3f;
            SelectedRegion.AnimalRisk = Mathf.Max(0f, SelectedRegion.AnimalRisk - 24f);
            shelterReadiness = Mathf.Min(100f, shelterReadiness + 2f);
            SetResponseMessage("ВЕТЕРИНАРНЫЕ ГРУППЫ РАЗВЁРНУТЫ");
            SaveRegionalState();
            return true;
        }

        private void ApplyResourcePressure()
        {
            var globalRatio = TotalPopulation > 0
                ? TotalInfected / (double)TotalPopulation
                : 0d;
            var crisisPressure = Mathf.Clamp01((float)(globalRatio * 180d));

            foodSupply -= InfectedRegions * 0.18f +
                shelterReadiness * 0.006f +
                safeSettlementCount * 0.16f;
            medicalSupply -= InfectedRegions * 0.14f + crisisPressure * 1.8f;
            security -= InfectedRegions * 0.1f + crisisPressure * 1.25f;
            publicTrust -= InfectedRegions * 0.08f + crisisPressure * 1.4f;

            if (foodSupply < 8f && protectedPopulation > 0L)
            {
                protectedPopulation = Math.Max(0L, protectedPopulation - Math.Max(1000L, protectedPopulation / 40L));
                publicTrust -= 1.5f;
            }

            if (InfectedRegions <= 1)
            {
                foodSupply += 0.2f;
                security += 0.1f;
                publicTrust += 0.12f;
            }

            foodSupply = Mathf.Clamp(foodSupply, 0f, 100f);
            medicalSupply = Mathf.Clamp(medicalSupply, 0f, 100f);
            security = Mathf.Clamp(security, 0f, 100f);
            publicTrust = Mathf.Clamp(publicTrust, 0f, 100f);
        }

        private bool HasResources(float food, float medicine, float order, float trust)
        {
            if (foodSupply >= food && medicalSupply >= medicine &&
                security >= order && publicTrust >= trust)
            {
                return true;
            }

            SetResponseMessage("НЕ ХВАТАЕТ ЗАПАСОВ ДЛЯ ЭТОГО РЕШЕНИЯ");
            return false;
        }

        public string GetCrisisChoiceLabel(int choice)
        {
            switch (crisisEventIndex)
            {
                case 0:
                    return choice == 0 ? "ИЗОЛИРОВАТЬ ЗОНУ"
                        : choice == 1 ? "ПЕРЕДАТЬ ОБРАЗЦЫ УЧЁНЫМ"
                        : "ТИХО ВЫВЕЗТИ СПЕЦИАЛИСТОВ";
                case 1:
                    return choice == 0 ? "ЗАКРЫТЬ ВСЕ РЕЙСЫ"
                        : choice == 1 ? "ОСТАВИТЬ МЕДИЦИНСКИЕ КОРИДОРЫ"
                        : "ЭВАКУИРОВАТЬ ДЕТЕЙ";
                case 2:
                    return choice == 0 ? "ПЕРЕДАТЬ КОНТРОЛЬ ВОЕННЫМ"
                        : choice == 1 ? "МАССОВОЕ ТЕСТИРОВАНИЕ"
                        : "СТРОИТЬ АВТОНОМНЫЕ ПОСЕЛЕНИЯ";
                case 3:
                    return choice == 0 ? "ОЦЕПИТЬ ВОДОХРАНИЛИЩА"
                        : choice == 1 ? "СОЗДАТЬ СИСТЕМУ ОЧИСТКИ"
                        : "ПЕРЕСЕЛИТЬ ЛЮДЕЙ К СКВАЖИНАМ";
                case 4:
                    return choice == 0 ? "УНИЧТОЖИТЬ ЗАРАЖЁННЫЕ СТАИ"
                        : choice == 1 ? "СОЗДАТЬ ВЕТЕРИНАРНУЮ ВАКЦИНУ"
                        : "ИЗОЛИРОВАТЬ ЗАПОВЕДНЫЕ ЗОНЫ";
                default:
                    return string.Empty;
            }
        }

        public void SelectCrisisChoice(int choice)
        {
            if (!crisisEventVisible || choice < 0 || choice > 2)
            {
                return;
            }

            if (choice == 0)
            {
                security = Mathf.Min(100f, security + 7f);
                publicTrust = Mathf.Max(0f, publicTrust - 6f);
                foodSupply = Mathf.Max(0f, foodSupply - 3f);
                warReadiness = Mathf.Min(100f, warReadiness + 14f);
                travelRestrictionTimer = Mathf.Max(travelRestrictionTimer, 36f);
                SetResponseMessage("МИР ВЫБИРАЕТ СИЛОВОЙ ОТВЕТ");
            }
            else if (choice == 1)
            {
                medicalSupply = Mathf.Max(0f, medicalSupply - 6f);
                publicTrust = Mathf.Min(100f, publicTrust + 3f);
                cureResearch = Mathf.Min(100f, cureResearch + 14f);
                SetResponseMessage("УЧЁНЫЕ ПОЛУЧИЛИ НОВЫЕ ДАННЫЕ");
            }
            else
            {
                foodSupply = Mathf.Max(0f, foodSupply - 8f);
                publicTrust = Mathf.Min(100f, publicTrust + 7f);
                shelterReadiness = Mathf.Min(100f, shelterReadiness + 14f);
                SetResponseMessage("НАЧАТА ПОДГОТОВКА БЕЗОПАСНЫХ ЗОН");
            }

            crisisEventVisible = false;
            crisisEventIndex++;
            PlayerPrefs.SetInt("IZMI.Events.Index", crisisEventIndex);
            SavePolicyState();
        }

        private void CheckCrisisEvent()
        {
            if (crisisEventVisible || crisisEventIndex >= 5)
            {
                return;
            }

            var shouldShow = false;
            if (crisisEventIndex == 0) shouldShow = TotalInfected >= 1000L;
            else if (crisisEventIndex == 1) shouldShow = TotalInfected >= 100000L;
            else if (crisisEventIndex == 2) shouldShow = TotalInfected >= 1000000L;
            else
            {
                foreach (var region in regions)
                {
                    if (crisisEventIndex == 3 && region.WaterRisk >= 20f)
                    {
                        shouldShow = true;
                        break;
                    }

                    if (crisisEventIndex == 4 && region.AnimalRisk >= 22f)
                    {
                        shouldShow = true;
                        break;
                    }
                }
            }

            if (shouldShow)
            {
                crisisEventVisible = true;
            }
        }

        public bool InvestInDefense()
        {
            if (!HasResources(4f, 0f, 0f, 5f) || !SpendResponsePoints(25f)) return false;
            foodSupply -= 4f;
            publicTrust -= 5f;
            security = Mathf.Min(100f, security + 9f);
            warReadiness = Mathf.Min(100f, warReadiness + 12f);
            travelRestrictionTimer = Mathf.Max(travelRestrictionTimer, 30f);
            SetResponseMessage("ВОЕННЫЕ ОГРАНИЧИЛИ ПЕРЕМЕЩЕНИЯ");
            SavePolicyState();
            return true;
        }

        public bool FundCureResearch()
        {
            if (!HasResources(0f, 7f, 0f, 0f) || !SpendResponsePoints(35f)) return false;
            medicalSupply -= 7f;
            publicTrust = Mathf.Min(100f, publicTrust + 2f);
            cureResearch = Mathf.Min(100f, cureResearch + 11f);
            SetResponseMessage(cureResearch >= 100f
                ? "ПРОТОТИП ЛЕЧЕНИЯ ГОТОВ"
                : "ИССЛЕДОВАНИЯ УСКОРЕНЫ");
            SavePolicyState();
            return true;
        }

        public bool BuildShelters()
        {
            if (!HasResources(9f, 0f, 3f, 0f) || !SpendResponsePoints(30f)) return false;
            foodSupply -= 9f;
            security -= 3f;
            publicTrust = Mathf.Min(100f, publicTrust + 7f);
            shelterReadiness = Mathf.Min(100f, shelterReadiness + 12f);
            safeSettlementCount++;
            protectedPopulation = Math.Min(
                LivingPopulation,
                protectedPopulation + 150000L + safeSettlementCount * 25000L);
            SetResponseMessage("НОВОЕ ПОСЕЛЕНИЕ ПРИНИМАЕТ ЛЮДЕЙ");
            SavePolicyState();
            return true;
        }

        private bool SpendResponsePoints(float cost)
        {
            if (responsePoints < cost)
            {
                SetResponseMessage("НЕДОСТАТОЧНО РЕСУРСА");
                return false;
            }

            responsePoints -= cost;
            return true;
        }

        private void SetResponseMessage(string message)
        {
            responseMessage = message;
            messageTimer = 6f;
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

        private void LoadPolicyState()
        {
            responsePoints = PlayerPrefs.GetFloat("IZMI.Policy.Points", 75f);
            warReadiness = PlayerPrefs.GetFloat("IZMI.Policy.War", 0f);
            cureResearch = PlayerPrefs.GetFloat("IZMI.Policy.Cure", 0f);
            shelterReadiness = PlayerPrefs.GetFloat("IZMI.Policy.Shelter", 0f);
            foodSupply = PlayerPrefs.GetFloat("IZMI.Resource.Food", 82f);
            medicalSupply = PlayerPrefs.GetFloat("IZMI.Resource.Medicine", 74f);
            security = PlayerPrefs.GetFloat("IZMI.Resource.Security", 68f);
            publicTrust = PlayerPrefs.GetFloat("IZMI.Resource.Trust", 76f);
            safeSettlementCount = PlayerPrefs.GetInt("IZMI.Survival.Settlements", 0);
            long.TryParse(
                PlayerPrefs.GetString("IZMI.Survival.Protected", "0"),
                out protectedPopulation);
            crisisEventIndex = PlayerPrefs.GetInt("IZMI.Events.Index", 0);
        }

        private void SavePolicyState()
        {
            PlayerPrefs.SetFloat("IZMI.Policy.Points", responsePoints);
            PlayerPrefs.SetFloat("IZMI.Policy.War", warReadiness);
            PlayerPrefs.SetFloat("IZMI.Policy.Cure", cureResearch);
            PlayerPrefs.SetFloat("IZMI.Policy.Shelter", shelterReadiness);
            PlayerPrefs.SetFloat("IZMI.Resource.Food", foodSupply);
            PlayerPrefs.SetFloat("IZMI.Resource.Medicine", medicalSupply);
            PlayerPrefs.SetFloat("IZMI.Resource.Security", security);
            PlayerPrefs.SetFloat("IZMI.Resource.Trust", publicTrust);
            PlayerPrefs.SetInt("IZMI.Survival.Settlements", safeSettlementCount);
            PlayerPrefs.SetString("IZMI.Survival.Protected", protectedPopulation.ToString());
            PlayerPrefs.Save();
        }

        private void LoadRegionalState()
        {
            var hasStoredWorld = false;
            foreach (var region in regions)
            {
                var key = "IZMI.Region." + region.Name;
                var stored = PlayerPrefs.GetString(key, string.Empty);
                if (double.TryParse(stored, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    region.Infected = Math.Max(0d, Math.Min(region.Population, value));
                    hasStoredWorld = true;
                }

                var storedDead = PlayerPrefs.GetString(key + ".Dead", string.Empty);
                if (double.TryParse(storedDead, NumberStyles.Float, CultureInfo.InvariantCulture, out var dead))
                {
                    region.Dead = Math.Max(0d, Math.Min(region.Population, dead));
                }

                var storedRecovered = PlayerPrefs.GetString(key + ".Recovered", string.Empty);
                if (double.TryParse(storedRecovered, NumberStyles.Float, CultureInfo.InvariantCulture, out var recovered))
                {
                    region.Recovered = Math.Max(0d, Math.Min(region.Population, recovered));
                }

                region.WaterRisk = PlayerPrefs.GetFloat(key + ".Water", 0f);
                region.AnimalRisk = PlayerPrefs.GetFloat(key + ".Animals", 0f);
            }

            if (!hasStoredWorld)
            {
                return;
            }

            var clock = GetComponent<SimulationClock>();
            var offlineDays = clock != null ? clock.LastOfflineAdvanceMinutes / 1440d : 0d;
            if (offlineDays <= 0d)
            {
                return;
            }

            foreach (var region in regions)
            {
                if (region.Infected > 0d)
                {
                    region.Infected = Math.Min(
                        region.Population,
                        region.Infected * Math.Exp(0.16d * offlineDays));
                }
            }

            var imports = Mathf.Clamp(Mathf.FloorToInt((float)(offlineDays / 2d)), 0, 3);
            for (var index = 0; index < regions.Count && imports > 0; index++)
            {
                if (regions[index].Infected < 1d)
                {
                    regions[index].Infected = 12d + index * 3d;
                    imports--;
                }
            }
        }

        private void SaveRegionalState()
        {
            foreach (var region in regions)
            {
                var key = "IZMI.Region." + region.Name;
                PlayerPrefs.SetString(
                    key,
                    region.Infected.ToString("R", CultureInfo.InvariantCulture));
                PlayerPrefs.SetString(
                    key + ".Dead",
                    region.Dead.ToString("R", CultureInfo.InvariantCulture));
                PlayerPrefs.SetString(
                    key + ".Recovered",
                    region.Recovered.ToString("R", CultureInfo.InvariantCulture));
                PlayerPrefs.SetFloat(key + ".Water", region.WaterRisk);
                PlayerPrefs.SetFloat(key + ".Animals", region.AnimalRisk);
            }
            SavePolicyState();
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveRegionalState();
            }
        }

        private void OnApplicationQuit()
        {
            SaveRegionalState();
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
                if (region.Infected < 1d)
                {
                    continue;
                }

                var availablePopulation = Math.Max(
                    0d,
                    region.Population - region.Dead - region.Recovered);
                var saturation = availablePopulation > 0d
                    ? Math.Max(0d, 1d - region.Infected / availablePopulation)
                    : 0d;
                var protection = Mathf.Clamp01(
                    shelterReadiness * 0.004f +
                    cureResearch * 0.003f +
                    medicalSupply * 0.0012f);
                if (foodSupply < 25f) protection -= 0.12f;
                protection = Mathf.Clamp01(protection);

                var gameDays = deltaTime / 240d;
                var dailyGrowth = 0.24d * (1d - protection) * saturation;
                var newCases = region.Infected * (Math.Exp(dailyGrowth * gameDays) - 1d);
                region.Infected = Math.Min(availablePopulation, region.Infected + Math.Max(0d, newCases));

                var supplyCrisis = 1d + (100d - medicalSupply) / 45d;
                var protectedShare = TotalPopulation > 0L
                    ? Mathf.Clamp01((float)(protectedPopulation / (double)TotalPopulation))
                    : 0f;
                var settlementProtection = 1d - protectedShare * 0.72d;
                var deaths = Math.Min(
                    region.Infected,
                    region.Infected * 0.0045d * supplyCrisis * settlementProtection * gameDays);
                region.Infected -= deaths;
                region.Dead = Math.Min(region.Population, region.Dead + deaths);

                var recoveryRate = 0.0015d + cureResearch / 100d * 0.028d;
                if (cureResearch >= 100f) recoveryRate += 0.16d;
                var recovered = Math.Min(region.Infected, region.Infected * recoveryRate * gameDays);
                region.Infected -= recovered;
                region.Recovered = Math.Min(region.Population - region.Dead, region.Recovered + recovered);

                var infectionPressure = Mathf.Clamp01((float)(region.Infected / 100000d));
                if (region.Infected >= 100d)
                {
                    region.WaterRisk += (0.18f + infectionPressure * 3.2f) * (float)gameDays;
                    region.AnimalRisk += (0.12f + infectionPressure * 2.4f) * (float)gameDays;
                }
                else
                {
                    region.WaterRisk -= 0.025f * (float)gameDays;
                    region.AnimalRisk -= 0.012f * (float)gameDays;
                }

                region.WaterRisk = Mathf.Clamp(region.WaterRisk, 0f, 100f);
                region.AnimalRisk = Mathf.Clamp(region.AnimalRisk, 0f, 100f);
            }
        }

        private void SimulateTravelSpread()
        {
            foreach (var region in regions)
            {
                if (region.Infected < 1d &&
                    (region.WaterRisk >= 35f || region.AnimalRisk >= 35f))
                {
                    var reservoirRisk = Mathf.Max(region.WaterRisk, region.AnimalRisk) / 100f;
                    if (UnityEngine.Random.value < reservoirRisk * 0.24f)
                    {
                        region.Infected = UnityEngine.Random.Range(5, 24);
                        SetResponseMessage("ИНФЕКЦИЯ ВЕРНУЛАСЬ ИЗ ОКРУЖАЮЩЕЙ СРЕДЫ");
                        return;
                    }
                }

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
                var borderControl =
                    (1f - warReadiness * 0.0065f) *
                    Mathf.Lerp(1.35f, 0.72f, publicTrust / 100f);
                if (AreFlightsRestricted) borderControl *= 0.18f;
                if (UnityEngine.Random.value < (0.08f + pressure * 0.32f) * borderControl)
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
                else if (region.WaterRisk >= 20f || region.AnimalRisk >= 20f)
                {
                    region.MarkerRenderer.sharedMaterial = reservoirMaterial;
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
            long dead = 0L;
            long recovered = 0L;
            long population = 0L;
            var affected = 0;
            foreach (var region in regions)
            {
                population += region.Population;
                var regionalInfected = (long)Math.Min(region.Population, Math.Floor(region.Infected));
                infected += regionalInfected;
                dead += (long)Math.Floor(region.Dead);
                recovered += (long)Math.Floor(region.Recovered);
                if (regionalInfected > 0)
                {
                    affected++;
                }
            }

            TotalPopulation = population;
            TotalInfected = infected;
            TotalDead = dead;
            TotalRecovered = recovered;
            infectedRegions = affected;
        }

        private void CreateMaterials()
        {
            healthyMaterial = CreateEmissive("Stable Region", new Color(0.08f, 0.62f, 0.92f), 1.5f);
            warningMaterial = CreateEmissive("Affected Region", new Color(1f, 0.36f, 0.035f), 2f);
            criticalMaterial = CreateEmissive("Critical Region", new Color(0.95f, 0.025f, 0.015f), 2.4f);
            reservoirMaterial = CreateEmissive("Environmental Reservoir", new Color(0.62f, 0.12f, 0.92f), 2f);
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
