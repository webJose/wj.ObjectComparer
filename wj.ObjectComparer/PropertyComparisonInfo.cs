using System;
using System.ComponentModel;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Defines the necessary data to compare one property value to another property value in a 
    /// different object.
    /// </summary>
    public class PropertyComparisonInfo
    {
        #region Data
        /// <summary>
        /// The original property descriptor.
        /// </summary>
        internal readonly PropertyDescriptor PropertyDescriptor;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the property's name.
        /// </summary>
        public string Name => PropertyDescriptor.Name;

        /// <summary>
        /// Gets the property's display name.
        /// </summary>
        public string DisplayName => PropertyDescriptor.DisplayName;

        /// <summary>
        /// Gets the property's data type.
        /// </summary>
        public Type PropertyType => PropertyDescriptor.PropertyType;

        /// <summary>
        /// Gets a Boolean value that determines if the property is ignored for comparison against 
        /// any target data type without a property map in the <see cref="Mappings"/> collection.
        /// </summary>
        public bool IgnoreProperty { get; internal set; }

        /// <summary>
        /// Gets the collection of property mappings.
        /// </summary>
        public PropertyMapCollection Mappings { get; } = new PropertyMapCollection();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="propertyDescriptor">Original roperty descriptor object.</param>
        /// <param name="ignoreProperty">A Boolean value that determines if the property is 
        /// ignored for comparison against any target data type without a property map.</param>
        public PropertyComparisonInfo(PropertyDescriptor propertyDescriptor, bool ignoreProperty)
        {
            PropertyDescriptor = propertyDescriptor ?? throw new ArgumentNullException(nameof(propertyDescriptor));
            IgnoreProperty = ignoreProperty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the property's value for the specified object.
        /// </summary>
        /// <param name="obj">The object being the subject of the query.</param>
        /// <returns>The property's value.</returns>
        public object GetValue(object obj)
        {
            return PropertyDescriptor.GetValue(obj);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}, Type:  {PropertyType}";
        }
        #endregion
    }
}