using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using wj.ObjectComparer.TestModels;

namespace wj.ObjectComparer.Tests
{
    [TestFixture(Category = nameof(ObjectComparer))]
    public class ObjectComparerTests
    {
        #region Setups
        private static Type[] modelTypes =
        {
            typeof(Person),
            typeof(PersonEx),
            typeof(PersonByRefUdt),
            typeof(PersonByValUdt)
        };
        [OneTimeSetUp]
        public void ModelRegistration()
        {
            foreach (Type type in modelTypes)
            {
                Scanner.RegisterType(type);
            }
        }

        [OneTimeTearDown]
        public void ModelUnregistration()
        {
            foreach (Type type in modelTypes)
            {
                Scanner.UnregisterType(type);
            }
        }
        #endregion

        #region Test Data
        private static IEnumerable CompareWithNullTestData()
        {
            string rootName = nameof(CompareWithNull);
            Person p = ModelsHelper.CreatePerson();
            yield return new TestCaseData(null, p, "object1")
                .SetName($"{rootName} object1 null");
            yield return new TestCaseData(p, null, "object2")
                .SetName($"{rootName} object2 null");
        }

        private static IEnumerable CompareTypeMismatchTestData()
        {
            string rootName = nameof(CompareTypeMismatch);
            Person p = ModelsHelper.CreatePerson();
            PersonByRefUdt p2 = new PersonByRefUdt();
            yield return new TestCaseData(p, p)
                .SetName($"{rootName} ({nameof(Person)}, {nameof(Person)})");
            yield return new TestCaseData(p2, p2)
                .SetName($"{rootName} ({nameof(PersonByRefUdt)}, {nameof(PersonByRefUdt)})");
        }

        private static IEnumerable ObjectComparerForUnregisteredTypeTestData()
        {
            string rootName = nameof(ObjectComparerForUnregisteredType);
            Action a = () => ObjectComparer.Create<DateTime>();
            yield return new TestCaseData(a)
                .SetName($"{rootName} Single Type");
            a = () => ObjectComparer.Create<DateTime, Person>();
            yield return new TestCaseData(a)
                .SetName($"{rootName} Two Types First Type");
            a = () => ObjectComparer.Create<Person, DateTime>();
            yield return new TestCaseData(a)
                .SetName($"{rootName} Two Types Second Type");
        }

        private static IEnumerable IComparerResultTestData()
        {
            string rootName = nameof(IComparerResult);
            yield return new TestCaseData(-1, ComparisonResult.LessThan)
                .SetName($"{rootName} Less Than");
            yield return new TestCaseData(0, ComparisonResult.Equal)
                .SetName($"{rootName} Equal");
            yield return new TestCaseData(1, ComparisonResult.GreaterThan)
                .SetName($"{rootName} Greater Than");
        }

        private static IEnumerable ByRefUserDefinedTypeTestData()
        {
            string rootName = nameof(ByRefUserDefinedType);
            yield return new TestCaseData(null, null, ComparisonResult.Equal)
                .SetName($"{rootName} (null, null)");
            yield return new TestCaseData(new UdtClass(), null, ComparisonResult.GreaterThan)
                .SetName($"{rootName} ({nameof(UdtClass)}, null)");
            yield return new TestCaseData(null, new UdtClass(), ComparisonResult.LessThan)
                .SetName($"{rootName} (null, {nameof(UdtClass)})");
            yield return new TestCaseData(new UdtClass(), new UdtClass(), ComparisonResult.NotEqual | ComparisonResult.NoComparer)
                .SetName($"{rootName} ({nameof(UdtClass)}, {nameof(UdtClass)})");
        }

        private static IEnumerable ByValUserDefinedTypeTestData()
        {
            string rootName = nameof(ByValUserDefinedType);
            yield return new TestCaseData(
                new UdtStruct(),
                new UdtStruct(),
                ComparisonResult.Equal | ComparisonResult.NoComparer
            ).SetName($"{rootName} (0, 0)");
            yield return new TestCaseData(
                new UdtStruct()
                {
                    CustomProperty = 1
                },
                new UdtStruct(),
                ComparisonResult.NotEqual | ComparisonResult.NoComparer
            ).SetName($"{rootName} (1, 0)");
            yield return new TestCaseData(
                new UdtStruct(),
                new UdtStruct()
                {
                    CustomProperty = 1
                },
                ComparisonResult.NotEqual | ComparisonResult.NoComparer
            ).SetName($"{rootName} (0, 1)");
        }
        #endregion

