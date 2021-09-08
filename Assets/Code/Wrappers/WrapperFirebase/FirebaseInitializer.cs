using System.Threading.Tasks;
using Code.Core.Logger;

namespace Code.Wrappers.WrapperFirebase
{
    public interface IFirebaseInitializer
    {
        Task Init();
    }

    public class FirebaseInitializer : IFirebaseInitializer
    {
        private const string Tag = "FirebaseInitializer";

        private readonly IAppLogger _logger;

        public FirebaseInitializer(
            IAppLogger logger)
        {
            _logger = logger;
        }

        public async Task Init()
        {
            var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                // More information: https://firebase.google.com/docs/crashlytics/get-started?platform=unity&hl=da
                var app = Firebase.FirebaseApp.DefaultInstance;

                #if DEBUG
                
                Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;  
                
                #endif
                
                _logger.Log(Tag, $"Firebase app initialized successfully!");
            }
            else
            {
                _logger.Log(Tag, $"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }
    }
}