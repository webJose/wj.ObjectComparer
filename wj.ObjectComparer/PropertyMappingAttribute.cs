using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Attribute used to specify the property name of another class to which this property needs 
    /// to be compared against.  Use one attribute per data type (class).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class PropertyMappingAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Gets the associated <see cref="PropertyMapping"/> object.
        /// </summary>
        internal PropertyMapping PropertyMapping { get; }

        /// <summary>
        /// Gets the target type containing the specified target property.
        /// </summary>
        public Type TargetType => PropertyMapping.TargetType;

        /// <summary>
        /// Gets the target property name that will be used for comparison purposes.
        /// </summary>
        public string TargetProperty => PropertyMapping.TargetProperty;

        /// <summary>
        /// Gets or sets an optional format string used in a call to ToString() whenever the value 
        /// needs to be compared as a string.  This happens when property types differ or when 
        /// string comparison is forced.  It can only be used if the property type implements 
        /// <see cref="IFormattable"/>.
        /// </summary>
        public string FormatString
        {
            get => PropertyMapping.FormatString;
            set => PropertyMapping.FormatString = value;
        }

        /// <summary>
        /// Gets or sets an optional format string used in a call to ToString() whenever the 
        /// target value needs to be compared as a string.  This happens when property types 
        /// differ or when string comparison is forced.  It can only be used if the target 
        /// property type implements <see cref="IFormattable"/>.
        /// </summary>
        public string TargetFormatString
        {
            get => PropertyMapping.TargetFormatString;
            set => PropertyMapping.TargetFormatString = value;
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the comparison operation needs to 
        /// forcefully convert the values to a string.
        /// </summary>
        public bool ForceStringValue
        {
            get => PropertyMapping.ForceStringValue;
            set => PropertyMapping.ForceStringValue = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="targetType">The target type of the specified target property.</param>
        /// <param name="targetProperty">The target property name that will be used for comparison 
        /// purposes.</param>
        public PropertyMappingAttribute(Type targetType, string targetProperty)
        {
            PropertyMapping = new PropertyMapping(targetType, targetProperty);
        }
        #endregion
    }
}