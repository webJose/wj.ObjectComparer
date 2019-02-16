using System;

namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithStringCoerce : Person
    {
        [PropertyMap(typeof(PersonEx2), PropertyMapOperation.MapToProperty, nameof(PersonEx2.BirthDate),
            ForceStringValue = true, FormatString = "yyyyMMdd", TargetFormatString = "ddMMyyyy")]
        public DateTime? DateOfBirth { get; set; }
    }
}
