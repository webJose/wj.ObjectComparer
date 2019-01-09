using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Object used to compare two objects in a property-by-property basis.
    /// </summary>
    /// <typeparam name="T1">The data type of the first object.</typeparam>
    /// <typeparam name="T2">The data type of the second object.</typeparam>
    public class ObjectComparer<T1, T2>
    {
        #region Private Data
        /// <summary>
        /// Type information about the data type of the first object.
        /// </summary>
        private TypeInfo _classInfo1;

        /// <summary>
        /// Type information about thet data type of the second object.
        /// </summary>
        private TypeInfo _classInfo2;
        #endregion

        #region Properties
        /// <summary>
        /// Gets The data type of the first object.
        /// </summary>
        public Type Type1 { get; } = typeof(T1);

        /// <summary>
        /// Gets the data type of the second object.
        /// </summary>
        public Type Type2 { get; } = typeof(T2);

        /// <summary>
        /// Gets a collection of comparers to be used for a specific data type.
        /// </summary>
        public Dictionary<Type, IComparer> Comparers { get; } = new Dictionary<Type, IComparer>();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class, using the optionally-provided comparers to 
        /// perform property value comparison.
        /// </summary>
        /// <param name="comparers">A collection of comparers to be used during comparison 
        /// execution.</param>
        public ObjectComparer(IDictionary<Type, IComparer> comparers = null)
        {
            Comparers = new Dictionary<Type, IComparer>();
            if (comparers != null)
            {
                foreach (KeyValuePair<Type, IComparer> comparerPair in comparers)
                {
                    Comparers.Add(comparerPair.Key, comparerPair.Value);
                }
            }
            SetScannedTypeInfo(Type1, ref _classInfo1);
            SetScannedTypeInfo(Type2, ref _classInfo2);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the type information of the specified type in the specified container.
        /// </summary>
        /// <param name="type">The type of interest.</param>
        /// <param name="ci">The destination container for the type information object.</param>
        /// <exception cref="NoTypeInformationException">Thrown if the type was not registered 
        /// in the scanner engine.</exception>
        private void SetScannedTypeInfo(Type type, ref TypeInfo ci)
        {
            try
            {
                lock (Scanner.SyncRoot)
                {
                    ci = Scanner.TypeInformation[type];
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new NoTypeInformationException(type, innerException: ex);
            }
        }

        /// <summary>
        /// Converts the specified property value to a string according to its nature and the 
        /// provided format string.
        /// </summary>
        /// <param name="value">The object to be converted to string.</param>
        /// <param name="formatString">The format string to use for the conversion.</param>
        /// <returns>The provided value in string form.</returns>
        private string ConvertPropertyValueToString(object value, string formatString)
        {
            if (value == null) return null;
            if (String.IsNullOrWhiteSpace(formatString)) return value.ToString();
            IFormattable formattableValue = value as IFormattable;
            return formattableValue?.ToString(formatString, null) ?? value.ToString();
        }

        /// <summary>
        /// Resolves which comparer to use for the specified data type.
        /// </summary>
        /// <param name="type">The data type that will be compared.</param>
        /// <returns>A comparer object that will be used for property value comparison.</returns>
        private IComparer ResolveComparerForType(Type type)
        {
            if (Comparers.ContainsKey(type)) return Comparers[type];
            return Scanner.GetGlobalComparerForType(type);
        }

        /// <summary>
        /// Compares the property values of the first object against the property values of the 
        /// second object according to the preset property mapping rules between the two object 
        /// data types.
        /// </summary>
        /// <param name="object1">The first object to be compared against a second object.</param>
        /// <param name="object2">The second object of the comparison operation.</param>
        /// <param name="results">If provided, it will be used to collec the comparison results.</param>
        /// <returns>A collection of <see cref="PropertyComparisonResult"/> objects that detail 
        /// how the values of two properties in two different objects compare to one another.  If 
        /// a collection was provided in the <paramref name="results"/> parameter, the returned 
        /// collection is the collection that was provided through the aforementioned parameter.</returns>
        public IDictionary<string, PropertyComparisonResult> Compare(T1 object1, T2 object2,
            IDictionary<string, PropertyComparisonResult> results = null) => 
            Compare(object1, object2, out var _, results);

        /// <summary>
        /// Compares the property values of the first object against the property values of the 
        /// second object according to the preset property mapping rules between the two object 
        /// data types.
        /// </summary>
        /// <param name="object1">The first object to be compared against a second object.</param>
        /// <param name="object2">The second object of the comparison operation.</param>
        /// <returns>A Boolean value with the summarized result of the comparison.  True if any 
        /// property values were deemed different; false if all property values turned out equal.</returns>
        public bool Compare(T1 object1, T2 object2)
        {
            Compare(object1, object2, out bool isDifferent);
            return isDifferent;
        }

        /// <summary>
        /// Compares the property values of the first object against the property values of the 
        /// second object according to the preset property mapping rules between the two object 
        /// data types.
        /// </summary>
        /// <param name="object1">The first object to be compared against a second object.</param>
        /// <param name="object2">The second object of the comparison operation.</param>
        /// <param name="isDifferent"></param>
        /// <param name="results">If provided, it will be used to collec the comparison results.</param>
        /// <returns>A collection of <see cref="PropertyComparisonResult"/> objects that detail 
        /// how the values of two properties in two different objects compare to one another.  If 
        /// a collection was provided in the <paramref name="results"/> parameter, the returned 
        /// collection is the collection that was provided through the aforementioned parameter.</returns>
        public IDictionary<string, PropertyComparisonResult> Compare(T1 object1, T2 object2, out bool isDifferent,
            IDictionary<string, PropertyComparisonResult> results = null)
        {
            if (results == null) results = new PropertyComparisonResultKeyedCollection();
            isDifferent = false;
            foreach (PropertyInfo propertyInfo in _classInfo1.Properties)
            {
                ComparisonResult result = ComparisonResult.Undefined;
                //Obtain the PropertyMapping for this propertyInfo.
                //If none, map by property name.
                PropertyMapping mappingToUse = null;
                if (propertyInfo.Mappings.Contains(Type2))
                {
                    mappingToUse = propertyInfo.Mappings[Type2];
                }
                string prop2Name = mappingToUse == null ? propertyInfo.Name : mappingToUse.TargetProperty;
                //Get the property value of the first object.
                object val1 = propertyInfo.GetValue(object1);
                object val2 = null;
                PropertyInfo propertyInfo2 = null;
                System.Exception comparisonException = null;
                if (_classInfo2.Properties.Contains(prop2Name))
                {
                    //Get the property value of the second object.
                    propertyInfo2 = _classInfo2.Properties[prop2Name];
                    val2 = propertyInfo2.GetValue(object2);
                    //Determine the comparer to use.
                    IComparer comparer = null;
                    if ((mappingToUse != null && mappingToUse.ForceStringValue) ||
                        propertyInfo.PropertyType != propertyInfo2.PropertyType)
                    {
                        comparer = ResolveComparerForType(typeof (string));
                        val1 = ConvertPropertyValueToString(val1, mappingToUse == null ? null : mappingToUse.FormatString);
                        val2 = ConvertPropertyValueToString(val2, mappingToUse == null ? null : mappingToUse.TargetFormatString);
                        result |= ComparisonResult.StringCoercion;
                    }
                    else
                    {
                        comparer = ResolveComparerForType(propertyInfo.PropertyType);
                    }
                    try
                    {
                        int comp = comparer.Compare(val1, val2);
                        if (comp < 0)
                        {
                            result |= ComparisonResult.LessThan;
                        }
                        else if (comp > 0)
                        {
                            result |= ComparisonResult.GreaterThan;
                        }
                        else
                        {
                            result |= ComparisonResult.Equal;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        comparisonException = ex;
                    }
                }
                else
                {
                    //We are done here since there is no matching property to compare against.
                    result |= ComparisonResult.PropertyNotFound;
                }
                PropertyComparisonResult pcr = new PropertyComparisonResult(result, propertyInfo, val1, propertyInfo2,
                    val2, mappingToUse, comparisonException);
                results.Add(null, pcr);
                isDifferent = isDifferent || ((result & (ComparisonResult.GreaterThan | ComparisonResult.LessThan)) !=
                              ComparisonResult.Undefined);
            }
            return results;
        }
        #endregion
    }
}