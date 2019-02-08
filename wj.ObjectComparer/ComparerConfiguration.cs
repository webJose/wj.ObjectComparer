using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace wj.ObjectComparer
{
    public class ComparerConfiguration<TSource, TDestination>
    {
        #region Properties
        private Type Type1 { get; set; }
        private Type Type2 { get; set; }
        private TypeInfo TypeInfo1 { get; set; }
        private TypeInfo TypeInfo2 { get; set; }
        private Dictionary<Type, IComparer> Comparers { get; } = new Dictionary<Type, IComparer>();
        #endregion

        #region Constructors
        internal ComparerConfiguration(bool ignoreAttributedPropertyMappings = false)
        {
            Type1 = typeof(TSource);
            Type2 = typeof(TDestination);
            TypeInfo1 = GetTypeInformation(Type1, ignoreAttributedPropertyMappings);
            TypeInfo2 = GetTypeInformation(Type2, ignoreAttributedPropertyMappings);
        }
        #endregion

        #region Methods
        private TypeInfo GetTypeInformation(Type type, bool ignoreAttributedPropertyMappings)
        {
            bool exists = Scanner.TryGetTypeInformation(type, out TypeInfo ti);
            if (!exists || ti.PropertyMappingsIgnored != ignoreAttributedPropertyMappings)
            {
                ti = Scanner.BuildTypeInformation(type, ignoreAttributedPropertyMappings);
            }
            else if (ti.PropertyMappingsIgnored == ignoreAttributedPropertyMappings)
            {
                ti = ti.Clone();
            }
            return ti;
        }

        private PropertyInfo GetPropertyInfo<TObject, TProperty>(Expression<Func<TObject, TProperty>> expr, string paramName)
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

        public ComparerConfiguration<TSource, TDestination> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropExpr,
            Expression<Func<TDestination, TTargetProperty>> targetPropExpr,
            bool forceStringValue = false,
            string formatString = null,
            string targetFormatString = null
        )
        {
            PropertyInfo piSource = GetPropertyInfo(sourcePropExpr, nameof(sourcePropExpr));
            PropertyInfo piTarget = GetPropertyInfo(targetPropExpr, nameof(targetPropExpr));
            PropertyMap mapping = new PropertyMap(
                Type2,
                PropertyMapOperation.MapToProperty,
                piTarget.Name,
                forceStringValue,
                formatString,
                targetFormatString
            );
            TypeInfo1.Properties[piSource.Name].Mappings.Replace(mapping);
            return this;
        }

        public ComparerConfiguration<TSource, TDestination> IgnoreProperty<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropExpr,
            Type targetType = null
        )
        {
            PropertyInfo piSource = GetPropertyInfo(sourcePropExpr, nameof(sourcePropExpr));
            PropertyComparisonInfo pci = TypeInfo1.Properties[piSource.Name];
            if (targetType == null)
            {
                //Ignore for all data types.
                pci.IgnoreProperty = true;
            }
            else
            {
                //Ignore only for the specified data type.
                PropertyMap mapping = new PropertyMap(
                    Type2,
                    PropertyMapOperation.IgnoreProperty
                );
                pci.Mappings.Replace(mapping);
            }
            return this;
        }

        public ComparerConfiguration<TSource, TDestination> AddComparer<TProperty>(IComparer comparer)
        {
            Comparers[typeof(TProperty)] = comparer;
            return this;
        }

        public ObjectComparer CreateComparer()
        {
            return new ObjectComparer(Type1, Type2, TypeInfo1, TypeInfo2, Comparers);
        }
        #endregion
    }
}
