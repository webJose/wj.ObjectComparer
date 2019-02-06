using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Defines the outcome of a property comparison operation.
    /// </summary>
    [Flags]
    public enum ComparisonResult
    {
        /// <summary>
        /// The result is undefined.
        /// </summary>
        /// <remarks>This result should never be seen and it is used for initialization and 
        /// comparison only.</remarks>
        Undefined = 0x0,
        /// <summary>
        /// The property in question was not found in the second object.
        /// </summary>
        PropertyNotFound = 0x1,
        /// <summary>
        /// The values of the properties have been deemed equal.
        /// </summary>
        Equal = 0x2,
        /// <summary>
        /// The values of the properties have been deemed not equal.
        /// </summary>
        NotEqual = 0x4,
        /// <summary>
        /// The value in the first object is considered smaller than the value in the second 
        /// object.
        /// </summary>
        LessThan = 0x8 | NotEqual,
        /// <summary>
        /// The value in the first object is considered larger than the value in the second 
        /// object.
        /// </summary>
        GreaterThan = 0x10 | NotEqual,
        /// <summary>
        /// The value of the property in at least one of the objects was coerced to a string 
        /// before the property values were compared.
        /// </summary>
        StringCoercion = 0x1000,
        /// <summary>
        /// The comparison operation resulted in an exception being thrown.
        /// </summary>
        Exception = 0x2000,
        /// <summary>
        /// The comparison operation could not use a comparer object.
        /// </summary>
        NoComparer = 0x4000
    }

    /// <summary>
    /// Represents the result of comparing the values of two properties in two different objects.
    /// </summary>
    public class PropertyComparisonResult
    {
        #region Properties
        /// <summary>
        /// Gets the computed result of the comparison operation.
        /// </summary>
        public ComparisonResult Result { get; }

        /// <summary>
        /// Gets the property information object for the property in the first object.
        /// </summary>
        public PropertyComparisonInfo Property1 { get; }

        /// <summary>
        /// Gets the property information object for the property in the second object.
        /// </summary>
        public PropertyComparisonInfo Property2 { get; }

        /// <summary>
        /// Gets the property value for the property in the first object.
        /// </summary>
        public object Value1 { get; }

        /// <summary>
        /// Gets the property value for the property in the second object.
        /// </summary>
        public object Value2 { get; }

        /// <summary>
        /// Returns the mapping data used for the comparison, if any was defined at the moment of 
        /// comparing the property values.
        /// </summary>
        public PropertyMapping MappingUsed { get; }

        /// <summary>
        /// Returns any exception raised during the comparison process for this property pair.
        /// </summary>
        public System.Exception Exception { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="result">The comparison's computed result.</param>
        /// <param name="property1">Property information for the property in the first object.</param>
        /// <param name="value1">Value found in the property of the first object.</param>
        /// <param name="property2">Property information for the property in the second object.</param>
        /// <param name="value2">Value found in the property of the second object.</param>
        /// <param name="mappingUsed">Mapping information used during the comparison operation.</param>
        /// <param name="exception">Exception raised during the comparison operation, if any.</param>
        public PropertyComparisonResult(ComparisonResult result, PropertyComparisonInfo property1, object value1,
            PropertyComparisonInfo property2 = null, object value2 = null, PropertyMapping mappingUsed = null,
            System.Exception exception = null)
        {
            Result = result;
            Property1 = property1;
            Property2 = property2;
            Value1 = value1;
            Value2 = value2;
            MappingUsed = mappingUsed;
            Exception = exception;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if the specified flag is set in the specified value.
        /// </summary>
        /// <param name="val">The value to be checked for the presence of the flag.</param>
        /// <param name="flag">The flag of interest.</param>
        /// <returns>True if the flag is set in the value; false otherwise.</returns>
        private bool ComparisonResultContains(ComparisonResult val, ComparisonResult flag)
            => (val & flag) == flag;

        /// <inheritdoc />
        public override string ToString()
        {
            string val;
            if (ComparisonResultContains(Result, ComparisonResult.PropertyNotFound))
            {
                val = $"{Result} {Property1}";
            }
            else if (ComparisonResultContains(Result, ComparisonResult.Exception))
            {
                val = $"{Result} {Property1} {Exception.GetType()}: {Exception.Message}";
            }
            else if (Result != ComparisonResult.Undefined && Result != ComparisonResult.StringCoercion)
            {
                string op = null;
                switch (Result & ~(ComparisonResult.StringCoercion | ComparisonResult.NoComparer))
                {
                    case ComparisonResult.Equal:
                        op = "=";
                        break;
                    case ComparisonResult.NotEqual:
                        op = "!=";
                        break;
                    case ComparisonResult.GreaterThan:
                        op = ">";
                        break;
                    case ComparisonResult.LessThan:
                        op = "<";
                        break;
                    default:
                        op = "??";
                        break;
                }
                val = $"{Property1?.Name ?? "(unspecified property)"}:  {Value1 ?? "(null)"} {op} {Value2 ?? "(null)"}";
            }
            else
            {
                //This should never happen.
                val = $"{Result}:  (undocumented)";
            }
            return val;
        }
        #endregion
    }
}