using UnityEngine;

namespace Izmi
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private SimulationClock simulationClock;
        private CityPrototypeController cityPrototype;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle activeButtonStyle;
        private GUIStyle buttonStyle;
        private Texture2D panelTexture;
        private Texture2D activeTexture;
        private bool stylesReady;

        private void Awake()
        {
            simulationClock = GetComponent<SimulationClock>();
            cityPrototype = GetComponent<CityPrototypeController>();
        }

        private void OnGUI()
        {
            if (simulationClock == null)
            {
                return;
            }

            EnsureStyles();

            var scale = Mathf.Clamp(Screen.width / 1440f, 0.72f, 1.25f);
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            var panelWidth = 360f;
            var panelHeight =
                cityPrototype != null && cityPrototype.IsCityView
                    ? 226f
                    : 198f;
            GUILayout.BeginArea(
                new Rect(28f, 26f, panelWidth, panelHeight),
                panelStyle);

            GUILayout.Label("IZMI  •  ГЛОБАЛЬНОЕ НАБЛЮДЕНИЕ", titleStyle);
            GUILayout.Space(7f);
            GUILayout.Label(
                simulationClock.CurrentDate.ToString("dd.MM.yyyy  •  HH:mm"),
                bodyStyle);
            GUILayout.Space(9f);

            GUILayout.BeginHorizontal();
            DrawSpeedButton("Ⅱ", 0f);
            DrawSpeedButton("×1", 1f);
            DrawSpeedButton("×5", 5f);
            DrawSpeedButton("×20", 20f);
            GUILayout.EndHorizontal();
            GUILayout.Space(9f);

            if (cityPrototype != null && cityPrototype.IsCityView &&
                cityPrototype.InfectionSystem != null)
            {
                var infection = cityPrototype.InfectionSystem;
                GUILayout.Label(
                    "ЗАРАЖЕНО: " + infection.InfectedCount +
                    " / " + infection.PopulationCount,
                    titleStyle);
                GUILayout.Space(5f);
            }

            if (cityPrototype != null)
            {
                var navigationLabel = cityPrototype.IsCityView
                    ? "←  ВЕРНУТЬСЯ К ПЛАНЕТЕ"
                    : "ПРИБЛИЗИТЬСЯ К ГОРОДУ  →";

                if (GUILayout.Button(
                        navigationLabel,
                        buttonStyle,
                        GUILayout.Height(32f)))
                {
                    if (cityPrototype.IsCityView)
                    {
                        cityPrototype.ExitCity();
                    }
                    else
                    {
                        cityPrototype.EnterCity();
                    }
                }
            }

            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
        }

        private void DrawSpeedButton(string label, float speed)
        {
            var style = Mathf.Approximately(simulationClock.CurrentSpeed, speed)
                ? activeButtonStyle
                : buttonStyle;

            if (GUILayout.Button(label, style, GUILayout.Width(68f), GUILayout.Height(34f)))
            {
                simulationClock.SetSpeed(speed);
            }
        }

        private void EnsureStyles()
        {
            if (stylesReady)
            {
                return;
            }

            panelTexture = CreateSolidTexture(new Color(0.025f, 0.045f, 0.075f, 0.9f));
            activeTexture = CreateSolidTexture(new Color(0.08f, 0.42f, 0.68f, 0.95f));

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(18, 18, 15, 15),
                normal = { background = panelTexture }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.62f, 0.84f, 1f) }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.82f, 0.88f, 0.94f) }
            };

            activeButtonStyle = new GUIStyle(buttonStyle);
            activeButtonStyle.normal.background = activeTexture;
            activeButtonStyle.normal.textColor = Color.white;
            activeButtonStyle.hover.background = activeTexture;
            activeButtonStyle.active.background = activeTexture;

            stylesReady = true;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            if (panelTexture != null)
            {
                Destroy(panelTexture);
            }

            if (activeTexture != null)
            {
                Destroy(activeTexture);
            }
        }
    }
}
