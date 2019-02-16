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

        public static PersonExWithPropMap CreatePersonExWithPropMapping()
        {
            PersonExWithPropMap p = CreatePersonCore<PersonExWithPropMap>();
            p.NewNickName = "JP";
            return p;
        }

        public static PersonEx2 CreatePersonEx2(bool nullDate = false)
        {
            PersonEx2 p = CreatePersonCore<PersonEx2>();
            p.BirthDate = nullDate ? (DateTime?)null : DateTime.Now.AddYears(-21);
            return p;
        }

        public static PersonExWithPropMapIgnore CreatePersonExWithPropMapIgnore()
        {
            PersonExWithPropMapIgnore p = CreatePersonCore<PersonExWithPropMapIgnore>();
            p.NewNickName = "Ignored Joe";
            return p;
        }

        public static PersonExWithIgnoreForSelf CreatePersonExWithIgnoreForSelf()
        {
            PersonExWithIgnoreForSelf p = CreatePersonCore<PersonExWithIgnoreForSelf>();
            p.NickName = "IgnoredForSelf boy";
            return p;
        }

        public static PersonExWithIgnore CreatePersonExWithIgnore()
        {
            PersonExWithIgnore p = CreatePersonCore<PersonExWithIgnore>();
            p.NickName = "Ignored Jane";
            return p;
        }


        public static PersonExWithIgnoreForOthers CreatePersonExWithIgnoreForOthers()
        {
            PersonExWithIgnoreForOthers p = CreatePersonCore<PersonExWithIgnoreForOthers>();
            p.NickName = "Racist Menace";
            return p;
        }

        public static PersonExWithStringCoerce CreatePersonExWithStringCoerce(bool nullDate = false)
        {
            PersonExWithStringCoerce p = CreatePersonCore<PersonExWithStringCoerce>();
            p.DateOfBirth = nullDate ? (DateTime?)null : DateTime.Now.AddYears(-21);
            return p;
        }

        public static PersonEx2WithPropertyMap CreatePersonEx2WithPropertyMap(bool nullDate = false)
        {
            PersonEx2WithPropertyMap p = CreatePersonCore<PersonEx2WithPropertyMap>();
            p.DateOfBirth = nullDate ? (DateTime?)null : DateTime.Now.AddYears(-21);
            return p;
        }

        public static PersonEx2NonNullable CreatePersonEx2NonNullable()
        {
            PersonEx2NonNullable p = CreatePersonCore<PersonEx2NonNullable>();
            p.BirthDate = DateTime.Now.AddYears(-21);
            return p;
        }

        public static PersonEx2StringDate CreatePersonEx2StringDate()
        {
            PersonEx2StringDate p = CreatePersonCore<PersonEx2StringDate>();
            p.BirthDate = DateTime.Now.AddYears(-21).ToShortDateString();
            return p;
        }
        #endregion
    }
}
