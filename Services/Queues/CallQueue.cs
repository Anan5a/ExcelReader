using Models;
using Services;
using System.Collections.Concurrent;
using System.Linq;

namespace ExcelReader.Services.Queues
{
    //this is a application wide queue for call management

    public interface ICallQueue<T> where T : class
    {
        bool Enqueue(T item);
        Task<T?> DequeueAsync();
        Task<T?> PeekAsync();
        int Count { get; }
        bool TryRemoveByUserId(string item);
        T? DequeueByIdAsync(int userId);

        IEnumerable<long> GetQueueUserIds();
    }
    public class CallQueue<T> : ICallQueue<T> where T : class
    {
        private readonly ConcurrentDictionary<int, T> _queue = new ConcurrentDictionary<int, T>();
        //using this hacked system because there is no thread-safe list ootb, value is always null
        private readonly ConcurrentDictionary<string, string?> inQueueUsers = new();
        private readonly ConcurrentDictionary<string, string?> processedUsers = new();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private int _counter = 0;

        public bool Enqueue(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item is QueueModel model && inQueueUsers.ContainsKey(model.UserId))
            {
                //ErrorConsole.Log("Already in queue... ::" + item.ToString());

                //already in queue, ignore
                return false;
            }
            else
            {
                //ErrorConsole.Log("Id did not match... ::" + item.ToString());
            }
            int key = Interlocked.Increment(ref _counter);
            _queue[key] = item;
            inQueueUsers.TryAdd((item as QueueModel).UserId, null);
            _signal.Release();
            return true;
        }

        public async Task<T?> DequeueAsync()
        {
            await _signal.WaitAsync();

            if (_queue.IsEmpty) return null;

            var firstItem = _queue.OrderBy(kvp => kvp.Key).FirstOrDefault();
            _queue.TryRemove(firstItem.Key, out var item);
            inQueueUsers.Remove((item as QueueModel).UserId, out var _);
            processedUsers.TryAdd((item as QueueModel).UserId, null);
            return item;
        }

        public T? DequeueByIdAsync(int userId)
        {
            if (_queue.IsEmpty) return null;

            var firstItem = _queue.FirstOrDefault(kvp => (kvp.Value as QueueModel).UserId.Equals(userId.ToString()));
            _queue.TryRemove(firstItem.Key, out var item);
            if (item != null)
            {
                inQueueUsers.Remove((item as QueueModel).UserId, out var _);
                processedUsers.TryAdd((item as QueueModel).UserId, null);
            }
            return item;
        }


        public bool TryRemoveByUserId(string item)
        {
            //ErrorConsole.Log($"I: Remove user={item} from queue");
            var kvp = _queue.FirstOrDefault(pair => (pair.Value as QueueModel).UserId.Equals(item));
            //remove from trackers
            var rmState = _queue.TryRemove(kvp.Key, out var _o);
            if (rmState && _o != null)
            {
                inQueueUsers.TryRemove(inQueueUsers.FirstOrDefault(it => it.Key == (_o as QueueModel)?.UserId));
                processedUsers.Remove((_o as QueueModel)?.UserId!, out var _);

            }
            return kvp.Key != 0 && rmState;
        }

        public async Task<T?> PeekAsync()
        {
            await _signal.WaitAsync();

            if (_queue.IsEmpty) return null;

            var firstItem = _queue.OrderBy(kvp => kvp.Key).FirstOrDefault();
            return firstItem.Value;
        }

        public IEnumerable<long> GetQueueUserIds()
        {
            return inQueueUsers.Select(q => long.Parse(q.Key)).Concat(processedUsers.Select(q => long.Parse(q.Key)));
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