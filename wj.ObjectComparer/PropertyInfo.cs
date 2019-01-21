using System;
using System.ComponentModel;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Defines the necessary data to compare one property value to another property value in a 
    /// different object.
    /// </summary>
    public class PropertyInfo
    {
        #region Private Data
        /// <summary>
        /// The original property descriptor.
        /// </summary>
        private readonly PropertyDescriptor _propertyDescriptor;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the property's name.
        /// </summary>
        public string Name => _propertyDescriptor.Name;

        /// <summary>
        /// Gets the property's display name.
        /// </summary>
        public string DisplayName => _propertyDescriptor.DisplayName;

        /// <summary>
        /// Gets the property's data type.
        /// </summary>
        public Type PropertyType => _propertyDescriptor.PropertyType;

        /// <summary>
        /// Gets the collection of property mappings.
        /// </summary>
        public PropertyMappingCollection Mappings { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="propertyDescriptor">Original roperty descriptor object.</param>
        public PropertyInfo(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor == null) throw new ArgumentNullException("propertyDescriptor");
            _propertyDescriptor = propertyDescriptor;
            Mappings = new PropertyMappingCollection();
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
            return _propertyDescriptor.GetValue(obj);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}, Type:  {PropertyType}";
        }
        #endregion
    }
}