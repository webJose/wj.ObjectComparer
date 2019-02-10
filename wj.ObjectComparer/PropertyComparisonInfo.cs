using System;
using System.ComponentModel;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Defines all possible ignore property options.
    /// </summary>
    [Flags]
    public enum IgnorePropertyOptions
    {
        /// <summary>
        /// Property is not being ignored.
        /// </summary>
        DoNotIgnore = 0x0,
        /// <summary>
        /// Property is ignored when comparing two objects of the same type.
        /// </summary>
        IgnoreForSelf = 0x1,
        /// <summary>
        /// Property is ignored when being compared against an object of a different type.
        /// </summary>
        IgnoreForOthers = 0x2,
        /// <summary>
        /// Property is ignored in all cases.
        /// </summary>
        IgnoreForAll = IgnoreForSelf | IgnoreForOthers
    }

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
        /// Gets a value that determines if and how the property is ignored for comparison against 
        /// target data types without a property map in the <see cref="Maps"/> collection.
        /// </summary>
        public IgnorePropertyOptions IgnoreProperty { get; internal set; }

        /// <summary>
        /// Gets the collection of property mappings.
        /// </summary>
        public PropertyMapCollection Maps { get; } = new PropertyMapCollection();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="propertyDescriptor">Original roperty descriptor object.</param>
        /// <param name="ignoreProperty">A Boolean value that determines if the property is 
        /// ignored for comparison against any target data type without a property map.</param>
        public PropertyComparisonInfo(PropertyDescriptor propertyDescriptor, IgnorePropertyOptions ignoreProperty)
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