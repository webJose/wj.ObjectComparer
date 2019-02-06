using System;

namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithPropMapping : Person
    {
        [PropertyMapping(typeof(PersonEx), nameof(PersonEx.NickName))]
        public string NewNickName { get; set; }
    }
}
