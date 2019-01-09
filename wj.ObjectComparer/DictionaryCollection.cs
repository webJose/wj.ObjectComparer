using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection based on the <see cref="KeyedCollection{TKey, TItem}"/> class that also 
    /// implements the <see cref="IDictionary{TKey, TValue}"/> interface.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TValue">The type of element contained in the collection.</typeparam>
    public abstract class DictionaryCollection<TKey, TValue> : KeyedCollection<TKey, TValue>, IDictionary<TKey, TValue>
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public DictionaryCollection()
            : base(null, 20)
        { }
        #endregion

        #region IDictionary
        /// <inheritdoc />
        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set
            {
                if (Contains(key))
                {
                    SetItem(IndexOf(this[key]), value);
                }
                else
                {
                    Add(value);
                }
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> allKeys = new List<TKey>();
                foreach (TValue item in this)
                {
                    allKeys.Add(GetKeyForItem(item));
                }
                return allKeys;
            }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> allValues = new List<TValue>();
                allValues.AddRange(this);
                return allValues;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            Add(value);
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Value);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item) => Contains(item.Key);

        /// <inheritdoc />
        public bool ContainsKey(TKey key) => Contains(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; ++i)
            {
                TValue item = this[i];
                array[i + arrayIndex] = new KeyValuePair<TKey, TValue>(GetKeyForItem(item), item);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            if (Contains(key))
            {
                value = this[key];
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (TValue item in this)
            {
                yield return new KeyValuePair<TKey, TValue>(GetKeyForItem(item), item);
            }
        }
        #endregion
    }
}
