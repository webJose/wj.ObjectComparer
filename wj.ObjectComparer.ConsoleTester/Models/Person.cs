using System;

namespace wj.ObjectComparer.Tester.Models
{
    public enum Genders
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    }

    public class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Genders Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
