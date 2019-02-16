using System;

namespace wj.ObjectComparer.TestModels
{
    public class PersonEx2WithPropertyMap : Person
    {
        [PropertyMap(typeof(PersonEx2), PropertyMapOperation.MapToProperty, nameof(PersonEx2.BirthDate))]
        public DateTime? DateOfBirth { get; set; }
    }
}
