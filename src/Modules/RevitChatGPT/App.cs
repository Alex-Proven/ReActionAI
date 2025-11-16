using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace ReActionAI.Modules.RevitChatGPT
{
    /// <summary>
    /// Revit Application Add-in:
    /// - создаёт вкладку ReActionAI и панель AI Tools;
    /// - регистрирует док-панель ChatGPT (справа);
    /// - добавляет кнопку ChatGPT.
    /// </summary>
    public class App : IExternalApplication
    {
        private const string TabName = "ReActionAI";
        private const string PanelName = "AI Tools";

        // GUID док-панели (НЕ менять после установки!)
        public static readonly Guid ChatPaneGuid = new Guid("7B2B1D77-7A9A-44AE-9C2B-6F6B6FD7DB11");
        public static readonly DockablePaneId ChatPaneId = new DockablePaneId(ChatPaneGuid);

        private static UI.ChatPanel? _chatPanel;
        private static UIApplication? _uiApp;
        private static bool _themeSubscribed;

        public Result OnStartup(UIControlledApplication application)
        {
            // 1) Вкладка
            try { application.CreateRibbonTab(TabName); }
            catch { /* уже есть */ }

            // 2) Панель
            RibbonPanel panel = application.GetRibbonPanels(TabName)
                .FirstOrDefault(p => string.Equals(p.Name, PanelName, StringComparison.OrdinalIgnoreCase))
                ?? application.CreateRibbonPanel(TabName, PanelName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // 3) Кнопка ChatGPT
            PushButtonData chatBtnData = new PushButtonData(
                "ReActionAI_ChatGPT_Button",
                "ChatGPT",
                assemblyPath,
                "ReActionAI.Modules.RevitChatGPT.Commands.Command");

            chatBtnData.LargeImage = new BitmapImage(
                new Uri("pack://application:,,,/ReActionAI.Modules.RevitChatGPT;component/Resources/icon32.png"));

            panel.AddItem(chatBtnData);

            // 4) Регистрация док-панели
            _chatPanel = new UI.ChatPanel();
            var provider = new UI.ChatPanelProvider(_chatPanel);
            application.RegisterDockablePane(ChatPaneId, "ChatGPT", provider);

            // Пока нет UIApplication — считаем светлую тему
            RefreshChatTheme();

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            _chatPanel = null;
            _uiApp = null;
            _themeSubscribed = false;
            return Result.Succeeded;
        }

        /// <summary>
        /// Вызывается из команды (Command.cs), когда у нас есть UIApplication.
        /// </summary>
        internal static void EnsureThemeSubscription(UIApplication uiApp)
        {
            _uiApp = uiApp;

            if (_themeSubscribed)
                return;

            _themeSubscribed = true;

            uiApp.ThemeChanged += (sender, args) =>
            {
                RefreshChatTheme();
            };

            RefreshChatTheme();
        }

        /// <summary>
        /// Определяет, тёмная/светлая тема в Revit, и обновляет панель.
        /// </summary>
        internal static void RefreshChatTheme()
        {
            bool isDark = false;

            try
            {
                if (_uiApp != null)
                {
                    var app = _uiApp.Application;
                    var method = app.GetType().GetMethod(
                        "GetRenderingStyle",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (method != null)
                    {
                        var styleObj = method.Invoke(app, null);
                        var styleName = styleObj?.ToString() ?? string.Empty;

                        // В Revit 2024 стиль называется что-то вроде "Dark" / "Light"
                        if (styleName.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0)
                            isDark = true;
                    }
                }
            }
            catch
            {
                isDark = false;   // на всякий случай, не ломаем аддин
            }

            InvokeApplyRevitTheme(_chatPanel, isDark);
        }

        /// <summary>
        /// Вызывает приватный метод ApplyRevitTheme(bool) у ChatPanel через рефлексию.
        /// </summary>
        private static void InvokeApplyRevitTheme(UI.ChatPanel? panel, bool isDark)
        {
            if (panel == null)
                return;

            try
            {
                MethodInfo? mi = panel.GetType().GetMethod(
                    "ApplyRevitTheme",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                mi?.Invoke(panel, new object[] { isDark });
            }
            catch
            {
                // Ошибки темы игнорируем
            }
        }
    }
}
