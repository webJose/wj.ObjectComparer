using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Attribute used to mark a data type (usually a data model class) for property scanning in 
    /// order to compare the property values of objects of that type with the property values of 
    /// another object, usually of another type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class ScanForPropertyComparisonAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Gets a Boolean value indicating whether the class associated with this attribute 
        /// object has to be scanned for comparison purposes.
        /// </summary>
        public bool Scan { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="scan">A Boolean value indicating whether the class associated with 
        /// this attribute object has to be scanned for comparison purposes.</param>
        /// <remarks>The default value of the <paramref name="scan"/> parameter is true, meaning 
        /// that by applying this attribute with no arguments will mark the data type (class) for 
        /// scanning.</remarks>
        public ScanForPropertyComparisonAttribute(bool scan = true)
        {
            Scan = scan;
        }
        #endregion
    }
}