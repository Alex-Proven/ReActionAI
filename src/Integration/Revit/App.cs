using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace ReActionAI.Integration.Revit
{
    /// <summary>
    /// Revit Application Add-in:
    /// - создаёт вкладку ReActionAI и панель AI Tools;
    /// - регистрирует док-панель Ассистент (справа);
    /// - добавляет кнопку Ассистент.
    /// </summary>
    public class App : IExternalApplication
    {
        // --- Константы UI ---

        private const string TabName = "ReActionAI";
        private const string PanelName = "AI Tools";
        private const string ButtonName = "Assistant";
        private const string ButtonText = "Ассистент";
        private const string ButtonTooltip = "Открыть панель ассистента ReActionAI";

        // GUID док-панели (стабильный, для сохранённых раскладок Revit)
        public static readonly Guid ChatPaneGuid =
            new Guid("D3BD10C9-1E0E-4C9E-9B77-6D81C5A1E7A1");

        public static readonly DockablePaneId ChatPaneId = new DockablePaneId(ChatPaneGuid);

        // --- Состояние приложения ---

        private static UI.ToolsPanel? _chatPanel;
        private static UIApplication? _uiApp;
        private static bool _themeSubscribed;

        // --- IExternalApplication ---

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                CreateRibbon(application);
                RegisterDockablePane(application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ReActionAI", $"Ошибка инициализации ReActionAI: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            _chatPanel = null;
            _uiApp = null;
            _themeSubscribed = false;
            return Result.Succeeded;
        }

        // --- Создание вкладки и панели ленты ---

        private void CreateRibbon(UIControlledApplication app)
        {
            // Вкладка
            try
            {
                app.CreateRibbonTab(TabName);
            }
            catch
            {
                // Вкладка уже существует — игнорируем.
            }

            // Панель
            RibbonPanel panel = app.GetRibbonPanels(TabName)
                                   .FirstOrDefault(p => p.Name == PanelName)
                               ?? app.CreateRibbonPanel(TabName, PanelName);

            // Кнопка открытия панели ассистента
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string className = "ReActionAI.Integration.Revit.Commands.Command";

            PushButtonData btnData = new PushButtonData(
                ButtonName,
                ButtonText,
                assemblyPath,
                className)
            {
                ToolTip = ButtonTooltip,
                LargeImage = LoadButtonImage()
            };

            panel.AddItem(btnData);
        }

        private BitmapImage? LoadButtonImage()
        {
            try
            {
                string uri = "pack://application:,,,/ReActionAI.Integration.Revit;component/Resources/icon32.png";
                return new BitmapImage(new Uri(uri, UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }

        // --- Регистрация док-панели ---

        private void RegisterDockablePane(UIControlledApplication app)
        {
            _chatPanel = new UI.ToolsPanel();
            var provider = new UI.ChatPanelProvider(_chatPanel);

            try
            {
                app.RegisterDockablePane(
                    ChatPaneId,
                    "Ассистент",
                    provider);
            }
            catch
            {
                // Если уже зарегистрирована — игнорируем
            }
        }

        // --- Публичные помощники для команд ---

        /// <summary>
        /// Показывает док-панель ассистента.
        /// Вызывается из внешней команды.
        /// </summary>
        public static void ShowChatPane(UIApplication uiApp)
        {
            _uiApp = uiApp;

            DockablePane pane = uiApp.GetDockablePane(ChatPaneId);
            pane.Show();

            bool isDark = IsRevitDarkTheme(uiApp);
            RefreshChatTheme(isDark);
        }

        /// <summary>
        /// Вызывается из Command.cs, когда у нас есть UIApplication.
        /// Обеспечивает начальную синхронизацию темы.
        /// </summary>
        internal static void EnsureThemeSubscription(UIApplication uiApp)
        {
            _uiApp = uiApp;

            if (_themeSubscribed)
                return;

            _themeSubscribed = true;

            // На данном этапе достаточно один раз обновить тему панели.
            RefreshChatTheme();
        }

        /// <summary>
        /// Перегрузка без параметров для совместимости.
        /// </summary>
        internal static void RefreshChatTheme()
        {
            RefreshChatTheme(null);
        }

        /// <summary>
        /// Обновляет тему панели в соответствии с темой Revit.
        /// </summary>
        public static void RefreshChatTheme(bool? isDarkOverride = null)
        {
            if (_chatPanel == null)
                return;

            bool isDark = isDarkOverride ?? IsRevitDarkTheme(_uiApp);
            InvokeApplyRevitTheme(_chatPanel, isDark);
        }

        // --- Работа с темой Revit ---

        private static bool IsRevitDarkTheme(UIApplication? uiApp)
        {
            try
            {
                if (uiApp == null)
                    return false;

                var uiAppType = uiApp.GetType();
                var themeProp = uiAppType.GetProperty("Theme", BindingFlags.Public | BindingFlags.Instance);
                if (themeProp == null)
                    return false;

                var themeValue = themeProp.GetValue(uiApp);
                if (themeValue == null)
                    return false;

                string themeName = themeValue.ToString() ?? string.Empty;
                return themeName.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Вызывает приватный метод ApplyRevitTheme(bool) у ToolsPanel через рефлексию.
        /// </summary>
        private static void InvokeApplyRevitTheme(UI.ToolsPanel? panel, bool isDark)
        {
            if (panel == null)
                return;

            try
            {
                var type = panel.GetType();
                var method = type.GetMethod(
                    "ApplyRevitTheme",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    Type.DefaultBinder,
                    new[] { typeof(bool) },
                    null);

                method?.Invoke(panel, new object[] { isDark });
            }
            catch
            {
                // Тихо игнорируем — вспомогательная функция.
            }
        }
    }
}
