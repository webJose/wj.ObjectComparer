using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyMapping"/> objects.
    /// </summary>
    public class PropertyMappingCollection : DictionaryCollection<Type, PropertyMapping>
    {
        #region Methods
        /// <inheritdoc />
        protected override Type GetKeyForItem(PropertyMapping item)
        {
            return item.TargetType;
        }
        #endregion
    }
}