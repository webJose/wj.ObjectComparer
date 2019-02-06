using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
#if NET461
using System.ComponentModel.DataAnnotations;
#endif

namespace wj.ObjectComparer
{
    /// <summary>
    /// Scanner engine that holds type information about classes as well as comparer objects for 
    /// comparison operation executions.
    /// </summary>
    public static class Scanner
    {
        #region Static Section

        #region Fields
        /// <summary>
        /// Use this object to synchronize access to <see cref="Scanner.TypeInformation"/> and 
        /// <see cref="Scanner._comparers"/>.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Type information to be used for property-by-property object comparison.  If a data 
        /// type is going to be compared, its type information must be registered in this 
        /// collection before the operation can succeed.  Access to this collection must be 
        /// synchronized using <see cref="Scanner.SyncRoot"/>.
        /// </summary>
        /// <remarks>Use the <see cref="Scanner.RegisterType(Type)"/> method to register a type 
        /// individually or the <see cref="Scanner.ScanAssembly(Assembly)"/> method to register 
        /// any data type marked with the <see cref="ScanForPropertyComparisonAttribute"/> 
        /// attribute.</remarks>
        internal static readonly TypeInfoCollection TypeInformation = new TypeInfoCollection();

        /// <summary>
        /// Collection of comparers for types.  Access to this field must be synchronized using 
        /// <see cref="Scanner.SyncRoot"/>.
        /// </summary>
        private static readonly Dictionary<Type, IComparer> _comparers = new Dictionary<Type, IComparer>()
        {
            [typeof(string)] = StringComparer.CurrentCulture
        };
        #endregion

        #region Methods
        /// <summary>
        /// Creates a comparer object for the specified data type.
        /// </summary>
        /// <param name="type">The data type of interest.</param>
        /// <returns>An object that implements the <see cref="IComparer{T}"/> interface, where 
        /// the T data type refers to the type specified through the <paramref name="type"/> 
        /// parameter.</returns>
        internal static IComparer CreateComparerForType(Type type)
        {
            Type genericType = typeof(Comparer<>).MakeGenericType(type);
            return (from pi in genericType.GetProperties() where pi.Name == "Default" select pi.GetValue(null) as IComparer).FirstOrDefault();
        }

        /// <summary>
        /// Returns a comparer object for the specified type from the collection of registered 
        /// comparer objects.
        /// </summary>
        /// <param name="type">The data type to be compared.</param>
        /// <returns>An object that can compare the specified data type.</returns>
        public static IComparer GetGlobalComparerForType(Type type)
        {
            lock (SyncRoot)
            {
                if (_comparers.ContainsKey(type)) return _comparers[type];
                IComparer comparer = CreateComparerForType(type);
                _comparers.Add(type, comparer);
                return comparer;
            }
        }

        internal static bool TryGetTypeInformation(Type type, out TypeInfo typeInfo)
        {
            lock(SyncRoot)
            {
                bool exists = TypeInformation.Contains(type);
                typeInfo = exists ? TypeInformation[type] : null;
                return exists;
            }
        }

        /// <summary>
        /// Registers a comparer object for a specific data type.  Use this method to ensure a 
        /// particular comparer is used for any specific purpose.
        /// </summary>
        /// <param name="type">The data type to compare.</param>
        /// <param name="comparer">The comparer object to be used to perform comparisons.</param>
        public static void RegisterGlobalComparerForType(Type type, IComparer comparer)
        {
            lock (SyncRoot)
            {
                _comparers[type] = comparer;
            }
        }

        /// <summary>
        /// Creates and returns a list of <see cref="PropertyComparisonInfo"/> objects from a list of 
        /// <see cref="PropertyDescriptor"/> objects.
        /// </summary>
        /// <param name="pdColl">The collection of property descriptors.</param>
        /// <param name="ignorePropertyMappings">A Boolean value that indicates if property 
        /// mappings defined through attributes should be ignored.</param>
        /// <returns>An enumeration that lists the created <see cref="PropertyComparisonInfo"/> objects.</returns>
        private static IEnumerable<PropertyComparisonInfo> ObtainPropertyInfos(PropertyDescriptorCollection pdColl, bool ignorePropertyMappings)
        {
            foreach (PropertyDescriptor pd in pdColl)
            {
                PropertyComparisonInfo pi = new PropertyComparisonInfo(pd);
                if (!ignorePropertyMappings)
                {
                    //Obtain mappings.
                    foreach (PropertyMappingAttribute attribute in pd.Attributes.OfType<PropertyMappingAttribute>())
                    {
                        if (attribute == null) continue;
                        pi.Mappings.Add(attribute.PropertyMapping);
                    }
                }
                yield return pi;
            }
        }

