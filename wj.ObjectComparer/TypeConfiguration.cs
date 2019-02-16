using System;
using System.Linq.Expressions;
using System.Reflection;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Configurator class for a data type.  Configuration specified through configuration objects 
    /// of this class is persisted in the scanner's cache.
    /// </summary>
    /// <typeparam name="TSource">The data type this configuration object has been set to configure.</typeparam>
    public class TypeConfiguration<TSource>
    {
        #region Properties
        /// <summary>
        /// Gets the type information object for the data type of this type configuration object.
        /// </summary>
        internal TypeInfo TypeInfo { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="typeInfo">The type information object for the data type to be configured.</param>
        internal TypeConfiguration(TypeInfo typeInfo)
        {
            Guard.RequiredArgument(typeInfo, nameof(typeInfo));
            TypeInfo = typeInfo;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Ignores a property for comparison result purposes.  A property can be included in the 
        /// comparison process, can be ignored when compared against objects of the same type, can 
        /// be ignored when compared against objects of a different type, or it can be ignored for 
        /// all comparisons.
        /// </summary>
        /// <typeparam name="TProperty">The property's return type.</typeparam>
        /// <param name="propertyExpr">Property lambda expression.</param>
        /// <param name="ignoreOption">The desired ignore option.</param>
        /// <returns>This type configuration object to enable fluent syntax.</returns>
        public TypeConfiguration<TSource> IgnoreProperty<TProperty>(
            Expression<Func<TSource, TProperty>> propertyExpr,
            IgnorePropertyOptions ignoreOption
        )
        {
            PropertyInfo pInfo = ExpressionHelper.GetPropertyInfo(propertyExpr, nameof(propertyExpr));
            lock (Scanner.SyncRoot)
            {
                TypeInfo.Properties[pInfo.Name].IgnoreProperty = ignoreOption;
            }
            return this;
        }

        /// <summary>
        /// Returns a target data type configuration object that serves as specifier of a target 
        /// data type in order to further configure the source data type in the context of this 
        /// target type.
        /// </summary>
        /// <typeparam name="TTarget">The target type to configure.</typeparam>
        /// <returns>A proxy or specifier object type that allows target type-specific 
        /// configuration of the source data type.</returns>
        public TargetTypeConfiguration<TSource, TTarget> ForType<TTarget>()
        {
            return new TargetTypeConfiguration<TSource, TTarget>(this);
        }
        #endregion
    }
}
