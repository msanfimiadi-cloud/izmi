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
            root.AddComponent<SimulationClock>();
            root.AddComponent<PrototypeHud>();
            root.AddComponent<IzmiBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            CreateEnvironment();
            CreateStarfield();
            var globe = CreateGlobe();
            globe.AddComponent<PlanetIdleRotation>();
            CreateAtmosphere(globe.transform);
            CreateCloudLayer(globe.transform);
            CreateCityMarkers(globe.transform);
            CreateCamera(globe.transform);
        }

        private static void CreateEnvironment()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.025f, 0.035f, 0.06f);

            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.82f;
            light.color = new Color(1f, 0.94f, 0.82f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(18f, -38f, 6f);
            lightObject.AddComponent<SunOrbit>();
        }

        private static void CreateStarfield()
        {
            var starObject = new GameObject("Starfield");
            var particles = starObject.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 700;
            main.startLifetime = float.MaxValue;
            main.startSpeed = 0f;
            main.startSize = 0.035f;

            var emission = particles.emission;
            emission.enabled = false;

            var shape = particles.shape;
            shape.enabled = false;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;

            var shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            }

            if (shader != null)
            {
                var material = new Material(shader)
                {
                    name = "Starfield Material"
                };
                material.color = Color.white;
                renderer.sharedMaterial = material;
            }

            const int starCount = 650;
            var starParticles = new ParticleSystem.Particle[starCount];
            var randomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(17031996);

            for (var index = 0; index < starCount; index++)
            {
                var direction = UnityEngine.Random.onUnitSphere;
                var brightness = UnityEngine.Random.Range(0.45f, 1f);
                var tint = UnityEngine.Random.value > 0.88f
                    ? new Color(0.65f, 0.78f, 1f, brightness)
                    : new Color(1f, 0.96f, 0.86f, brightness);

                starParticles[index].position =
                    direction * UnityEngine.Random.Range(28f, 48f);
                starParticles[index].startColor = tint;
                starParticles[index].startSize =
                    UnityEngine.Random.Range(0.018f, 0.055f);
                starParticles[index].remainingLifetime = float.MaxValue;
            }

            UnityEngine.Random.state = randomState;
            particles.SetParticles(starParticles, starParticles.Length);
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
                0f,
                0.14f);

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
            atmosphere.transform.localScale = Vector3.one * 1.035f;

            var collider = atmosphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var shader = Shader.Find("IZMI/Atmosphere");
            if (shader != null)
            {
                var material = new Material(shader)
                {
                    name = "Atmosphere Glow"
                };
                material.SetColor("_Color", new Color(0.12f, 0.5f, 1f, 0.72f));
                material.SetFloat("_Power", 2.6f);
                atmosphere.GetComponent<Renderer>().sharedMaterial = material;
            }
            else
            {
                atmosphere.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                    "Atmosphere Fallback",
                    new Color(0.15f, 0.4f, 0.8f, 0.08f),
                    0f,
                    0f);
            }
        }

        private static void CreateCloudLayer(Transform globe)
        {
            var clouds = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            clouds.name = "Moving Clouds";
            clouds.transform.SetParent(globe, false);
            clouds.transform.localPosition = Vector3.zero;
            clouds.transform.localRotation = Quaternion.identity;
            clouds.transform.localScale = Vector3.one * 1.009f;

            var collider = clouds.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var material = CreateMaterial(
                "Cloud Layer",
                new Color(1f, 1f, 1f, 0.48f),
                0f,
                0.15f);

            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2990;
            }

            material.mainTexture = CreateProceduralCloudTexture();
            clouds.GetComponent<Renderer>().sharedMaterial = material;
            clouds.AddComponent<CloudLayerMotion>();
        }

        private static Texture2D CreateProceduralCloudTexture()
        {
            const int width = 512;
            const int height = 256;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true)
            {
                name = "Procedural Global Clouds",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var u = x / (float)width;
                    var v = y / (float)height;
                    var broad = Mathf.PerlinNoise(u * 5.8f + 1.7f, v * 4.2f + 8.3f);
                    var detail = Mathf.PerlinNoise(u * 15.4f + 4.1f, v * 11.8f + 2.6f);
                    var noise = broad * 0.72f + detail * 0.28f;
                    var alpha = Mathf.SmoothStep(0.49f, 0.68f, noise) * 0.68f;
                    var byteAlpha = (byte)Mathf.RoundToInt(alpha * 255f);
                    pixels[y * width + x] = new Color32(244, 249, 255, byteAlpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
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
            marker.transform.localScale = Vector3.one * 0.014f;

            var markerMaterial = CreateMaterial(markerName, color, 0f, 0.25f);
            if (markerMaterial.HasProperty("_EmissionColor"))
            {
                markerMaterial.EnableKeyword("_EMISSION");
                markerMaterial.SetColor("_EmissionColor", color * 1.8f);
            }

            marker.GetComponent<Renderer>().sharedMaterial = markerMaterial;
            marker.AddComponent<MarkerPulse>();
        }

        private static void CreateCamera(Transform globe)
        {
            var cameraObject = new GameObject("Globe Camera");
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.001f, 0.002f, 0.008f);
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
