using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Configurator class for a target type bound to a source type.  In other words, a 
    /// configurator class that helps configure a source data type comparison behavior when 
    /// comparing against objects of the target type.
    /// </summary>
    /// <typeparam name="TSource">The data type this configuration object's parent configuration 
    /// object has been set to configure.</typeparam>
    /// <typeparam name="TTarget">The data type of potential target objects in a comparison 
    /// operation.</typeparam>
    public class TargetTypeConfiguration<TSource, TTarget>
    {
        #region Properties
        /// <summary>
        /// Gets the target object's type.
        /// </summary>
        private Type TargetType { get; } = typeof(TTarget);

        /// <summary>
        /// Gets the parent configuration object.
        /// </summary>
        private TypeConfiguration<TSource> Parent { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="parent">The parent object (or creator) of this object.</param>
        internal TargetTypeConfiguration(TypeConfiguration<TSource> parent)
        {
            Parent = parent;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets a new property map (or replaces any existing map) that defines the behavior of 
        /// the comparison routine when comparing objects of type <typeparamref name="TSource"/> 
        /// against objects of type <typeparamref name="TTarget"/>.
        /// A map can be used to point a property in the source object to an arbitrarily-named 
        /// property in the target object, or it can be used to ignore the property altogether 
        /// so its value will not account for differences between de objects being compared.
        /// </summary>
        /// <typeparam name="TSourceProperty">The type of the source property.</typeparam>
        /// <typeparam name="TTargetProperty">The type of the target property.</typeparam>
        /// <param name="sourcePropExpr">Source property lambda expression.</param>
        /// <param name="targetPropExpr">Target property lambda expression.</param>
        /// <param name="forceStringValue">Optional Boolean value that determines if the 
        /// comparison routine should coerce both source and target values to a string before 
        /// performing the comparison.</param>
        /// <param name="formatString">Format string to use during string coercion of the source 
        /// property value.</param>
        /// <param name="targetFormatString">Format string to use during string coercion of the 
        /// target property value.</param>
        /// <returns>This target type configuration object to enable fluent syntax.</returns>
        public TargetTypeConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropExpr,
            Expression<Func<TTarget, TTargetProperty>> targetPropExpr,
            bool forceStringValue = false,
            string formatString = null,
            string targetFormatString = null
        )
        {
            PropertyInfo piSource = ExpressionHelper.GetPropertyInfo(sourcePropExpr, nameof(sourcePropExpr));
            PropertyInfo piTarget = ExpressionHelper.GetPropertyInfo(targetPropExpr, nameof(targetPropExpr));
            PropertyMap map = new PropertyMap(
                TargetType,
                PropertyMapOperation.MapToProperty,
                piTarget.Name,
                forceStringValue,
                formatString,
                targetFormatString
            );
            lock (Scanner.SyncRoot)
            {
                Parent.TypeInfo.Properties[piSource.Name].Maps.Replace(map);
            }
            return this;
        }

        /// <summary>
        /// Configures the property comparison information of the source property to ignore the 
        /// specified property during the comparison routine against objects of type 
        /// <see cref="TDestination"/>.
        /// </summary>
        /// <typeparam name="TSourceProperty">The type of the source property.</typeparam>
        /// <param name="sourcePropExpr">Source property lambda expression.</param>
        /// <returns>This target type configuration object to enable fluent syntax.</returns>
        public TargetTypeConfiguration<TSource, TTarget> IgnoreProperty<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropExpr
        )
        {
            PropertyInfo piSource = ExpressionHelper.GetPropertyInfo(sourcePropExpr, nameof(sourcePropExpr));
            PropertyMap map = new PropertyMap(TargetType, PropertyMapOperation.IgnoreProperty);
            lock (Scanner.SyncRoot)
            {
                Parent.TypeInfo.Properties[piSource.Name].Maps.Replace(map);
            }
            return this;
        }

        /// <summary>
        /// Returns a target data type configuration object that serves as specifier of a target 
        /// data type in order to further configure the source data type in the context of this 
        /// target type.
        /// NOTE:  This is just a shortcut to this object's parent method of the same name.
        /// </summary>
        /// <typeparam name="TTargetChild">The target type to configure.</typeparam>
        /// <returns>A proxy or specifier object type that allows target type-specific 
        /// configuration of the source data type.</returns>
        [ExcludeFromCodeCoverage]
        public TargetTypeConfiguration<TSource, TTargetChild> ForType<TTargetChild>()
        {
            return Parent.ForType<TTargetChild>();
        }
        #endregion
    }
}
