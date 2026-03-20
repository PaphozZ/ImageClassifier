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
            services.AddTransient<MainViewModel>();
            services.AddTransient<SampleViewModel>();
            services.AddTransient<PreviewViewModel>();
            services.AddTransient<FileCollectionViewModel>();
            services.AddTransient<FullscreenViewModel>();
            return services;
        }

        [SupportedOSPlatform("windows")]
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IFolderPicker, DeferredFolderPicker>();
            services.AddSingleton<IFileStorageService, JsonFileStorageService>();
            services.AddSingleton<IDialogService, MauiDialogService>();
            services.AddSingleton<IMediaPickerService, MediaPickerService>();
            services.AddSingleton<IFileScanner, FileScanner>();
            services.AddSingleton<IThumbnailService, ThumbnailService>();
            return services;
        }
    }
}
