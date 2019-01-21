using System;
using wj.ObjectComparer.Tester.Models;

namespace wj.ObjectComparer.ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Scanner.RegisterType(typeof(Person));
            Person p1 = new Person()
            {
                BirthDate = DateTime.Now.AddYears(-15),
                Email = "webJose@gmail.com",
                FirstName = "José",
                LastName = "Ramírez",
                Gender = Genders.Male,
                Id = 1
            };
            Person p2 = new Person()
            {
                BirthDate = p1.BirthDate.AddDays(-50),
                Email = p1.Email,
                FirstName = p1.FirstName,
                LastName = p1.LastName + " Vargas",
                Gender = Genders.Female,
                Id = p1.Id + 1
            };
            ObjectComparer comparer = ObjectComparer.Create<Person>();
            var result = comparer.Compare(p1, p2, out var isDifferent);
            Console.WriteLine($"Persons are different:  {isDifferent}");
            Console.WriteLine();
            Console.WriteLine($"Properties compared:  {result.Count}");
            Console.WriteLine("========================");
            foreach (PropertyComparisonResult pcr in result.Values)
            {
                Console.WriteLine($"{pcr.Result, 15}   {pcr}");
            }
        }
    }
}
