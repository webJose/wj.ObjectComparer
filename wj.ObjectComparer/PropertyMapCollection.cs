using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyMap"/> objects.
    /// </summary>
    public class PropertyMapCollection : DictionaryCollection<Type, PropertyMap>
    {
        #region Methods
        /// <inheritdoc />
        protected override Type GetKeyForItem(PropertyMap item)
        {
            return item.TargetType;
        }

        /// <summary>
        /// Replaces a property mapping with another.  The item replaced is the one that returns 
        /// the same target type as the item being provided.  If the key is not present in this 
        /// collection, then a regular addition is performed.
        /// </summary>
        /// <param name="item">The new item to add to the collection.</param>
        internal void Replace(PropertyMap item)
        {
            Type key = GetKeyForItem(item);
            if (Contains(key))
            {
                Remove(key);
            }
            Add(item);
        }
        #endregion
    }
}