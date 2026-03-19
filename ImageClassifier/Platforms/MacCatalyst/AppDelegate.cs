using Foundation;
using System.Runtime.Versioning;

namespace ImageClassifier
{
    [Register("AppDelegate")]
    [SupportedOSPlatform("windows")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
