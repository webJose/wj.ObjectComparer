namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyInfo"/> objects from a specific data type.
    /// </summary>
    public class PropertyInfoCollection : DictionaryCollection<string, PropertyInfo>
    {
        #region Methods
        /// <inheritdoc />
        protected override string GetKeyForItem(PropertyInfo item)
        {
            return item.Name;
        }
        #endregion
    }
}