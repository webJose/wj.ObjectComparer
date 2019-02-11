using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Groups the necessary information to perform a property-by-property comparison of objects 
    /// of the specified type and objects of other types.
    /// </summary>
    public class TypeInfo
    {
        #region Properties
        /// <summary>
        /// Gets the type this object collects information for.
        /// </summary>
        public Type DataType { get; }

        /// <summary>
        /// Gets a Boolean value indicating if this object was constructed by ignoring any 
        /// existing property mapping attributes defined in the type.
        /// </summary>
        public bool PropertyMapsIgnored { get; }

        /// <summary>
        /// Gets a collection of objects that describe how the property values of objects of the 
        /// target type should be compared to the property values of other objects, either of the 
        /// same type or different type.
        /// </summary>
        public PropertyComparisonInfoCollection Properties { get; } = new PropertyComparisonInfoCollection();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an instance of this class to gather data of the specified type.
        /// </summary>
        /// <param name="dataType">The type being targetted by this object.</param>
        public TypeInfo(Type dataType, bool propertyMappingsIgnored)
        {
            DataType = dataType;
            PropertyMapsIgnored = propertyMappingsIgnored;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        /// <param name="clonePropertyMaps">A Boolean value that indicates if property maps should 
        /// be cloned as well.</param>
        /// <returns>A type information object that contains the same information as this object.</returns>
        internal TypeInfo Clone(bool clonePropertyMaps)
        {
            TypeInfo ti = new TypeInfo(DataType, PropertyMapsIgnored);
            foreach (PropertyComparisonInfo pci in Properties)
            {
                PropertyComparisonInfo newPci = new PropertyComparisonInfo(pci.PropertyDescriptor, pci.IgnoreProperty);
                if (!PropertyMapsIgnored && pci.Maps.Count > 0 && clonePropertyMaps)
                {
                    foreach(PropertyMap pm in pci.Maps)
                    {
                        newPci.Maps.Add(pm);
                    }
                }
                ti.Properties.Add(newPci);
            }
            return ti;
        }
        #endregion
    }
}