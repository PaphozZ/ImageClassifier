using CommunityToolkit.Maui.Storage;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Infrastructure.Services;
using ImageClassifier.ViewModel.ViewModels;
using System.Runtime.Versioning;

namespace ImageClassifier.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SampleViewModel>();
            services.AddSingleton<PreviewViewModel>();
            return services;
        }

        [SupportedOSPlatform("windows")]
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IFolderPicker, DeferredFolderPicker>();
            services.AddSingleton<IFileStorageService, JsonFileStorageService>();
            services.AddSingleton<IDialogService, MauiDialogService>();
            return services;
        }
    }
}
