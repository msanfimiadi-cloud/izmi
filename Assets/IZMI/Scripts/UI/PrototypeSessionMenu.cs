using UnityEngine;
using UnityEngine.SceneManagement;

namespace Izmi
{
    public sealed class PrototypeSessionMenu : MonoBehaviour
    {
        private SimulationClock clock;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private Texture2D panelTexture;
        private bool stylesReady;
        private bool confirmNewWorld;
        private float previousSpeed = 1f;

        public bool IsOpen { get; private set; }
        public bool HasSavedWorld =>
            !string.IsNullOrEmpty(PlayerPrefs.GetString("IZMI.World.DateTicks", string.Empty));

        private void Awake()
        {
            clock = GetComponent<SimulationClock>();
            if (PlayerPrefs.GetInt("IZMI.Session.AutoStart", 0) == 1)
            {
                PlayerPrefs.DeleteKey("IZMI.Session.AutoStart");
                PlayerPrefs.Save();
                IsOpen = false;
                return;
            }

            OpenMenu();
        }

        public void OpenMenu()
        {
            if (IsOpen)
            {
                return;
            }

            if (clock != null)
            {
                previousSpeed = clock.CurrentSpeed > 0f ? clock.CurrentSpeed : 1f;
                clock.SetSpeed(0f);
            }
            IsOpen = true;
            confirmNewWorld = false;
        }

        public void ContinueGame()
        {
            IsOpen = false;
            confirmNewWorld = false;
            if (clock != null)
            {
                clock.SetSpeed(previousSpeed > 0f ? previousSpeed : 1f);
            }
        }

        private void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            EnsureStyles();
            var scale = Mathf.Clamp(Screen.width / 1440f, 0.72f, 1.2f);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            var virtualWidth = Screen.width / scale;
            var virtualHeight = Screen.height / scale;
            var width = Mathf.Min(560f, virtualWidth - 36f);
            var height = confirmNewWorld ? 360f : 330f;
            var rect = new Rect(
                (virtualWidth - width) * 0.5f,
                (virtualHeight - height) * 0.5f,
                width,
                height);

            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Label("IZMI", titleStyle);
            GUILayout.Label("ГЛОБАЛЬНЫЙ ЗОМБИ-КРИЗИС", subtitleStyle);
            GUILayout.Space(14f);
            GUILayout.Label(
                "Наблюдайте за живой планетой, принимайте решения и определите, чем закончится история человечества.",
                subtitleStyle);
            GUILayout.FlexibleSpace();

            if (!confirmNewWorld)
            {
                if (GUILayout.Button(
                        HasSavedWorld ? "ПРОДОЛЖИТЬ МИР" : "НАЧАТЬ СИМУЛЯЦИЮ",
                        buttonStyle,
                        GUILayout.Height(48f)))
                {
                    ContinueGame();
                }

                GUILayout.Space(8f);
                if (HasSavedWorld && GUILayout.Button(
                        "НОВАЯ СИМУЛЯЦИЯ",
                        buttonStyle,
                        GUILayout.Height(42f)))
                {
                    confirmNewWorld = true;
                }
            }
            else
            {
                GUILayout.Label(
                    "Текущий мир, решения и прогресс будут удалены.",
                    subtitleStyle);
                GUILayout.Space(8f);
                if (GUILayout.Button(
                        "ДА, СОЗДАТЬ НОВЫЙ МИР",
                        buttonStyle,
                        GUILayout.Height(44f)))
                {
                    StartNewWorld();
                }

                if (GUILayout.Button(
                        "ОТМЕНА",
                        buttonStyle,
                        GUILayout.Height(38f)))
                {
                    confirmNewWorld = false;
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private void StartNewWorld()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("IZMI.Session.AutoStart", 1);
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void EnsureStyles()
        {
            if (stylesReady)
            {
                return;
            }

            panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            panelTexture.SetPixel(0, 0, new Color(0.018f, 0.035f, 0.06f, 0.97f));
            panelTexture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(28, 28, 24, 24),
                normal = { background = panelTexture }
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.62f, 0.86f, 1f) }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                wordWrap = true,
                normal = { textColor = new Color(0.88f, 0.92f, 0.96f) }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            stylesReady = true;
        }

        private void OnDestroy()
        {
            if (panelTexture != null)
            {
                Destroy(panelTexture);
            }
        }
    }
}
