using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Attribute used to ignore a property for property comparison purposes for all data types.  
    /// If the property needs to be selectively ignored depending on the data type of the target 
    /// object, use a <see cref="PropertyMapAttribute"/> attribute instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnoreForComparisonAttribute : Attribute
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public IgnoreForComparisonAttribute()
            : base()
        { }
        #endregion
    }
}
