using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Attribute used to specify the property name of another class to which this property needs 
    /// to be compared against.  Use one attribute per data type (class).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class PropertyMapAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Gets the associated <see cref="PropertyMap"/> object.
        /// </summary>
        internal PropertyMap PropertyMap { get; }

        /// <summary>
        /// Gets the target type containing the specified target property.
        /// </summary>
        public Type TargetType => PropertyMap.TargetType;

        /// <summary>
        /// Gets the target property name that will be used for comparison purposes.
        /// </summary>
        public string TargetProperty => PropertyMap.TargetProperty;

        /// <summary>
        /// Gets or sets an optional format string used in a call to ToString() whenever the value 
        /// needs to be compared as a string.  This happens when property types differ or when 
        /// string comparison is forced.  It can only be used if the property type implements 
        /// <see cref="IFormattable"/>.
        /// </summary>
        public string FormatString
        {
            get => PropertyMap.FormatString;
            set => PropertyMap.FormatString = value;
        }

        /// <summary>
        /// Gets or sets an optional format string used in a call to ToString() whenever the 
        /// target value needs to be compared as a string.  This happens when property types 
        /// differ or when string comparison is forced.  It can only be used if the target 
        /// property type implements <see cref="IFormattable"/>.
        /// </summary>
        public string TargetFormatString
        {
            get => PropertyMap.TargetFormatString;
            set => PropertyMap.TargetFormatString = value;
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the comparison operation needs to 
        /// forcefully convert the values to a string.
        /// </summary>
        public bool ForceStringValue
        {
            get => PropertyMap.ForceStringValue;
            set => PropertyMap.ForceStringValue = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="targetType">The target type of the specified target property, or the type 
        /// for which the property will be ignored.</param>
        /// <param name="operation">The operation being defined.  If the operation is 
        /// <see cref="PropertyMapOperation.MapToProperty"/>, then a target property name must be 
        /// provided.</param>
        /// <param name="targetProperty">The target property name that will be used for comparison 
        /// purposes, as long as the property map does not mark the property as ignored.</param>
        public PropertyMapAttribute(Type targetType, PropertyMapOperation operation, string targetProperty = null)
        {
            PropertyMap = new PropertyMap(targetType, operation, targetProperty);
        }
        #endregion
    }
}