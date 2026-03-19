using ImageClassifier.ViewModel.ViewModels;

namespace ImageClassifier.ViewModel.Extensions
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task, Action<Exception>? onError = null)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                    onError?.Invoke(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
