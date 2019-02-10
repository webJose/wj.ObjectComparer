using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Attribute used to ignore a property for property comparison purposes for all data types.  
    /// If the property needs to be selectively ignored depending on the data type of the target 
    /// object, use a <see cref="PropertyMapAttribute"/> attribute instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreForComparisonAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Gets the options set for property ignore.
        /// </summary>
        public IgnorePropertyOptions IgnoreOptions { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public IgnoreForComparisonAttribute(IgnorePropertyOptions ignoreOptions = IgnorePropertyOptions.IgnoreForOthers)
            : base()
        {
            IgnoreOptions = ignoreOptions;
        }
        #endregion
    }
}