        #region Tests
        [Test]
        [Description("Makes sure that identical property values in separate objects yield all-equal results.")]
        public void CompareEqualObjects()
        {
            //Arrange.
            Person p1 = ModelsHelper.CreatePerson();
            Person p2 = ModelsHelper.CreatePerson();
            ObjectComparer comparer = ObjectComparer.Create<Person>();

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            isDifferent.Should().BeFalse();
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            foreach (PropertyComparisonResult pcr in result.Values)
            {
                pcr.Result.Should().Be(ComparisonResult.Equal);
            }
        }

        [Test]
        [Description("Makes sure that an exception is thrown if an object is compared against itself.")]
        public void CompareSameObject()
        {
            //Arrange.
            Person p1 = ModelsHelper.CreatePerson();
            Person p2 = p1;
            ObjectComparer comparer = ObjectComparer.Create<Person>();

            //Act.
            Action act = () => comparer.Compare(p1, p2);

            //Assert.
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        [Description("Makes sure an exception is thrown if one of the objects is null.")]
        [TestCaseSource(nameof(CompareWithNullTestData))]
        public void CompareWithNull(object o1, object o2, string paramName)
        {
            //Arrange.
            ObjectComparer comparer = ObjectComparer.Create<Person>();

            //Act.
            Action act = () => comparer.Compare(o1, o2);
            ArgumentNullException ex = null;
            try
            {
                act();
            }
            catch(ArgumentNullException aex)
            {
                ex = aex;
            }

            //Assert.
            act.Should().Throw<ArgumentNullException>();
            ex.Should().NotBeNull();
            ex.ParamName.Should().Be(paramName);
        }

        [Test]
        [Description("Makes sure an exception is thrown if one of the objects is not of the expected type.")]
        [TestCaseSource(nameof(CompareTypeMismatchTestData))]
        public void CompareTypeMismatch(object o1, object o2)
        {
            //Arrange.
            ObjectComparer comparer = ObjectComparer.Create<Person, PersonByRefUdt>();

            //Act.
            Action act = () => comparer.Compare(o1, o2);

            //Assert.
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [Description("Makes sure that comparers provided are used.")]
        public void CompareWithCustomComparer()
        {
            //Arrange.
            Mock<IComparer> customComparer = new Mock<IComparer>();
            customComparer.Setup(m => m.Compare(It.IsAny<Genders>(), It.IsAny<Genders>()))
                .Returns(0)
                .Callback<Genders, Genders>((d1, d2) =>
                {
                    if (d1.GetType() != typeof(Genders) || d2.GetType() != typeof(Genders))
                    {
                        throw new AssertionException("The arguments of the custom comparer are not of the expected type.");
                    }
                })
                .Verifiable("Custom comparer was not invoked.");
            Person p1 = ModelsHelper.CreatePerson();
            Person p2 = ModelsHelper.CreatePerson();
            ObjectComparer comparer = ObjectComparer.Create<Person>();
            comparer.Comparers.Add(typeof(Genders), customComparer.Object);

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            customComparer.VerifyAll();
            isDifferent.Should().BeFalse();
        }

        [Test]
        [Description("Makes sure an exception is thrown if a comparer for an unregistered type is attempted.")]
        [TestCaseSource(nameof(ObjectComparerForUnregisteredTypeTestData))]
        public void ObjectComparerForUnregisteredType(Action act)
        {
            //Assert.
            act.Should().Throw<NoTypeInformationException>("An unregistered data type was attempted.");
        }

        [Test]
        [Description("Makes sure values from IComparable.Compare translate to correct results.")]
        [TestCaseSource(nameof(IComparerResultTestData))]
        public void IComparerResult(int comparerReturnValue, ComparisonResult expectedResult)
        {
            //Arrange.
            Mock<IComparer> customComparer = new Mock<IComparer>();
            customComparer.Setup(m => m.Compare(It.IsAny<Genders>(), It.IsAny<Genders>()))
                .Returns(comparerReturnValue)
                .Verifiable("Custom comparer was not invoked.");
            Person p1 = ModelsHelper.CreatePerson();
            Person p2 = ModelsHelper.CreatePerson();
            ObjectComparer comparer = ObjectComparer.Create<Person>();
            comparer.Comparers.Add(typeof(Genders), customComparer.Object);

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            customComparer.VerifyAll();
            isDifferent.Should().Be(expectedResult != ComparisonResult.Equal);
            result[nameof(Person.Gender)].Result.Should().Be(expectedResult);
        }

        [Test]
        [Description("Makes sure properties in the first object that are not in the second object are " +
            "marked appropriately.")]
        public void PropertyNotFound()
        {
            //Arrange.
            PersonEx p1 = ModelsHelper.CreatePersonEx();
            Person p2 = ModelsHelper.CreatePerson();
            ObjectComparer comparer = ObjectComparer.Create<PersonEx, Person>();

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            isDifferent.Should().BeFalse();
            result[nameof(PersonEx.NickName)].Result.Should().Be(ComparisonResult.PropertyNotFound);
        }

        [Test]
        [Description("Makes sure that class property values that do not implement IComparable " +
            "are compared as expected.")]
        [TestCaseSource(nameof(ByRefUserDefinedTypeTestData))]
        public void ByRefUserDefinedType(UdtClass value1, UdtClass value2, ComparisonResult expectedResult)
        {
            //Arrange.
            PersonByRefUdt p1 = ModelsHelper.CreatePersonByRefUdt();
            p1.ByRefProperty = value1;
            PersonByRefUdt p2 = ModelsHelper.CreatePersonByRefUdt();
            p2.ByRefProperty = value2;
            ObjectComparer comparer = ObjectComparer.Create<PersonByRefUdt>();

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            isDifferent.Should().Be((expectedResult & ComparisonResult.NotEqual) == ComparisonResult.NotEqual);
            result[nameof(PersonByRefUdt.ByRefProperty)].Result.Should().Be(expectedResult);
        }

        [Test]
        [Description("Makes sure that struct property values that do not implement IComparable " +
            "are property marked as such.")]
        [TestCaseSource(nameof(ByValUserDefinedTypeTestData))]
        public void ByValUserDefinedType(UdtStruct value1, UdtStruct value2, ComparisonResult expectedResult)
        {
            //Arrange.
            PersonByValUdt p1 = ModelsHelper.CreatePersonByValUdt();
            p1.ByValProperty = value1;
            PersonByValUdt p2 = ModelsHelper.CreatePersonByValUdt();
            p2.ByValProperty = value2;
            ObjectComparer comparer = ObjectComparer.Create<PersonByValUdt>();

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            isDifferent.Should().Be((expectedResult & ComparisonResult.NotEqual) == ComparisonResult.NotEqual);
            result[nameof(PersonByValUdt.ByValProperty)].Result.Should().Be(expectedResult);
        }

        [Test]
        [Description("Makes sure exceptions are caught if the comparer used throws any.")]
        public void IComparerThrows()
        {
            //Arrange.
            Person p1 = ModelsHelper.CreatePerson();
            Person p2 = ModelsHelper.CreatePerson();
            Mock<IComparer> throwingComparer = new Mock<IComparer>();
            throwingComparer.Setup(m => m.Compare(It.IsAny<object>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("Compare throws."))
                .Verifiable($"{nameof(IComparer.Compare)}() method not invoked.");
            ObjectComparer comparer = ObjectComparer.Create<Person>();
            comparer.Comparers.Add(typeof(Genders), throwingComparer.Object);

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            throwingComparer.VerifyAll();
            PropertyComparisonResult r = result[nameof(Person.Gender)];
            r.Result.Should().Be(ComparisonResult.Exception);
            r.Exception.Should().NotBeNull();
            r.Exception.Should().BeOfType<InvalidOperationException>();
        }
        #endregion
    }
}