using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Defines the possible property map operations.
    /// </summary>
    public enum PropertyMapOperation
    {
        /// <summary>
        /// The operation is undefined.
        /// </summary>
        /// <remarks>This operation should never be seen and it is used for initialization and 
        /// comparison only.</remarks>
        Undefined = 0,
        /// <summary>
        /// The property map defines a custom map operation.
        /// </summary>
        MapToProperty = 1,
        /// <summary>
        /// The property map defines an ignore property operation.
        /// </summary>
        IgnoreProperty = 2
    }

    /// <summary>
    /// Represents a manual mapping of a property in one data type to a property in another data 
    /// type.
    /// </summary>
    public class PropertyMap
    {
        #region Properties
        /// <summary>
        /// Gets the operation the property map object has been configured for.
        /// </summary>
        public PropertyMapOperation Operation { get; }

        /// <summary>
        /// Gets the data type the property belongs to.
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
        /// <param name="operation">The operation this property map defines.  If its value is 
        /// <see cref="PropertyMapOperation.MapToProperty"/>, then a target property name must be 
        /// provided.</param>
        /// <param name="targetProperty">The destination property's name.</param>
        /// <param name="forceStringValue">A Boolean value indicating if forceful string 
        /// conversion will take place before comparison.</param>
        /// <param name="formatString">The format string to use to convert the origin property 
        /// value to string.</param>
        /// <param name="targetFormatString">The format string to use to convert the destination 
        /// property value to string.</param>
        public PropertyMap(Type targetType, PropertyMapOperation operation, string targetProperty = null, bool forceStringValue = false, string formatString = null, string targetFormatString = null)
        {
            Guard.RequiredArgument(targetType, nameof(targetType));
            Guard.ArgumentCondition(
                () => operation != PropertyMapOperation.Undefined,
                nameof(operation),
                "A property map must have a valid operation value set."
            );
            Guard.ArgumentCondition(
                () => operation == PropertyMapOperation.IgnoreProperty || !String.IsNullOrWhiteSpace(targetProperty),
                nameof(targetProperty),
                "A target property name is required for property maps that do not define an ignored property."
            );
            Operation = operation;
            TargetType = targetType;
            TargetProperty = targetProperty;
            FormatString = formatString;
            TargetFormatString = targetFormatString;
            ForceStringValue = forceStringValue;
        }
        #endregion
    }
}