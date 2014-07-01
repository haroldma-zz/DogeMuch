#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DogeMuch.Rest;
using DogeMuch.Utility;

#if WINDOWS_PHONE_APP
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.Media.Capture;
using Windows.UI.ViewManagement;

#endif

#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif

#endregion

namespace DogeMuch
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
        public static string QrScanAddress;
#endif

        public static string ApiKey
        {
            get { return (string) ApplicationSettingsHelper.ReadSettingsValue("apiKey"); }
            set { ApplicationSettingsHelper.SaveSettingsValue("apiKey", value); }
        }

        private static DogeApi _api;

        public static DogeApi Api
        {
            get { return _api ?? (_api = new DogeApi(ApiKey)); }
        }

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used when the application is launched to open a specific file, to display
        ///     search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                //DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
#if WINDOWS_APP
                SettingsPane.GetForCurrentView().CommandsRequested += (_, ee) =>
                    ee.Request.ApplicationCommands.Add(new SettingsCommand("P", "Privacy Policy", async __ =>
                        await Launcher.LaunchUriAsync(new Uri("https://www.dogeapi.com/privacy_policy"))));
#endif

                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
    // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                var tpage = typeof (MainPage);

                if (ApplicationSettingsHelper.ReadSettingsValue("tosAgree") == null)
                    tpage = typeof (TosPage);
                else if (ApiKey == null)
                    tpage = typeof (LoginPage);

                if (!rootFrame.Navigate(tpage, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            else
            {
                var main = rootFrame.Content as MainPage;
                if (main != null)
                {
                    try
                    {
                        main.Vm.LoadDataAsync();
                    }
                    catch
                    {
                    }
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        public MediaCapture MediaCapture { get; set; }
        public CaptureElement PreviewElement { get; set; }

        /// <summary>
        ///     Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = transitions ?? new TransitionCollection {new NavigationThemeTransition()};
            rootFrame.Navigated -= RootFrame_FirstNavigated;

            StatusBar.GetForCurrentView().HideAsync();
        }
#endif

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
#if WINDOWS_PHONE_APP
            if (MediaCapture != null)
            {
                await MediaCapture.StartPreviewAsync();
                MediaCapture.Dispose();
                MediaCapture = null;
                PreviewElement.Source = null;
                PreviewElement = null;
            }
#endif
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}