using Models;
using Services;
using System.Collections.Concurrent;

namespace ExcelReader.Queues
{
    //this is a application wide queue for call management

    public interface ICallQueue<T> where T : class
    {
        bool Enqueue(T item);
        Task<T?> DequeueAsync();
        Task<T?> PeekAsync();
        int Count { get; }
    }
    public class CallQueue<T> : ICallQueue<T> where T : class
    {
        private readonly ConcurrentDictionary<int, T> _queue = new ConcurrentDictionary<int, T>();
        private readonly ConcurrentBag<string> inQueueUsers = new();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private int _counter = 0;

        public bool Enqueue(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item is CallQueueModel model && inQueueUsers.Contains(model.UserId))
            {
                //already in queue, ignore
                return false;
            }
            else
            {
                ErrorConsole.Log("Id did not match... ::" + item.ToString());
            }
            int key = Interlocked.Increment(ref _counter);
            _queue[key] = item;
            inQueueUsers.Add((item as CallQueueModel).UserId);
            _signal.Release();
            return true;
        }

        public async Task<T?> DequeueAsync()
        {
            await _signal.WaitAsync();

            if (_queue.IsEmpty) return null;

            var firstItem = _queue.OrderBy(kvp => kvp.Key).FirstOrDefault();
            _queue.TryRemove(firstItem.Key, out var item);
            return item;
        }

        public bool TryRemoveByUserId(string item)
        {
            var kvp = _queue.FirstOrDefault(pair => pair.Key.Equals(int.Parse(item)));
            return kvp.Key != 0 && _queue.TryRemove(kvp.Key, out _);
        }

        public async Task<T?> PeekAsync()
        {
            await _signal.WaitAsync();

            if (_queue.IsEmpty) return null;

            var firstItem = _queue.OrderBy(kvp => kvp.Key).FirstOrDefault();
            return firstItem.Value;
        }


        public int Count => _queue.Count;
    }

    //public class CallQueue<T> : ICallQueue<T> where T : class
    //{
    //    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    //    private readonly HashSet<int> _queueIndex = new HashSet<int>();
    //    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

    //    public void Enqueue(T item)
    //    {
    //        if (item == null) throw new ArgumentNullException(nameof(item));

    //        _queue.Enqueue(item);
    //        _signal.Release();
    //    }

    //    public async Task<T?> DequeueAsync()
    //    {
    //        await _signal.WaitAsync();
    //        _queue.TryDequeue(out var item);
    //        return item;
    //    }
    //    public async Task<T?> PeekAsync()
    //    {
    //        await _signal.WaitAsync();
    //        _queue.TryPeek(out var item);
    //        return item;
    //    }
    //    public int Count => _queue.Count;


    //}

}
