using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace wj.ObjectComparer
{
    internal static class ExpressionHelper
    {
        /// <summary>
        /// Gets the property information object for the property in the given lambda expression.
        /// </summary>
        /// <typeparam name="TObject">The type of object that provides the property in the 
        /// expression.</typeparam>
        /// <typeparam name="TProperty">The type of the property in the expression.</typeparam>
        /// <param name="expr">Lambda expression that defines the property of interest.</param>
        /// <param name="paramName">Parameter name used when throwing exceptions.</param>
        /// <returns>The property information object of the property specified in the lambda 
        /// expression.</returns>
        /// <exception cref="ArgumentException">Thrown if the lambda expression does not conform 
        /// to an expression of a property of the <typeparamref name="TObject"/> data type.</exception>
        public static PropertyInfo GetPropertyInfo<TObject, TProperty>(Expression<Func<TObject, TProperty>> expr, string paramName)
        {
            MemberExpression member = expr.Body as MemberExpression;
            Guard.ArgumentCondition(() => member != null, paramName, $"Expression '{expr}' refers to a method, not a property.");
            PropertyInfo propInfo = member.Member as PropertyInfo;
            Guard.ArgumentCondition(() => propInfo != null, paramName, $"Expression '{expr}' refers to a field, not a property.");
            Type type = typeof(TObject);
            Guard.ArgumentCondition(
                () => type == propInfo.ReflectedType || type.IsSubclassOf(propInfo.ReflectedType),
                paramName,
                $"Expression '{expr}' refers to a property that is not from type {type}."
            );
            return propInfo;
        }
    }
}
