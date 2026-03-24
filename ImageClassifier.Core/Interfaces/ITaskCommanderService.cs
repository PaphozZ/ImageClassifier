namespace ImageClassifier.Core.Interfaces;

public interface ITaskCommanderService
{
    void AddTask(Func<Task> taskFactory, bool highPriority = false);
    void SetMaxConcurrency(int maxConcurrency);
    Task WaitForAllAsync(CancellationToken cancellationToken = default);
    bool IsProcessing { get; }
}