using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace wj.ObjectComparer
{
    public static class ComparerConfigurator
    {
        #region Static Section
        public static ComparerConfiguration<TSource, TDestination> Configure<TSource, TDestination>(bool ignoreAttributedPropertyMappings = false)
        {
            return new ComparerConfiguration<TSource, TDestination>(ignoreAttributedPropertyMappings);
        }

        public static ComparerConfiguration<TSource, TSource> Configure<TSource>(bool ignoreAttributedPropertyMappings = false)
        {
            return new ComparerConfiguration<TSource, TSource>(ignoreAttributedPropertyMappings);
        }
        #endregion
    }
}
