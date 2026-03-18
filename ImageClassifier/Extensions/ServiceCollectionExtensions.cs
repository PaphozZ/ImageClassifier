using ImageClassifier.ViewModel.ViewModels;

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
    }
}
