using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        /// Gets a collection of objects that describe how the property values of objects of the 
        /// target type should be compared to the property values of other objects, either of the 
        /// same type or different type.
        /// </summary>
        public PropertyInfoCollection Properties { get; } = new PropertyInfoCollection();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an instance of this class to gather data of the specified type.
        /// </summary>
        /// <param name="targetType">The type being targetted by this object.</param>
        public TypeInfo(Type targetType)
        {
            TargetType = targetType;
        }
        #endregion
    }
}