using System;
using System.Collections;
using System.Collections.Generic;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Object used to compare two objects in a property-by-property basis.
    /// </summary>
    public class ObjectComparer
    {
        #region Static Section
        /// <summary>
        /// Creates a new <see cref="ObjectComparer"/> object capable of comparing objects of the 
        /// same type.
        /// </summary>
        /// <typeparam name="T">The type of object to compare.  This type must have been 
        /// previously registered with this class library's type scanner.</typeparam>
        /// <returns>A newly created <see cref="ObjectComparer"/> object.</returns>
        public static ObjectComparer Create<T>()
        {
            return new ObjectComparer(typeof(T), typeof(T));
        }

        /// <summary>
        /// Creates a new <see cref="ObjectComparer"/> object capable of comparing objects of 
        /// different types.
        /// </summary>
        /// <typeparam name="T1">The data type of the first object.</typeparam>
        /// <typeparam name="T2">The data type of the second object.</typeparam>
        /// <returns>A newly created <see cref="ObjectComparer"/> object.</returns>
        public static ObjectComparer Create<T1, T2>()
        {
            return new ObjectComparer(typeof(T1), typeof(T2));
        }
        #endregion

        #region Private Data
        /// <summary>
        /// Type information about the data type of the first object.
        /// </summary>
        private TypeInfo _typeInfo1;

        /// <summary>
        /// Type information about thet data type of the second object.
        /// </summary>
        private TypeInfo _typeInfo2;
        #endregion

        #region Properties
        /// <summary>
        /// Gets The data type of the first object.
        /// </summary>
        public Type Type1 { get; }

        /// <summary>
        /// Gets the data type of the second object.
        /// </summary>
        public Type Type2 { get; }

        /// <summary>
        /// Gets a collection of comparers to be used for a specific data type.
        /// </summary>
        public Dictionary<Type, IComparer> Comparers { get; } = new Dictionary<Type, IComparer>();
        #endregion

        #region Constructors
        /// <summary>
        /// Crests a new instance of this class capable of comparing objects of the data types 
        /// specified.
        /// </summary>
        /// <param name="type1">The type of the first object to compare.</param>
        /// <param name="type2">The type of the second object to compare.</param>
        /// <param name="comparers">A collection of comparer objects for property data types.</param>
        private ObjectComparer(Type type1, Type type2, IDictionary<Type, IComparer> comparers)
        {
            Type1 = type1;
            Type2 = type2;
            if (comparers?.Count > 0)
            {
                foreach (KeyValuePair<Type, IComparer> comparerPair in comparers)
                {
                    Comparers.Add(comparerPair.Key, comparerPair.Value);
                }
            }
        }

        /// <summary>
        /// Crests a new instance of this class capable of comparing objects of the data types 
        /// specified.
        /// </summary>
        /// <param name="type1">The type of the first object to compare.</param>
        /// <param name="type2">The type of the second object to compare.</param>
        /// <param name="typeInfo1">Type information for the first data type.</param>
        /// <param name="typeInfo2">Type information for the second data type.</param>
        /// <param name="comparers">A collection of comparer objects for property data types.</param>
        internal ObjectComparer(Type type1, Type type2, TypeInfo typeInfo1, TypeInfo typeInfo2, IDictionary<Type, IComparer> comparers)
            : this(type1, type2, comparers)
        {
            _typeInfo1 = typeInfo1;
            _typeInfo2 = typeInfo2;
        }

        /// <summary>
        /// Crests a new instance of this class capable of comparing objects of the data types 
        /// specified.
        /// </summary>
        /// <param name="type1">The type of the first object to compare.</param>
        /// <param name="type2">The type of the second object to compare.</param>
        /// <exception cref="NoTypeInformationException">Thrown if the type of either object was 
        /// not registered with the scanner engine.</exception>
        public ObjectComparer(Type type1, Type type2)
            : this(type1, type2, null)
        {
            SetScannedTypeInfo(Type1, ref _typeInfo1);
            SetScannedTypeInfo(Type2, ref _typeInfo2);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the type information of the specified type in the specified container.
        /// </summary>
        /// <param name="type">The type of interest.</param>
        /// <param name="ci">The destination container for the type information object.</param>
        /// <exception cref="NoTypeInformationException">Thrown if the type was not registered 
        /// with the scanner engine.</exception>
        private void SetScannedTypeInfo(Type type, ref TypeInfo ci)
        {
            if (!Scanner.TryGetTypeInformation(type, out ci))
            {
                throw new NoTypeInformationException(type);
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
            if (formattableValue != null)
            {
                return formattableValue.ToString(formatString, null);
            }
            return value.ToString();
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
        /// Gets the underlying (or base) type of a nullable type, or null if the type to test is 
        /// not <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The type to test for a base type.</param>
        /// <returns>The underlying, or base, type of the given type.</returns>
        private Type GetNullableUnderlyingType(Type type) => Nullable.GetUnderlyingType(type);

        /// <summary>
        /// Compares the property values of the first object against the property values of the 
        /// second object according to the preset property mapping rules between the two object 
        /// data types.
        /// </summary>
        /// <param name="object1">The first object to be compared against a second object.</param>
        /// <param name="object2">The second object of the comparison operation.</param>
        /// <param name="results">If provided, it will be used to collect the comparison results.</param>
        /// <returns>A Boolean value with the summarized result of the comparison.  True if any 
        /// property values were deemed different; false if all property values turned out equal.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either object is null.</exception>
        /// <exception cref="ArgumentException">Thrown if either object is not of the expected 
        /// data type.</exception>
        /// <exception cref="InvalidOperationException">Thrown if both objects are really the same 
        /// object.</exception>
        public bool Compare(object object1, object object2, ICollection<PropertyComparisonResult> results = null)
        {
            #region Argument Validation
            Guard.RequiredArgument(object1, nameof(object1));
            Guard.RequiredArgument(object2, nameof(object2));
            Guard.ArgumentCondition(
                () => object1.GetType() == Type1, nameof(object1),
                $"The provided object is not of the expected type ({Type1})."
            );
            Guard.ArgumentCondition(
                () => object2.GetType() == Type2, nameof(object2),
                $"The provided object is not of the expected type ({Type2})."
            );
            Guard.Condition(() => !Object.ReferenceEquals(object1, object2), "The objects to compare must be different.");
            #endregion

            bool isDifferent = false;
            foreach (PropertyComparisonInfo pci1 in _typeInfo1.Properties)
            {
                ComparisonResult result = ComparisonResult.Undefined;
                //Obtain the PropertyMapping for this propertyInfo.
                //If none, map by property name.
                PropertyMap mapToUse = null;
                if (pci1.Maps.Contains(Type2))
                {
                    mapToUse = pci1.Maps[Type2];
                }
                object val1 = null;
                object val2 = null;
                PropertyComparisonInfo pci2 = null;
                System.Exception catchedException = null;
                //Ignore the property if no mapping exists and is being ignored for type 2, 
                //or mapping exists and it states the property must be ignored.
                bool propertyIgnored =
                    ((pci1.IgnoreProperty & IgnorePropertyOptions.IgnoreForSelf) == IgnorePropertyOptions.IgnoreForSelf && Type1 == Type2) ||
                    ((pci1.IgnoreProperty & IgnorePropertyOptions.IgnoreForOthers) == IgnorePropertyOptions.IgnoreForOthers && Type1 != Type2);
                if ((mapToUse == null && propertyIgnored)
                    || (mapToUse?.Operation == PropertyMapOperation.IgnoreProperty))
                {
                    result |= ComparisonResult.PropertyIgnored;
                }
                else
                {
                    string prop2Name = mapToUse?.TargetProperty ?? pci1.Name;
                    //Get the property value of the first object.
                    val1 = pci1.GetValue(object1);
                    if (_typeInfo2.Properties.Contains(prop2Name))
                    {
                        //Get the property value of the second object.
                        pci2 = _typeInfo2.Properties[prop2Name];
                        val2 = pci2.GetValue(object2);
                        //Determine any base type to cover for T? vs T or vice versa.
                        Type p1BaseType = GetNullableUnderlyingType(pci1.PropertyType) ?? pci1.PropertyType;
                        Type p2BaseType = GetNullableUnderlyingType(pci2.PropertyType) ?? pci2.PropertyType;
                        //Determine the comparer to use.
                        IComparer comparer = null;
                        if ((mapToUse?.ForceStringValue ?? false) ||
                            p1BaseType != p2BaseType)
                        {
                            comparer = ResolveComparerForType(typeof(string));
                            try
                            {
                                val1 = ConvertPropertyValueToString(val1, mapToUse?.FormatString);
                                val2 = ConvertPropertyValueToString(val2, mapToUse?.TargetFormatString);
                            }
                            catch (System.Exception ex)
                            {
                                result |= ComparisonResult.StringCoercionException;
                                catchedException = ex;
                            }
                            result |= ComparisonResult.StringCoercion;
                        }
                        else
                        {
                            comparer = ResolveComparerForType(p1BaseType);
                        }
                        if (catchedException == null)
                        {
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
                            catch (System.ArgumentException)
                            {
                                //Property type does not implement IComparable and there is no comparer 
                                //registered for the data type.
                                result |= ComparisonResult.NoComparer;
                                //So try to at least find out if it is equal or not.
                                try
                                {
                                    if (Object.Equals(val1, val2))
                                    {
                                        result |= ComparisonResult.Equal;
                                    }
                                    else
                                    {
                                        result |= ComparisonResult.NotEqual;
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    result |= ComparisonResult.ComparisonException;
                                    catchedException = ex;
                                }
                            }
                            catch (System.Exception ex)
                            {
                                result |= ComparisonResult.ComparisonException;
                                catchedException = ex;
                            }
                        }
                    }
                    else
                    {
                        //We are done here since there is no matching property to compare against.
                        result |= ComparisonResult.PropertyNotFound;
                    }
                }
                PropertyComparisonResult pcr = new PropertyComparisonResult(result, pci1, val1, pci2,
                    val2, mapToUse, catchedException);
                results?.Add(pcr);
                isDifferent = isDifferent || ((result & ComparisonResult.NotEqual) ==
                              ComparisonResult.NotEqual);
            }
            return isDifferent;
        }

        /// <summary>
        /// Compares the property values of the first object against the property values of the 
        /// second object according to the preset property mapping rules between the two object 
        /// data types.
        /// </summary>
        /// <param name="object1">The first object to be compared against a second object.</param>
        /// <param name="object2">The second object of the comparison operation.</param>
        /// <param name="isDifferent">A Boolean out parameter that contains an overall result of 
        /// the comparison:  It will be true if there is any difference in any of the property 
        /// values; false if all property values turn out to be equal.</param>
        /// <returns>A collection of <see cref="PropertyComparisonResult"/> objects that detail 
        /// how the values of two properties in two different objects compare to one another.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either object is null.</exception>
        /// <exception cref="ArgumentException">Thrown if either object is not of the expected 
        /// data type.</exception>
        /// <exception cref="InvalidOperationException">Thrown if both objects are really the same 
        /// object.</exception>
        public PropertyComparisonResultCollection Compare(object object1, object object2, out bool isDifferent)
        {
            PropertyComparisonResultCollection results = new PropertyComparisonResultCollection();
            isDifferent = Compare(object1, object2, results);
            return results;
        }
        #endregion
    }
}