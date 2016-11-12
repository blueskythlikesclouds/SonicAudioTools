using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SonicAudioLib.Collections
{
    /// <summary>
    /// Represents a key/value pair for an <see cref="OrderedDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Represents a key/value pair collection that is accessable by its key or index.
    /// </summary>
    public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private List<KeyValuePair<TKey, TValue>> items = new List<KeyValuePair<TKey, TValue>>();

        /// <summary>
        /// Gets the count of key/value pairs.
        /// </summary>
        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        public TValue this[int index]
        {
            get
            {
                return items[index].Value;
            }

            set
            {
                items[index].Value = value;
            }
        }

        /// <summary>
        /// Gets the value by the specified key.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                return items.Single(k => (k.Key).Equals(key)).Value;
            }

            set
            {
                items.Single(k => (k.Key).Equals(key)).Value = value;
            }
        }

        /// <summary>
        /// Determines whether the collection contains the specified key.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return items.Any(k => (k.Key).Equals(key));
        }

        /// <summary>
        /// Adds a key/value pair to end of the collection.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            items.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes the key/value pair by its key.
        /// </summary>
        public bool Remove(TKey key)
        {
            return items.Remove(items.Single(k => (k.Key).Equals(key)));
        }

        /// <summary>
        /// Clears all the key/value pairs.
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Gets the index of the specified key.
        /// </summary>
        public int IndexOf(TKey key)
        {
            return items.IndexOf(items.Single(k => (k.Key).Equals(key)));
        }

        /// <summary>
        /// Inserts a key/value pair to the specified index.
        /// </summary>
        public void Insert(int index, TKey key, TValue value)
        {
            items.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes key/value pair at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        
        /// <summary>
        /// Gets key at the specified index.
        /// </summary>
        public TKey GetKeyByIndex(int index)
        {
            return items[index].Key;
        }

        /// <summary>
        /// Gets value at the specified index.
        /// </summary>
        public TValue GetValueByIndex(int index)
        {
            return items[index].Value;
        }

        /// <summary>
        /// Sets key at the specified index.
        /// </summary>
        public void SetKeyByIndex(int index, TKey key)
        {
            items[index].Key = key;
        }

        /// <summary>
        /// Sets value at the specified index.
        /// </summary>
        public void SetValueByIndex(int index, TValue value)
        {
            items[index].Value = value;
        }

        /// <summary>
        /// Returns an enumerator of key/value pairs.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
