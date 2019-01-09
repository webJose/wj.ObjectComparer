using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Collection of <see cref="PropertyMapping"/> objects.
    /// </summary>
    public class PropertyMappingCollection : DictionaryCollection<Type, PropertyMapping>
    {
        #region Methods
        /// <inheritdoc />
        protected override Type GetKeyForItem(PropertyMapping item)
        {
            return item.TargetType;
        }
        #endregion
    }
}