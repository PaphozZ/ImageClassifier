using ImageClassifier.Core.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ImageClassifier.Infrastructure.Services;

public class TaskCommanderService : ITaskCommanderService
{
    private readonly ConcurrentQueue<Func<Task>> _highPriorityQueue = new();
    private readonly ConcurrentQueue<Func<Task>> _lowPriorityQueue = new();
    private SemaphoreSlim _semaphore;
    private int _maxConcurrency;
    private int _isProcessing = 0;
    private readonly object _semaphoreLock = new();

    public bool IsProcessing => _isProcessing == 1;

    private int _taskCount = 0;
    private TaskCompletionSource<bool> _allTasksCompletion = new();

    public TaskCommanderService(int maxConcurrency)
    {
        _maxConcurrency = Math.Max(maxConcurrency, 1);
        _semaphore = new(maxConcurrency);
    }

    public async Task WaitForAllAsync(CancellationToken cancellationToken = default)
    {
        if (_taskCount == 0)
            return;

        var tcs = _allTasksCompletion;
        using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
        {
            await tcs.Task;
        }
    }

    public void SetMaxConcurrency(int maxConcurrency)
    {
        lock (_semaphoreLock)
        {
            if (_maxConcurrency == maxConcurrency) return;
            _maxConcurrency = maxConcurrency;
            _semaphore = new SemaphoreSlim(maxConcurrency);
        }
    }

    public void AddTask(Func<Task> taskFactory, bool highPriority = false)
    {
        int newCount = Interlocked.Increment(ref _taskCount);
        if (newCount == 1 && _allTasksCompletion.Task.IsCompleted)
        {
            _allTasksCompletion = new();
        }

        if (highPriority)
            _highPriorityQueue.Enqueue(taskFactory);
        else
            _lowPriorityQueue.Enqueue(taskFactory);

        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
        {
            Task.Run(ProcessQueueAsync);
        }
    }

    private async Task ProcessQueueAsync()
    {
        while (true)
        {
            SemaphoreSlim currentSemaphore;
            lock (_semaphoreLock)
                currentSemaphore = _semaphore;

            await currentSemaphore.WaitAsync();

            if (!_highPriorityQueue.TryDequeue(out var taskFactory) &&
                !_lowPriorityQueue.TryDequeue(out taskFactory))
            {
                currentSemaphore.Release();
                break;
            }

            _ = RunTaskAsync(taskFactory, currentSemaphore);
        }

        Interlocked.Exchange(ref _isProcessing, 0);

        if ((!_highPriorityQueue.IsEmpty || !_lowPriorityQueue.IsEmpty) &&
            Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
        {
            await ProcessQueueAsync();
        }
    }

    private async Task RunTaskAsync(Func<Task> taskFactory, SemaphoreSlim semaphore)
    {
        try
        {
            await taskFactory();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в задаче: {ex}");
        }
        finally
        {
            semaphore.Release();
            if (Interlocked.Decrement(ref _taskCount) == 0)
            {
                _allTasksCompletion.TrySetResult(true);
                _allTasksCompletion = new();
            }
        }
    }
}