using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Represents a manual mapping of a property in one data type to a property in another data 
    /// type.
    /// </summary>
    public class PropertyMapping
    {
        #region Properties
        /// <summary>
        /// Gets the data type of the property in the destination data type.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the name of the property in the destination data type.
        /// </summary>
        public string TargetProperty { get; }

        /// <summary>
        /// Gets the format string to be used to convert the origin property value to string.
        /// </summary>
        public string FormatString { get; internal set; }

        /// <summary>
        /// Gets the format string to be used to convert the destination property value to string.
        /// </summary>
        public string TargetFormatString { get; internal set; }

        /// <summary>
        /// Gets a Boolean value that determines if the values of the properties will be 
        /// forcefully converted to string before they are compared.
        /// </summary>
        public bool ForceStringValue { get; internal set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="targetType">The destination property's data type.</param>
        /// <param name="targetProperty">The destination property's name.</param>
        /// <param name="forceStringValue">A Boolean value indicating if forceful string 
        /// conversion will take place before comparison.</param>
        /// <param name="formatString">The format string to use to convert the origin property 
        /// value to string.</param>
        /// <param name="targetFormatString">The format string to use to convert the destination 
        /// property value to string.</param>
        public PropertyMapping(Type targetType, string targetProperty, bool forceStringValue = false, string formatString = null, string targetFormatString = null)
        {
            TargetType = targetType;
            TargetProperty = targetProperty;
            FormatString = formatString;
            TargetFormatString = targetFormatString;
            ForceStringValue = forceStringValue;
        }
        #endregion
    }
}