        /// <summary>
        /// Creates a <see cref="TypeInfo"/> object with the necessary property information to 
        /// compare objects of the specified type.
        /// </summary>
        /// <param name="type">The data type whose type information is requested.</param>
        /// <param name="ignorePropertyMappings">A Boolean value that indicates if property 
        /// mappings defined through attributes should be ignored.  If the argument is not 
        /// provided, then by default the property mappings are included.</param>
        /// <returns>A <see cref="TypeInfo"/> object that can be used to compare properties.</returns>
        internal static TypeInfo BuildTypeInformation(Type type, bool ignorePropertyMappings = false)
        {
            TypeInfo ti = new TypeInfo(type, ignorePropertyMappings);
#if NET461
            //Obtain PropertyMapping data from MetadataTypeAttribute, if present.
            PropertyComparisonInfoCollection metadataOnlyPropertyInfos = new PropertyComparisonInfoCollection();
            MetadataTypeAttribute att = type.GetCustomAttribute<MetadataTypeAttribute>();
            if (att != null)
            {
                foreach (PropertyComparisonInfo pi in ObtainPropertyInfos(TypeDescriptor.GetProperties(att.MetadataClassType), ignorePropertyMappings))
                {
                    metadataOnlyPropertyInfos.Add(pi);
                }
            }
#endif
            //Now process regular property descriptors.
            foreach (PropertyComparisonInfo pi in ObtainPropertyInfos(TypeDescriptor.GetProperties(type), ignorePropertyMappings))
            {
#if NET461
                    if (!ignorePropertyMappings && metadataOnlyPropertyInfos.Contains(pi.Name))
                    {
                        //Merge the PropertyMapping objects.
                        foreach (PropertyMapping pm in metadataOnlyPropertyInfos[pi.Name].Mappings)
                        {
                            if (pi.Mappings.Contains(pm.TargetType)) continue;
                            pi.Mappings.Add(pm);
                        }
                    }
#endif
                ti.Properties.Add(pi);
            }
            return ti;
        }

        /// <summary>
        /// Registers a data type so it is ready for property-by-property object comparison.
        /// </summary>
        /// <param name="type">The data type to register.</param>
        public static void RegisterType(Type type)
        {
            TypeInfo ti = BuildTypeInformation(type);
            //Register the type information object in the scanner's collection.
            lock (SyncRoot)
            {
                if (!TypeInformation.Contains(ti.TargetType))
                {
                    TypeInformation.Add(ti);
                }
            }
        }

        /// <summary>
        /// Unregisters a data type from the global type registry.  Unregistered types can no 
        /// longer be used to create object comparers, but any previously-created comparers will 
        /// work.
        /// </summary>
        /// <param name="type">The data type to unregister.</param>
        public static void UnregisterType(Type type)
        {
            lock(SyncRoot)
            {
                if (TypeInformation.Contains(type))
                {
                    TypeInformation.Remove(type);
                }
            }
        }

        /// <summary>
        /// Scans a .Net assembly for types that have been marked with the 
        /// <see cref="ScanForPropertyComparisonAttribute"/> attribute.  These types are 
        /// registered for property-by-property comparison.
        /// </summary>
        /// <param name="assembly">The .Net assembly to scan.</param>
        public static void ScanAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                ScanForPropertyComparisonAttribute addToScanner = type.GetCustomAttribute<ScanForPropertyComparisonAttribute>();
                if ((addToScanner != null && !addToScanner.Scan) || addToScanner == null) continue;
                RegisterType(type);
            }
        }
        #endregion

        #endregion
    }
}