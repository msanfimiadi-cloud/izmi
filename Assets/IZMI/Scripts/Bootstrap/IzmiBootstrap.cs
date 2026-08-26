using UnityEngine;

namespace Izmi
{
    public sealed class IzmiBootstrap : MonoBehaviour
    {
        private const float GlobeRadius = 3f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (FindFirstObjectByType<IzmiBootstrap>() != null)
            {
                return;
            }

            var root = new GameObject("IZMI Prototype");
            root.AddComponent<IzmiBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            CreateEnvironment();
            var globe = CreateGlobe();
            CreateCityMarkers(globe.transform);
            CreateCamera(globe.transform);
        }

        private static void CreateEnvironment()
        {
            RenderSettings.ambientLight = new Color(0.16f, 0.2f, 0.3f);

            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.95f, 0.85f);
            lightObject.transform.rotation = Quaternion.Euler(28f, -34f, 0f);
        }

        private static GameObject CreateGlobe()
        {
            var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globe.name = "Living Earth";
            globe.transform.localScale = Vector3.one * GlobeRadius * 2f;

            var renderer = globe.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial(
                "Earth Ocean",
                new Color(0.025f, 0.17f, 0.32f),
                0.28f,
                0.72f);

            return globe;
        }

        private static void CreateCityMarkers(Transform globe)
        {
            CreateMarker(globe, "Europe", 52.0f, 20.0f, new Color(0.2f, 0.85f, 1f));
            CreateMarker(globe, "Asia", 55.0f, 82.9f, new Color(0.2f, 0.85f, 1f));
            CreateMarker(globe, "North America", 40.7f, -74.0f, new Color(0.2f, 0.85f, 1f));
            CreateMarker(globe, "First anomaly", 1.3f, 103.8f, new Color(1f, 0.16f, 0.08f));
        }

        private static void CreateMarker(
            Transform parent,
            string markerName,
            float latitude,
            float longitude,
            Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = markerName;
            marker.transform.SetParent(parent, false);

            var lat = latitude * Mathf.Deg2Rad;
            var lon = longitude * Mathf.Deg2Rad;
            var localDirection = new Vector3(
                Mathf.Cos(lat) * Mathf.Cos(lon),
                Mathf.Sin(lat),
                Mathf.Cos(lat) * Mathf.Sin(lon));

            marker.transform.localPosition = localDirection * 0.505f;
            marker.transform.localScale = Vector3.one * 0.018f;
            marker.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial(markerName, color, 0f, 0.35f);
        }

        private static void CreateCamera(Transform globe)
        {
            var cameraObject = new GameObject("Globe Camera");
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.005f, 0.008f, 0.018f);
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 500f;

            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.transform.LookAt(Vector3.zero);

            var controller = cameraObject.AddComponent<GlobeCameraController>();
            controller.Configure(globe, 5.3f, 14f);
        }

        private static Material CreateMaterial(
            string materialName,
            Color color,
            float metallic,
            float smoothness)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            return material;
        }
    }
}
