using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Izmi
{
    public sealed class CityPrototypeController : MonoBehaviour
    {
        private GameObject cityRoot;
        private GameObject globe;
        private GameObject starfield;
        private Camera worldCamera;
        private CityResponseVisualizer responseVisualizer;
        private GlobalOutbreakSystem globalOutbreak;
        private GlobalOutbreakSystem.RegionState activeRegion;
        private Vector3 globeCameraPosition;
        private Quaternion globeCameraRotation;
        private bool transitioning;
        private float commandPoints = 70f;
        private float messageTimer;
        private string commandMessage = "ОЖИДАНИЕ РЕШЕНИЯ";
        private string builtRegion = "First anomaly";

        public bool IsCityView { get; private set; }
        public PrototypeInfectionSystem InfectionSystem { get; private set; }
        public int CommandPoints => Mathf.FloorToInt(commandPoints);
        public string CommandMessage => commandMessage;
        public string ActiveRegionName { get; private set; } = "First anomaly";
        public string ActiveCityName => CityNameForRegion(ActiveRegionName);

        private void Awake()
        {
            BuildCity(builtRegion);
        }

        private void BuildCity(string regionName)
        {
            if (cityRoot != null)
            {
                Destroy(cityRoot);
            }

            builtRegion = regionName;
            cityRoot = ProceduralCityBuilder.Build(regionName);
            InfectionSystem = cityRoot.GetComponent<PrototypeInfectionSystem>();
            responseVisualizer = cityRoot.AddComponent<CityResponseVisualizer>();
            cityRoot.AddComponent<CityDayNightController>();
            cityRoot.SetActive(false);
        }

        private void Update()
        {
            commandPoints = Mathf.Min(100f, commandPoints + Time.deltaTime * 0.42f);
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f)
                {
                    commandMessage = "ОЖИДАНИЕ РЕШЕНИЯ";
                }
            }

            if (IsCityView && Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitCity();
            }
        }

        public bool TryQuarantine()
        {
            if (!SpendPoints(30))
            {
                return false;
            }

            InfectionSystem.DeployQuarantine(24f);
            globalOutbreak?.ApplyCityQuarantine(activeRegion);
            SetMessage("КАРАНТИН ВВЕДЁН • РАСПРОСТРАНЕНИЕ ЗАМЕДЛЕНО");
            return true;
        }

        public bool TryMedicalTeams()
        {
            if (InfectionSystem == null || InfectionSystem.InfectedCount <= 0)
            {
                SetMessage("МЕДБРИГАДАМ НЕКОГО ЛЕЧИТЬ");
                return false;
            }

            if (!SpendPoints(40))
            {
                return false;
            }

            var treated = InfectionSystem.TreatInfected(4);
            globalOutbreak?.ApplyCityTreatment(activeRegion, treated);
            SetMessage("ВЫЛЕЧЕНО: " + treated);
            return true;
        }

        public bool TryEvacuation()
        {
            if (InfectionSystem == null || InfectionSystem.HealthyCount <= 0)
            {
                SetMessage("НЕТ ДОСТУПНЫХ ДЛЯ ЭВАКУАЦИИ");
                return false;
            }

            if (!SpendPoints(25))
            {
                return false;
            }

            var evacuated = InfectionSystem.EvacuateHealthy(6);
            globalOutbreak?.ApplyCityEvacuation(activeRegion, evacuated);
            SetMessage("ЭВАКУИРОВАНО: " + evacuated);
            return true;
        }

        private bool SpendPoints(int cost)
        {
            if (InfectionSystem == null)
            {
                return false;
            }

            if (commandPoints < cost)
            {
                SetMessage("НЕДОСТАТОЧНО РЕСУРСА КОМАНДОВАНИЯ");
                return false;
            }

            commandPoints -= cost;
            return true;
        }

        private void SetMessage(string message)
        {
            commandMessage = message;
            messageTimer = 5f;
        }

        public void EnterCity()
        {
            if (IsCityView || transitioning)
            {
                return;
            }

            ResolveWorldObjects();
            globalOutbreak = GetComponent<GlobalOutbreakSystem>();
            if (globalOutbreak != null && globalOutbreak.SelectedRegion != null)
            {
                activeRegion = globalOutbreak.SelectedRegion;
                ActiveRegionName = activeRegion.Name;
                if (builtRegion != ActiveRegionName)
                {
                    BuildCity(ActiveRegionName);
                }
                InfectionSystem.ConfigureRegionalSeverity(
                    activeRegion.Infected,
                    activeRegion.Population);
                responseVisualizer.Apply(globalOutbreak);
            }

            if (worldCamera == null || globe == null)
            {
                return;
            }

            StartCoroutine(TransitionToCity());
        }

        public void ExitCity()
        {
            if (!IsCityView || transitioning)
            {
                return;
            }

            if (globalOutbreak != null && activeRegion != null && InfectionSystem != null)
            {
                globalOutbreak.ReportCityOutbreak(
                    activeRegion,
                    InfectionSystem.InfectionRatio);
            }

            StartCoroutine(TransitionToGlobe());
        }

        private static string CityNameForRegion(string regionName)
        {
            switch (regionName)
            {
                case "Europe": return "БЕРЛИН";
                case "Asia": return "ТОКИО";
                case "North America": return "НЬЮ-ЙОРК";
                case "Africa": return "НАЙРОБИ";
                case "South America": return "САН-ПАУЛУ";
                case "Australia": return "СИДНЕЙ";
                case "Middle East": return "ДУБАЙ";
                default: return "СИНГАПУР";
            }
        }

        private void ResolveWorldObjects()
        {
            worldCamera = Camera.main;
            globe = GameObject.Find("Living Earth");
            starfield = GameObject.Find("Starfield");
        }

        private IEnumerator TransitionToCity()
        {
            transitioning = true;
            globeCameraPosition = worldCamera.transform.position;
            globeCameraRotation = worldCamera.transform.rotation;

            yield return PrototypeScreenFade.FadeTo(1f, 0.45f);

            globe.SetActive(false);
            if (starfield != null)
            {
                starfield.SetActive(false);
            }

            cityRoot.SetActive(true);
            worldCamera.transform.position = new Vector3(0f, 31f, -27f);
            worldCamera.transform.rotation = Quaternion.Euler(47f, 0f, 0f);
            worldCamera.fieldOfView = 48f;
            IsCityView = true;

            yield return PrototypeScreenFade.FadeTo(0f, 0.55f);
            transitioning = false;
        }

        private IEnumerator TransitionToGlobe()
        {
            transitioning = true;
            yield return PrototypeScreenFade.FadeTo(1f, 0.45f);

            cityRoot.SetActive(false);
            globe.SetActive(true);
            if (starfield != null)
            {
                starfield.SetActive(true);
            }

            worldCamera.transform.position = globeCameraPosition;
            worldCamera.transform.rotation = globeCameraRotation;
            worldCamera.fieldOfView = 42f;
            IsCityView = false;

            yield return PrototypeScreenFade.FadeTo(0f, 0.55f);
            transitioning = false;
        }
    }
}
