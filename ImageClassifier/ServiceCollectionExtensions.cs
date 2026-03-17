using ImageClassifier.ViewModel.ViewModels;

namespace ImageClassifier
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            return services;
        }
    }
}
