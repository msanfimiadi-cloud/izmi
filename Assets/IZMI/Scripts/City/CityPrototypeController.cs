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
        private Vector3 globeCameraPosition;
        private Quaternion globeCameraRotation;
        private bool transitioning;
        private float commandPoints = 70f;
        private float messageTimer;
        private string commandMessage = "ОЖИДАНИЕ РЕШЕНИЯ";

        public bool IsCityView { get; private set; }
        public PrototypeInfectionSystem InfectionSystem { get; private set; }
        public int CommandPoints => Mathf.FloorToInt(commandPoints);
        public string CommandMessage => commandMessage;
        public string ActiveRegionName { get; private set; } = "First anomaly";

        private void Awake()
        {
            cityRoot = ProceduralCityBuilder.Build();
            InfectionSystem = cityRoot.GetComponent<PrototypeInfectionSystem>();
            responseVisualizer = cityRoot.AddComponent<CityResponseVisualizer>();
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
            var outbreak = GetComponent<GlobalOutbreakSystem>();
            if (outbreak != null && outbreak.SelectedRegion != null)
            {
                ActiveRegionName = outbreak.SelectedRegion.Name;
                responseVisualizer.Apply(outbreak);
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

            StartCoroutine(TransitionToGlobe());
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
