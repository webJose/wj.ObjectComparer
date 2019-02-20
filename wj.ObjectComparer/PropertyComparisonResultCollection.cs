using System.Collections.ObjectModel;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyComparisonResult"/> objects product of a comparison 
    /// operation between two objects.
    /// </summary>
    public class PropertyComparisonResultCollection : KeyedCollection<string, PropertyComparisonResult>
    {
        #region Methods
        /// <inheritdoc />
        protected override string GetKeyForItem(PropertyComparisonResult item)
        {
            return item.Property1.Name;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public PropertyComparisonResultCollection()
            : base(null, 20)
        { }
        #endregion
    }
}