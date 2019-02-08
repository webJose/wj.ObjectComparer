using System;

namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithPropMapIgnore : Person
    {
        [PropertyMap(typeof(PersonEx), PropertyMapOperation.IgnoreProperty)]
        public string NewNickName { get; set; }
    }
}
