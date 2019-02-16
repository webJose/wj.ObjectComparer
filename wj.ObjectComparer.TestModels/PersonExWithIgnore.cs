namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithIgnore : Person
    {
        [IgnoreForComparison(IgnorePropertyOptions.IgnoreForAll)]
        public string NickName { get; set; }
    }
}
