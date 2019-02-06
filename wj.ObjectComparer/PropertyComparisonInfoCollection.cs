namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyComparisonInfo"/> objects from a specific data type.
    /// </summary>
    public class PropertyComparisonInfoCollection : DictionaryCollection<string, PropertyComparisonInfo>
    {
        #region Methods
        /// <inheritdoc />
        protected override string GetKeyForItem(PropertyComparisonInfo item)
        {
            return item.Name;
        }
        #endregion
    }
}