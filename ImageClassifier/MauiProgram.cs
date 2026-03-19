using CommunityToolkit.Maui;
using ImageClassifier.Extensions;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ImageClassifier
{
    public static class MauiProgram
    {
        [SupportedOSPlatform("windows")]
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .Services
                    .AddViewModels()
                    .AddServices();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
