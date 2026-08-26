using UnityEngine;

namespace Izmi
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private SimulationClock simulationClock;
        private CityPrototypeController cityPrototype;
        private GlobalOutbreakSystem globalOutbreak;
        private PrototypeSessionMenu sessionMenu;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle activeButtonStyle;
        private GUIStyle buttonStyle;
        private GUIStyle eventTextStyle;
        private Texture2D panelTexture;
        private Texture2D activeTexture;
        private bool stylesReady;
        private int mobileTab;
        private Vector2 mobileSummaryScroll;
        private Vector2 mobileStrategyScroll;

        private void Awake()
        {
            simulationClock = GetComponent<SimulationClock>();
            cityPrototype = GetComponent<CityPrototypeController>();
            globalOutbreak = GetComponent<GlobalOutbreakSystem>();
            sessionMenu = GetComponent<PrototypeSessionMenu>();
        }

        private void OnGUI()
        {
            if (simulationClock == null)
            {
                return;
            }

            if (sessionMenu == null)
            {
                sessionMenu = GetComponent<PrototypeSessionMenu>();
            }
            if (sessionMenu != null && sessionMenu.IsOpen)
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

            var virtualWidth = Screen.width / scale;
            var virtualHeight = Screen.height / scale;
            var safeArea = GetVirtualSafeArea(scale);
            var compactLayout = safeArea.width < 900f;
            var globeView = cityPrototype == null || !cityPrototype.IsCityView;

            var menuRect = compactLayout
                ? new Rect(safeArea.xMax - 106f, safeArea.y + 10f, 96f, 44f)
                : new Rect(virtualWidth * 0.5f - 54f, safeArea.y + 18f, 108f, 38f);
            if (sessionMenu != null && GUI.Button(menuRect, "МЕНЮ", buttonStyle))
            {
                sessionMenu.OpenMenu();
            }

            if (compactLayout && globeView)
            {
                var tabWidth = Mathf.Min(156f, (safeArea.width - 132f) * 0.5f);
                var tabY = safeArea.y + 10f;
                var summaryStyle = mobileTab == 0 ? activeButtonStyle : buttonStyle;
                var strategyStyle = mobileTab == 1 ? activeButtonStyle : buttonStyle;

                if (GUI.Button(new Rect(safeArea.x + 10f, tabY, tabWidth, 44f), "СВОДКА", summaryStyle))
                {
                    mobileTab = 0;
                }
                if (GUI.Button(new Rect(safeArea.x + 18f + tabWidth, tabY, tabWidth, 44f), "СТРАТЕГИЯ", strategyStyle))
                {
                    mobileTab = 1;
                }
            }

            var showSummary = !compactLayout || !globeView || mobileTab == 0;
            var panelWidth = compactLayout ? safeArea.width - 20f : 360f;
            var panelHeight = compactLayout
                ? safeArea.height - 74f
                : cityPrototype != null && cityPrototype.IsCityView ? 390f : 610f;
            var panelRect = compactLayout
                ? new Rect(safeArea.x + 10f, safeArea.y + 64f, panelWidth, panelHeight)
                : new Rect(28f, safeArea.y + 26f, panelWidth, panelHeight);

            if (showSummary)
            {
                GUILayout.BeginArea(panelRect, panelStyle);
                if (compactLayout)
                {
                    mobileSummaryScroll = GUILayout.BeginScrollView(mobileSummaryScroll, false, true);
                }

            GUILayout.Label("IZMI  •  ГЛОБАЛЬНОЕ НАБЛЮДЕНИЕ", titleStyle);
            GUILayout.Space(7f);
            GUILayout.Label(
                simulationClock.CurrentDate.ToString("dd.MM.yyyy  •  HH:mm"),
                bodyStyle);
            if (globalOutbreak != null)
            {
                GUILayout.Label("СЛОЖНОСТЬ: " + globalOutbreak.DifficultyName, titleStyle);
            }
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

                if (globalOutbreak.NewsFeed.Count > 0)
                {
                    GUILayout.Space(10f);
                    GUILayout.Label("МИРОВАЯ ЛЕНТА", titleStyle);
                    var visibleNews = compactLayout
                        ? Mathf.Min(6, globalOutbreak.NewsFeed.Count)
                        : Mathf.Min(3, globalOutbreak.NewsFeed.Count);
                    for (var newsIndex = 0; newsIndex < visibleNews; newsIndex++)
                    {
                        GUILayout.Label(globalOutbreak.NewsFeed[newsIndex], eventTextStyle);
                        GUILayout.Space(3f);
                    }
                }
                GUILayout.Space(8f);
            }

            if (cityPrototype != null && cityPrototype.IsCityView &&
                cityPrototype.InfectionSystem != null)
            {
                var infection = cityPrototype.InfectionSystem;
                GUILayout.Label(
                    "ГОРОД: " + cityPrototype.ActiveCityName,
                    bodyStyle);
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
                        GUILayout.Height(compactLayout ? 48f : 32f)))
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

                if (compactLayout)
                {
                    GUILayout.EndScrollView();
                }
                GUILayout.EndArea();
            }

            if (globalOutbreak != null && globeView)
            {
                if (!compactLayout || mobileTab == 1)
                {
                    var strategyRect = compactLayout
                        ? new Rect(safeArea.x + 10f, safeArea.y + 64f, safeArea.width - 20f, safeArea.height - 74f)
                        : new Rect(virtualWidth - 388f, safeArea.y + 26f, 360f, Mathf.Min(780f, virtualHeight - safeArea.y - 40f));
                    DrawGlobalStrategyPanel(strategyRect, compactLayout);
                }
                if (globalOutbreak.HasEndingReport)
                {
                    DrawEndingPanel(scale);
                }
                else if (globalOutbreak.HasOfflineReport)
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

        private void DrawEndingPanel(float scale)
        {
            var virtualWidth = Screen.width / scale;
            var virtualHeight = Screen.height / scale;
            var width = Mathf.Min(620f, virtualWidth - 40f);
            var x = (virtualWidth - width) * 0.5f;
            var y = Mathf.Max(20f, (virtualHeight - 300f) * 0.5f);

            GUILayout.BeginArea(new Rect(x, y, width, 300f), panelStyle);
            GUILayout.Label("ИТОГ МИРОВОГО КРИЗИСА", titleStyle);
            GUILayout.Space(8f);
            GUILayout.Label(globalOutbreak.EndingTitle, bodyStyle);
            GUILayout.Space(10f);
            GUILayout.Label(globalOutbreak.EndingDescription, eventTextStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                "ЖИВЫ: " + globalOutbreak.LivingPopulation.ToString("N0") +
                "   ПОГИБЛО: " + globalOutbreak.TotalDead.ToString("N0"),
                titleStyle);
            GUILayout.Space(8f);

            if (GUILayout.Button("ПРОДОЛЖИТЬ НАБЛЮДЕНИЕ", buttonStyle, GUILayout.Height(40f)))
            {
                globalOutbreak.AcknowledgeEnding();
            }
            GUILayout.EndArea();
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

        private void DrawGlobalStrategyPanel(Rect panelRect, bool compactLayout)
        {
            GUILayout.BeginArea(panelRect, panelStyle);
            mobileStrategyScroll = GUILayout.BeginScrollView(mobileStrategyScroll, false, true);
            GUILayout.Label("МЕЖДУНАРОДНЫЙ КРИЗИСНЫЙ СОВЕТ", titleStyle);
            GUILayout.Space(5f);
            GUILayout.Label(
                "СТРАТЕГИЯ: " + globalOutbreak.StrategicDirection,
                titleStyle);
            GUILayout.Label(
                "ЦЕЛЬ: " + globalOutbreak.CurrentObjective,
                eventTextStyle);
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
            GUILayout.Space(8f);

            GUILayout.Label("СНАБЖЕНИЕ И ИНФРАСТРУКТУРА", titleStyle);
            if (GUILayout.Button("ПРОДОВОЛЬСТВЕННЫЕ КОНВОИ  •  22", buttonStyle, GUILayout.Height(38f)))
            {
                globalOutbreak.OrganizeFoodConvoys();
            }
            if (GUILayout.Button("ПРОИЗВОДСТВО МЕДИКАМЕНТОВ  •  28", buttonStyle, GUILayout.Height(38f)))
            {
                globalOutbreak.ExpandMedicineProduction();
            }
            if (GUILayout.Button("ВОССТАНОВИТЬ ИНФРАСТРУКТУРУ  •  20", buttonStyle, GUILayout.Height(38f)))
            {
                globalOutbreak.RestoreInfrastructure();
            }
            GUILayout.Space(8f);

            GUILayout.Label("КТО ПОЛУЧИТ ОРУЖИЕ  •  СМЕНА: 18", titleStyle);
            GUILayout.Label(globalOutbreak.ArmamentDoctrine, eventTextStyle);
            GUILayout.BeginHorizontal();
            DrawArmamentButton("ГРАЖДАНСКИЕ", 1);
            DrawArmamentButton("ТОЛЬКО АРМИЯ", 2);
            GUILayout.EndHorizontal();
            GUILayout.Space(7f);

            GUILayout.Label("КОМУ ОТДАВАТЬ ПРОДОВОЛЬСТВИЕ  •  СМЕНА: 12", titleStyle);
            GUILayout.Label(globalOutbreak.RationDoctrine, eventTextStyle);
            GUILayout.BeginHorizontal();
            DrawRationButton("МИРНЫМ", 1);
            DrawRationButton("АРМИИ", 2);
            DrawRationButton("ПОСЕЛЕНИЯМ", 3);
            GUILayout.EndHorizontal();
            DrawRationButton("РАВНЫЕ ПАЙКИ", 0);
            GUILayout.Space(9f);

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

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private Rect GetVirtualSafeArea(float scale)
        {
            var safe = Screen.safeArea;
            return new Rect(
                safe.x / scale,
                (Screen.height - safe.yMax) / scale,
                safe.width / scale,
                safe.height / scale);
        }

        private void DrawArmamentButton(string label, int doctrine)
        {
            var style = globalOutbreak.ArmamentDoctrine ==
                (doctrine == 1 ? "ВООРУЖАТЬ ГРАЖДАНСКИХ" : "ОРУЖИЕ ТОЛЬКО ВОЕННЫМ")
                ? activeButtonStyle
                : buttonStyle;
            if (GUILayout.Button(label, style, GUILayout.Height(42f)))
            {
                globalOutbreak.SetArmamentDoctrine(doctrine);
            }
        }

        private void DrawRationButton(string label, int doctrine)
        {
            var selected = doctrine == 0 && globalOutbreak.RationDoctrine == "РАВНЫЕ ПАЙКИ"
                || doctrine == 1 && globalOutbreak.RationDoctrine == "ПРИОРИТЕТ МИРНЫМ"
                || doctrine == 2 && globalOutbreak.RationDoctrine == "ПРИОРИТЕТ АРМИИ"
                || doctrine == 3 && globalOutbreak.RationDoctrine == "ПРИОРИТЕТ ПОСЕЛЕНИЯМ";
            if (GUILayout.Button(label, selected ? activeButtonStyle : buttonStyle, GUILayout.Height(42f)))
            {
                globalOutbreak.SetRationDoctrine(doctrine);
            }
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
