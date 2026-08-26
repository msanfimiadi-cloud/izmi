using UnityEngine;

namespace Izmi
{
    public sealed class IzmiBootstrap : MonoBehaviour
    {
        private const float GlobeRadius = 3f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (FindAnyObjectByType<IzmiBootstrap>() != null)
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
            CreateAtmosphere(globe.transform);
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
                "Earth Surface",
                new Color(0.025f, 0.17f, 0.32f),
                0.08f,
                0.4f);

            var earthTexture = Resources.Load<Texture2D>("Earth/BlueMarble");
            if (earthTexture != null)
            {
                earthTexture.wrapMode = TextureWrapMode.Repeat;
                earthTexture.filterMode = FilterMode.Trilinear;
                renderer.sharedMaterial.mainTexture = earthTexture;
                renderer.sharedMaterial.color = Color.white;
                globe.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }

            return globe;
        }

        private static void CreateAtmosphere(Transform globe)
        {
            var atmosphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atmosphere.name = "Atmosphere";
            atmosphere.transform.SetParent(globe, false);
            atmosphere.transform.localPosition = Vector3.zero;
            atmosphere.transform.localRotation = Quaternion.identity;
            atmosphere.transform.localScale = Vector3.one * 1.018f;

            var collider = atmosphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var material = CreateMaterial(
                "Atmosphere Glow",
                new Color(0.18f, 0.55f, 1f, 0.08f),
                0f,
                0.1f);

            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }

            atmosphere.GetComponent<Renderer>().sharedMaterial = material;
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
            // The prototype starts on Unity's built-in renderer so it can
            // open reliably before the final URP assets are configured.
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
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
