using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="TypeInfo"/> objects.
    /// </summary>
    public class TypeInfoCollection : DictionaryCollection<Type, TypeInfo>
    {
        #region Methods
        /// <inheritdoc />
        protected override Type GetKeyForItem(TypeInfo item)
        {
            return item.DataType;
        }
        #endregion
    }
}