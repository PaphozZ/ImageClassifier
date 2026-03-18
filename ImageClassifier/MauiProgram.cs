using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using ImageClassifier.Extensions;
using Microsoft.Extensions.Logging;
using ImageClassifier.Infrastructure;

namespace ImageClassifier
{
    public static class MauiProgram
    {
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
                    .AddSingleton<IFolderPicker, DeferredFolderPicker>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
