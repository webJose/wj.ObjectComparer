using System;
using System.Collections.Generic;
using System.Text;
using wj.ObjectComparer.TestModels;

namespace wj.ObjectComparer.Tests
{
    internal static class ModelsHelper
    {
        #region Methods
        public static TPerson CreatePersonCore<TPerson>()
            where TPerson : Person, new()
        {
            return new TPerson()
            {
                Email = "someEmail@someDomain.com",
                FirstName = "John",
                LastName = "Doe",
                Gender = Genders.Male,
                Id = 1
            };
        }
        public static Person CreatePerson()
        {
            return CreatePersonCore<Person>();
        }

        public static PersonEx CreatePersonEx()
        {
            PersonEx p = CreatePersonCore<PersonEx>();
            p.NickName = "CoolJoe";
            return p;
        }

        public static PersonByRefUdt CreatePersonByRefUdt()
        {
            PersonByRefUdt p = CreatePersonCore<PersonByRefUdt>();
            p.ByRefProperty = new UdtClass();
            return p;
        }

        public static PersonByValUdt CreatePersonByValUdt()
        {
            return CreatePersonCore<PersonByValUdt>();
        }

        public static PersonExWithPropMapping CreatePersonExWithPropMapping()
        {
            PersonExWithPropMapping p = CreatePersonCore<PersonExWithPropMapping>();
            p.NewNickName = "JP";
            return p;
        }

        public static PersonEx2 CreatePersonEx2()
        {
            PersonEx2 p = CreatePersonCore<PersonEx2>();
            p.BirthDate = DateTime.Now.AddYears(-21);
            return p;
        }
        #endregion
    }
}
