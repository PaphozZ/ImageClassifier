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

    public bool IsProcessing { get => _isProcessing == 1; }

    public TaskCommanderService(int maxConcurrency = 1)
    {
        _maxConcurrency = maxConcurrency;
        _semaphore = new SemaphoreSlim(maxConcurrency);
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
        }
    }
}