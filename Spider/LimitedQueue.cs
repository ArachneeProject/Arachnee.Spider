using System;
using System.Collections;
using System.Collections.Generic;
using Spider;

namespace TmdbCrwaler
{
    public class LimitedQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();

        public int Limit { get; }

        public int Count => _queue.Count;

        public LimitedQueue(int limit)
        {
            if (limit < 0)
            {
                throw new ArgumentException($"Limit was {limit}, but it has to be non-negative.");
            }

            Limit = limit;
        }

        public void Enqueue(T item)
        {
            while (Count >= Limit)
            {
                Dequeue();
            }

            Logger.Instance.LogMessage(item + " was added to cache.");
            _queue.Enqueue(item);
        }

        public T Dequeue()
        {
            var discarded = _queue.Dequeue();
            Logger.Instance.LogMessage(discarded + " was discarded from cache.");

            return discarded;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }
    }
}