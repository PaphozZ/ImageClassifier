using CommunityToolkit.Maui.Storage;
using ImageClassifier.Core.Interfaces;
using ImageClassifier.Core.Models;
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
            services.AddSingleton<FileCollectionViewModel>();
            services.AddTransient<FullscreenViewModel>();
            services.AddSingleton<WorkflowViewModel>();
            services.AddTransient<DragDropManagerViewModel>();
            services.AddTransient<TrainMenuViewModel>();
            services.AddTransient<PredictMenuViewModel>();
            services.AddSingleton<ModeManagerViewModel>();
            return services;
        }

        [SupportedOSPlatform("windows")]
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IFolderPicker, DeferredFolderPicker>();
            services.AddSingleton<IJsonStorageService<ImageItemModel>>(sp => new JsonStorageService<ImageItemModel>("loaded_files.json"));
            services.AddSingleton<IJsonStorageService<ModelItemModel>>(sp => new JsonStorageService<ModelItemModel>("models.json"));
            services.AddSingleton<IDialogService, MauiDialogService>();
            services.AddSingleton<IMediaPickerService, MediaPickerService>();
            services.AddSingleton<IFileScanner, FileScanner>();
            services.AddSingleton<IImageResizeService, ImageResizeService>();
            services.AddSingleton<ITaskCommanderService>(sp => new TaskCommanderService(Environment.ProcessorCount - 1));
            services.AddSingleton<IModelTrainingService, ModelTrainingService>();
            services.AddSingleton<IPredictionService, PredictionService>();
            services.AddSingleton<IModelManagerService, ModelManagerService>();
            return services;
        }
    }
}
