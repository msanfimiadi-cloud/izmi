using UnityEngine;

namespace Izmi
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private SimulationClock simulationClock;
        private CityPrototypeController cityPrototype;
        private GlobalOutbreakSystem globalOutbreak;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle activeButtonStyle;
        private GUIStyle buttonStyle;
        private GUIStyle eventTextStyle;
        private Texture2D panelTexture;
        private Texture2D activeTexture;
        private bool stylesReady;

        private void Awake()
        {
            simulationClock = GetComponent<SimulationClock>();
            cityPrototype = GetComponent<CityPrototypeController>();
            globalOutbreak = GetComponent<GlobalOutbreakSystem>();
        }

        private void OnGUI()
        {
            if (simulationClock == null)
            {
                return;
            }

            EnsureStyles();
            if (globalOutbreak == null)
            {
                globalOutbreak = GetComponent<GlobalOutbreakSystem>();
            }

            var scale = Mathf.Clamp(Screen.width / 1440f, 0.72f, 1.25f);
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            var panelWidth = 360f;
            var panelHeight =
                cityPrototype != null && cityPrototype.IsCityView
                    ? 390f
                    : 468f;
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

            if (globalOutbreak != null &&
                (cityPrototype == null || !cityPrototype.IsCityView))
            {
                GUILayout.Label("ГЛОБАЛЬНАЯ ОБСТАНОВКА", titleStyle);
                GUILayout.Space(3f);
                GUILayout.Label(
                    "ЗАРАЖЕНО: " + globalOutbreak.TotalInfected.ToString("N0"),
                    titleStyle);
                GUILayout.Label(
                    "ПОГИБЛО: " + globalOutbreak.TotalDead.ToString("N0") +
                    "   ВЫЗДОРОВЕЛО: " + globalOutbreak.TotalRecovered.ToString("N0"),
                    titleStyle);
                GUILayout.Label(
                    "ЖИВЫ: " + globalOutbreak.LivingPopulation.ToString("N0") +
                    "   " + globalOutbreak.HumanityStatus,
                    titleStyle);
                GUILayout.Label(
                    "ЗАТРОНУТО РЕГИОНОВ: " + globalOutbreak.InfectedRegions +
                    " / " + globalOutbreak.Regions.Count,
                    titleStyle);

                var selected = globalOutbreak.SelectedRegion;
                if (selected != null)
                {
                    GUILayout.Space(5f);
                    GUILayout.Label(
                        "ВЫБРАНО: " + LocalizeRegion(selected.Name),
                        titleStyle);
                    GUILayout.Label(
                        "НАСЕЛЕНИЕ: " + selected.Population.ToString("N0") +
                        "   ЗАРАЖЕНО: " + ((long)selected.Infected).ToString("N0"),
                        titleStyle);
                    GUILayout.Label(
                        "ПОГИБЛО: " + ((long)selected.Dead).ToString("N0") +
                        "   ВЫЗДОРОВЕЛО: " + ((long)selected.Recovered).ToString("N0"),
                        titleStyle);
                    GUILayout.Label(
                        "ВОДА: " + Mathf.RoundToInt(selected.WaterRisk) + "%   ЖИВОТНЫЕ: " +
                        Mathf.RoundToInt(selected.AnimalRisk) + "%",
                        titleStyle);
                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("ОЧИСТИТЬ ВОДУ\n20", buttonStyle, GUILayout.Height(42f)))
                    {
                        globalOutbreak.PurifySelectedWater();
                    }
                    if (GUILayout.Button("ВЕТЕРИНАРНЫЕ ГРУППЫ\n20", buttonStyle, GUILayout.Height(42f)))
                    {
                        globalOutbreak.ControlSelectedAnimals();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(8f);
            }

            if (cityPrototype != null && cityPrototype.IsCityView &&
                cityPrototype.InfectionSystem != null)
            {
                var infection = cityPrototype.InfectionSystem;
                GUILayout.Label(
                    "РЕГИОН: " + LocalizeRegion(cityPrototype.ActiveRegionName),
                    titleStyle);
                GUILayout.Space(3f);
                GUILayout.Label(
                    "УРОВЕНЬ УГРОЗЫ: " + infection.AlertLevel,
                    titleStyle);
                GUILayout.Space(3f);
                GUILayout.Label(
                    "ЗАРАЖЕНО: " + infection.InfectedCount +
                    "   ЗДОРОВЫ: " + infection.HealthyCount,
                    titleStyle);
                DrawInfectionBar(infection.InfectionRatio);
                GUILayout.Space(5f);
                GUILayout.Label(
                    "ЭВАКУИРОВАНО: " + infection.EvacuatedCount +
                    (infection.IsQuarantineActive
                        ? "   КАРАНТИН: " + Mathf.CeilToInt(infection.QuarantineSeconds) + "с"
                        : string.Empty),
                    titleStyle);
                GUILayout.Space(7f);
                GUILayout.Label(
                    "РЕСУРС КОМАНДОВАНИЯ: " + cityPrototype.CommandPoints + " / 100",
                    titleStyle);
                DrawCommandBar(cityPrototype.CommandPoints / 100f);
                GUILayout.Space(5f);
                GUILayout.Label(cityPrototype.CommandMessage, titleStyle);
                GUILayout.Space(6f);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("КАРАНТИН\n30", buttonStyle, GUILayout.Height(44f)))
                {
                    cityPrototype.TryQuarantine();
                }
                if (GUILayout.Button("МЕДБРИГАДЫ\n40", buttonStyle, GUILayout.Height(44f)))
                {
                    cityPrototype.TryMedicalTeams();
                }
                if (GUILayout.Button("ЭВАКУАЦИЯ\n25", buttonStyle, GUILayout.Height(44f)))
                {
                    cityPrototype.TryEvacuation();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(7f);
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

            if (globalOutbreak != null &&
                (cityPrototype == null || !cityPrototype.IsCityView))
            {
                DrawGlobalStrategyPanel(scale);
                if (globalOutbreak.HasOfflineReport)
                {
                    DrawOfflineReportPanel(scale);
                }
                else if (globalOutbreak.HasCrisisEvent)
                {
                    DrawCrisisEventPanel(scale);
                }
            }

            GUI.matrix = previousMatrix;
        }

        private void DrawOfflineReportPanel(float scale)
        {
            var virtualWidth = Screen.width / scale;
            var virtualHeight = Screen.height / scale;
            var width = Mathf.Min(560f, virtualWidth - 40f);
            var x = (virtualWidth - width) * 0.5f;
            var y = Mathf.Max(20f, virtualHeight - 268f);

            GUILayout.BeginArea(new Rect(x, y, width, 238f), panelStyle);
            GUILayout.Label("МИР ЖИЛ БЕЗ ВАС", titleStyle);
            GUILayout.Space(5f);
            GUILayout.Label(
                "ПРОШЛО: " + FormatOfflineTime(globalOutbreak.OfflineElapsedGameMinutes),
                bodyStyle);
            GUILayout.Space(6f);
            GUILayout.Label(
                "НОВЫХ ЗАРАЖЕНИЙ: " + globalOutbreak.OfflineNewInfections.ToString("N0"),
                eventTextStyle);
            GUILayout.Label(
                "НОВЫХ ЗАТРОНУТЫХ РЕГИОНОВ: " + globalOutbreak.OfflineNewRegions,
                eventTextStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("ПРИНЯТЬ ДОКЛАД", buttonStyle, GUILayout.Height(38f)))
            {
                globalOutbreak.DismissOfflineReport();
            }
            GUILayout.EndArea();
        }

        private static string FormatOfflineTime(double minutes)
        {
            if (minutes >= 1440d)
            {
                return (minutes / 1440d).ToString("0.0") + " ДН.";
            }

            if (minutes >= 60d)
            {
                return (minutes / 60d).ToString("0.0") + " Ч.";
            }

            return Mathf.Max(1, Mathf.RoundToInt((float)minutes)) + " МИН.";
        }

        private void DrawCrisisEventPanel(float scale)
        {
            var virtualWidth = Screen.width / scale;
            var virtualHeight = Screen.height / scale;
            var width = Mathf.Min(580f, virtualWidth - 40f);
            var x = (virtualWidth - width) * 0.5f;
            var y = Mathf.Max(20f, virtualHeight - 282f);

            GUILayout.BeginArea(new Rect(x, y, width, 252f), panelStyle);
            GUILayout.Label("СРОЧНОЕ СОБЫТИЕ", titleStyle);
            GUILayout.Space(4f);
            GUILayout.Label(globalOutbreak.CrisisEventTitle, bodyStyle);
            GUILayout.Space(5f);
            GUILayout.Label(globalOutbreak.CrisisEventDescription, eventTextStyle);
            GUILayout.FlexibleSpace();

            for (var choice = 0; choice < 3; choice++)
            {
                var selectedChoice = choice;
                if (GUILayout.Button(
                        globalOutbreak.GetCrisisChoiceLabel(choice),
                        buttonStyle,
                        GUILayout.Height(34f)))
                {
                    globalOutbreak.SelectCrisisChoice(selectedChoice);
                }
            }

            GUILayout.EndArea();
        }

        private static string LocalizeRegion(string regionName)
        {
            switch (regionName)
            {
                case "Europe": return "ЕВРОПА";
                case "Asia": return "АЗИЯ";
                case "North America": return "СЕВЕРНАЯ АМЕРИКА";
                case "First anomaly": return "ЮГО-ВОСТОЧНАЯ АЗИЯ";
                case "Africa": return "АФРИКА";
                case "South America": return "ЮЖНАЯ АМЕРИКА";
                case "Australia": return "АВСТРАЛИЯ";
                case "Middle East": return "БЛИЖНИЙ ВОСТОК";
                default: return regionName.ToUpperInvariant();
            }
        }

        private void DrawGlobalStrategyPanel(float scale)
        {
            var virtualWidth = Screen.width / scale;
            var wideLayout = virtualWidth >= 900f;
            var x = wideLayout ? virtualWidth - 388f : 28f;
            var y = wideLayout ? 26f : 350f;

            GUILayout.BeginArea(new Rect(x, y, 360f, 506f), panelStyle);
            GUILayout.Label("МЕЖДУНАРОДНЫЙ КРИЗИСНЫЙ СОВЕТ", titleStyle);
            GUILayout.Space(5f);
            GUILayout.Label(
                "СТРАТЕГИЯ: " + globalOutbreak.StrategicDirection,
                titleStyle);
            GUILayout.Label(
                "РЕСУРС РЕАГИРОВАНИЯ: " + globalOutbreak.ResponsePoints + " / 100",
                titleStyle);
            DrawCommandBar(globalOutbreak.ResponsePoints / 100f);
            GUILayout.Space(7f);

            GUILayout.Label("СОСТОЯНИЕ МИРА: " + globalOutbreak.WorldCondition, titleStyle);
            GUILayout.Label(
                "ПОСЕЛЕНИЯ: " + globalOutbreak.SafeSettlementCount +
                "   ПОД ЗАЩИТОЙ: " + globalOutbreak.ProtectedPopulation.ToString("N0"),
                titleStyle);
            DrawResourceRow("ЕДА", globalOutbreak.FoodSupply);
            DrawResourceRow("МЕДИКАМЕНТЫ", globalOutbreak.MedicalSupply);
            DrawResourceRow("БЕЗОПАСНОСТЬ", globalOutbreak.Security);
            DrawResourceRow("ДОВЕРИЕ", globalOutbreak.PublicTrust);
            GUILayout.Space(7f);

            GUILayout.Label("ВОЕННАЯ ГОТОВНОСТЬ  " + globalOutbreak.WarReadiness + "%", titleStyle);
            DrawCommandBar(globalOutbreak.WarReadiness / 100f);
            GUILayout.Label("ИССЛЕДОВАНИЕ ЛЕЧЕНИЯ  " + globalOutbreak.CureResearch + "%", titleStyle);
            DrawCommandBar(globalOutbreak.CureResearch / 100f);
            GUILayout.Label("ГОТОВНОСТЬ УБЕЖИЩ  " + globalOutbreak.ShelterReadiness + "%", titleStyle);
            DrawCommandBar(globalOutbreak.ShelterReadiness / 100f);
            GUILayout.Space(7f);

            GUILayout.Label(globalOutbreak.ResponseMessage, titleStyle);
            GUILayout.Space(6f);

            if (GUILayout.Button("МОБИЛИЗАЦИЯ И ГРАНИЦЫ  •  25", buttonStyle, GUILayout.Height(36f)))
            {
                globalOutbreak.InvestInDefense();
            }
            if (GUILayout.Button("ФИНАНСИРОВАТЬ ЛЕЧЕНИЕ  •  35", buttonStyle, GUILayout.Height(36f)))
            {
                globalOutbreak.FundCureResearch();
            }
            if (GUILayout.Button("СТРОИТЬ УБЕЖИЩА  •  30", buttonStyle, GUILayout.Height(36f)))
            {
                globalOutbreak.BuildShelters();
            }

            GUILayout.EndArea();
        }

        private void DrawInfectionBar(float ratio)
        {
            var rect = GUILayoutUtility.GetRect(300f, 10f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, panelTexture);
            var fill = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(ratio), rect.height);
            GUI.DrawTexture(fill, activeTexture);
        }

        private void DrawResourceRow(string label, int value)
        {
            GUILayout.Label(label + "  " + value + "%", titleStyle);
            DrawCommandBar(value / 100f);
        }

        private void DrawCommandBar(float ratio)
        {
            var rect = GUILayoutUtility.GetRect(300f, 7f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, panelTexture);
            var fill = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(ratio), rect.height);
            GUI.DrawTexture(fill, activeTexture);
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

            eventTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.93f, 0.96f) }
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
