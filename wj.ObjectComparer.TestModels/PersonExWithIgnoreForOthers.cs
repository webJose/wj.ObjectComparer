namespace wj.ObjectComparer.TestModels
{
    public class PersonExWithIgnoreForOthers : Person
    {
        [IgnoreForComparison(IgnorePropertyOptions.IgnoreForOthers)]
        public string NickName { get; set; }
    }
}
