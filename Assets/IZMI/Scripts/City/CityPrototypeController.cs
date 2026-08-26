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
        private Vector3 globeCameraPosition;
        private Quaternion globeCameraRotation;
        private bool transitioning;

        public bool IsCityView { get; private set; }

        private void Awake()
        {
            cityRoot = ProceduralCityBuilder.Build();
            cityRoot.SetActive(false);
        }

        private void Update()
        {
            if (IsCityView && Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitCity();
            }
        }

        public void EnterCity()
        {
            if (IsCityView || transitioning)
            {
                return;
            }

            ResolveWorldObjects();
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
