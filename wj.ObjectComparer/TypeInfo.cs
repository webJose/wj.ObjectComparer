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
        public Type TargetType { get; }

        /// <summary>
        /// Gets a Boolean value indicating if this object was constructed by ignoring any 
        /// existing property mapping attributes defined in the type.
        /// </summary>
        public bool PropertyMappingsIgnored { get; private set; }

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
        /// <param name="targetType">The type being targetted by this object.</param>
        public TypeInfo(Type targetType, bool propertyMappingsIgnored)
        {
            TargetType = targetType;
            PropertyMappingsIgnored = propertyMappingsIgnored;
        }
        #endregion

        #region Methods
        internal TypeInfo Clone()
        {
            TypeInfo ti = new TypeInfo(TargetType, PropertyMappingsIgnored);
            foreach (PropertyComparisonInfo pci in Properties)
            {
                PropertyComparisonInfo newPci = new PropertyComparisonInfo(pci.PropertyDescriptor);
                if (!PropertyMappingsIgnored && pci.Mappings.Count > 0)
                {
                    foreach(PropertyMapping pm in pci.Mappings)
                    {
                        newPci.Mappings.Add(pm);
                    }
                }
                ti.Properties.Add(newPci);
            }
            return ti;
        }
        #endregion
    }
}