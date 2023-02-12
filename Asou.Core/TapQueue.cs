namespace Asou.Core;

/// <summary>
///     Queue with TAP await support
/// </summary>
/// <typeparam name="T"></typeparam>
public class TapQueue<T>
{
    private readonly Queue<T> _queue = new();
    private TaskCompletionSource? _taskCompletionSource;

    public void Enqueue(T obj)
    {
        _queue.Enqueue(obj);
        _taskCompletionSource?.SetResult();
    }

    public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (_queue.Count == 0)
        {
            // If nothing dequeue, await new queue elements with TaskCompletionSource
            _taskCompletionSource = new TaskCompletionSource();
            cancellationToken.Register(() => { _taskCompletionSource?.TrySetCanceled(); });
            await _taskCompletionSource.Task;
        }

        return _queue.Dequeue();
    }
}
