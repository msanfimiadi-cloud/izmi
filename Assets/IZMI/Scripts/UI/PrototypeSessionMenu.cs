using System;
using System.Reflection;
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
        private int selectedDifficulty = 1;

        public bool IsOpen { get; private set; }
        public bool HasSavedWorld =>
            !string.IsNullOrEmpty(PlayerPrefs.GetString("IZMI.World.DateTicks", string.Empty));

        private void Awake()
        {
            clock = GetComponent<SimulationClock>();
            selectedDifficulty = Mathf.Clamp(PlayerPrefs.GetInt("IZMI.World.Difficulty", 1), 0, 2);
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
            if (!HasSavedWorld)
            {
                PlayerPrefs.SetInt("IZMI.World.Difficulty", selectedDifficulty);
                PlayerPrefs.Save();
            }
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

            var safe = Screen.safeArea;
            var safeRect = new Rect(
                safe.x / scale,
                (Screen.height - safe.yMax) / scale,
                safe.width / scale,
                safe.height / scale);
            var width = Mathf.Min(560f, safeRect.width - 24f);
            var showDifficulty = !HasSavedWorld || confirmNewWorld;
            var desiredHeight = showDifficulty ? 430f : 330f;
            var height = Mathf.Min(desiredHeight, safeRect.height - 24f);
            var rect = new Rect(
                safeRect.x + (safeRect.width - width) * 0.5f,
                safeRect.y + (safeRect.height - height) * 0.5f,
                width,
                height);

            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Label("IZMI", titleStyle);
            GUILayout.Label("ГЛОБАЛЬНЫЙ ЗОМБИ-КРИЗИС", subtitleStyle);
            GUILayout.Space(14f);
            GUILayout.Label(
                "Наблюдайте за живой планетой, принимайте решения и определите, чем закончится история человечества.",
                subtitleStyle);
            GUILayout.Space(12f);
            if (showDifficulty)
            {
                DrawDifficultySelector();
            }
            else
            {
                GUILayout.Label(
                    "СЛОЖНОСТЬ МИРА: " + DifficultyLabel(selectedDifficulty),
                    subtitleStyle);
            }
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
            PlayerPrefs.SetInt("IZMI.World.Difficulty", selectedDifficulty);
            PlayerPrefs.SetInt("IZMI.Session.AutoStart", 1);
            PlayerPrefs.Save();
            Time.timeScale = 1f;

            SceneManager.sceneLoaded += RebuildPrototypeAfterReload;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static void RebuildPrototypeAfterReload(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= RebuildPrototypeAfterReload;

            var assembly = typeof(PrototypeSessionMenu).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (string.IsNullOrEmpty(type.Namespace) ||
                    !type.Namespace.StartsWith("Izmi", StringComparison.Ordinal))
                {
                    continue;
                }

                var methods = type.GetMethods(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);
                foreach (var method in methods)
                {
                    if (method.GetParameters().Length != 0 ||
                        method.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>() == null)
                    {
                        continue;
                    }

                    method.Invoke(null, null);
                }
            }
        }

        private void DrawDifficultySelector()
        {
            GUILayout.Label("ВЫБЕРИТЕ СЛОЖНОСТЬ", subtitleStyle);
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            DrawDifficultyButton(0, "ИСТОРИЯ\nМЕДЛЕННЕЕ");
            DrawDifficultyButton(1, "КРИЗИС\nСТАНДАРТ");
            DrawDifficultyButton(2, "ВЫМИРАНИЕ\nБЫСТРЕЕ");
            GUILayout.EndHorizontal();
        }

        private void DrawDifficultyButton(int level, string label)
        {
            var previousColor = GUI.backgroundColor;
            if (selectedDifficulty == level)
            {
                GUI.backgroundColor = new Color(0.16f, 0.62f, 0.96f);
            }

            if (GUILayout.Button(label, buttonStyle, GUILayout.Height(54f)))
            {
                selectedDifficulty = level;
            }
            GUI.backgroundColor = previousColor;
        }

        private static string DifficultyLabel(int level)
        {
            if (level == 0) return "ИСТОРИЯ";
            if (level == 2) return "ВЫМИРАНИЕ";
            return "КРИЗИС";
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
