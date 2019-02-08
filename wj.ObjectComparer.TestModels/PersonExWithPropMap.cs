using System;

namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithPropMap : Person
    {
        [PropertyMap(typeof(PersonEx), PropertyMapOperation.MapToProperty, nameof(PersonEx.NickName))]
        public string NewNickName { get; set; }
    }
}
