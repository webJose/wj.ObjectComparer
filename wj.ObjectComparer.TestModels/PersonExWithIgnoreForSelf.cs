namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithIgnoreForSelf : Person
    {
        [IgnoreForComparison(IgnorePropertyOptions.IgnoreForSelf)]
        public string NickName { get; set; }
    }
}
