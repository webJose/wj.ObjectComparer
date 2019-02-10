using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Helper class that simplifies the syntax to create comparer configuration objects.
    /// </summary>
    public static class ComparerConfigurator
    {
        #region Static Section
        /// <summary>
        /// Creates a new comparer configuration object for the data types specified.
        /// </summary>
        /// <typeparam name="TSource">The type of the first object to compare.</typeparam>
        /// <typeparam name="TDestination">The type of the second object to compare.</typeparam>
        /// <param name="ignoreAttributedPropertyMappings">Boolean value that controls if type 
        /// scanning should include property maps defined via the <see cref="PropertyMapAttribute"/> 
        /// attribute.  The default value is false, meaning that property maps will not be ignored.</param>
        /// <returns>A newly created comparer configuration object.</returns>
        public static ComparerConfiguration<TSource, TDestination> Configure<TSource, TDestination>(bool ignoreAttributedPropertyMappings = false)
        {
            return new ComparerConfiguration<TSource, TDestination>(ignoreAttributedPropertyMappings);
        }

        /// <summary>
        /// Creates a new comparer configuration object for the data type specified.
        /// </summary>
        /// <typeparam name="TSource">The type of the first and second objects to compare.</typeparam>
        /// <param name="ignoreAttributedPropertyMappings">Boolean value that controls if type 
        /// scanning should include property maps defined via the <see cref="PropertyMapAttribute"/> 
        /// attribute.  The default value is false, meaning that property maps will not be ignored.</param>
        /// <returns>A newly created comparer configuration object.</returns>
        public static ComparerConfiguration<TSource, TSource> Configure<TSource>(bool ignoreAttributedPropertyMappings = false)
        {
            return new ComparerConfiguration<TSource, TSource>(ignoreAttributedPropertyMappings);
        }
        #endregion
    }
}
