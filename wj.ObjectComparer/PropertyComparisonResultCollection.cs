namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyComparisonResult"/> objects product of a comparison 
    /// operation between two objects.
    /// </summary>
    public class PropertyComparisonResultCollection : DictionaryCollection<string, PropertyComparisonResult>
    {
        #region Methods
        /// <inheritdoc />
        protected override string GetKeyForItem(PropertyComparisonResult item)
        {
            return item.Property1.Name;
        }
        #endregion
    }
}