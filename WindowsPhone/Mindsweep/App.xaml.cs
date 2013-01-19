using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mindsweep.Model;
using Mindsweep.ViewModels;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;

namespace Mindsweep
{
    public partial class App : Application
    {
        public const string NO_INTERNET_EXCEPTION_MESSAGE = "The remote server returned an error: NotFound.";
        /// <summary>
        /// Component used to handle unhandle exceptions, to collect runtime info and to send email to developer.
        /// </summary>
		public RadDiagnostics diagnostics;
        /// <summary>
        /// Component used to raise a notification to the end users to rate the application on the marketplace.
        /// </summary>
        public RadRateApplicationReminder rateReminder;

        // The static ViewModel, to be used across the application.
        private static MainViewModel viewModel;
        public static MainViewModel ViewModel
        {
            get { return viewModel; }
        }

        private static string _GetResourceString(string key)
        {
            string val = null;

            if ((App.Current != null) && (App.Current.Resources != null) && App.Current.Resources.Contains(key))
                val = App.Current.Resources[key] as string;

            return val;
        }

        public static string AppTitle
        {
            get { return _GetResourceString("AppTitle"); }
        }
        public static string AppTitleUppercase
        {
            get { return _GetResourceString("AppTitleUppercase"); }
        }
        public static string AppTitleVersion
        {
            get { return _GetResourceString("AppTitleVersion"); }
        }
        public static string AppVersion
        {
            get { return _GetResourceString("AppVersion"); }
        }
        public static string AppVersionNumber
        {
            get { return _GetResourceString("AppVersionNumber"); }
        }
        public static string AppDescription
        {
            get { return _GetResourceString("AppDescription"); }
        }
        public static string FeedbackEmail
        {
            get { return _GetResourceString("FeedbackEmail"); }
        }
        public static string CompanyName
        {
            get { return _GetResourceString("CompanyName"); }
        }

        public static string RtmApiKey
        {
            get { return _GetResourceString("RtmApiKey"); }
        }

        public static string RtmSecret
        {
            get { return _GetResourceString("RtmSecret"); }
        }

        public static string RtmDebugToken
        {
            get { return _GetResourceString("RtmDebugToken"); }
        }

        public static string FlurryApiKey
        {
            get { return _GetResourceString("FlurryApiKey"); }
        }

		/// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            //Creates a new instance of the RadRateApplicationReminder component.
            rateReminder = new RadRateApplicationReminder();

            //Sets how often the rate reminder is displayed.
            rateReminder.RecurrencePerUsageCount = 4;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

				// Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }


            //Creates an instance of the Diagnostics component.
            diagnostics = new RadDiagnostics()
            {
                EmailTo = FeedbackEmail,
                ApplicationName = AppTitle,
                ApplicationVersion = AppVersion,
                IncludeScreenshot = true
            };

            //Initializes this instance.
            diagnostics.Init();
    

            // Specify the local database connection string.
            string DBConnectionString = "Data Source=isostore:/Mindsweep.sdf";

            // Create the database if it does not exist.
            using (MainDataContext db = new MainDataContext(DBConnectionString))
            {
                if (db.DatabaseExists() == false)
                {
                    // Create the local database.
                    db.CreateDatabase();
                }
            }
			
            // Create the ViewModel object.
            viewModel = new MainViewModel(DBConnectionString);

            // Query the local database and load observable collections.
            viewModel.LoadCollectionsFromDatabase();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //Before using any of the ApplicationBuildingBlocks, this class should be initialized with the version of the application.
            ApplicationUsageHelper.Init(AppVersionNumber);
            FlurryWP7SDK.Api.StartSession(FlurryApiKey);
		}

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
            {
                //This will ensure that the ApplicationUsageHelper is initialized again if the application has been in Tombstoned state.
                ApplicationUsageHelper.OnApplicationActivated();
            }

            FlurryWP7SDK.Api.StartSession(FlurryApiKey);
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
			// Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            FlurryWP7SDK.Api.LogError("Unhandled Exception", e.ExceptionObject);
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new RadPhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}
