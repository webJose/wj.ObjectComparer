using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using wj.ObjectComparer.TestModels;

namespace wj.ObjectComparer.Tests
{
    [TestFixture(Category = "ComparerConfiguration")]
    public class ComparerConfigurationTests
    {
        #region Tests
        [Test]
        [Description("Makes sure an object comparer configured works without registering its type.")]
        public void ConfigurationWorksWithNoTypeRegistration()
        {
            //Arrange.
            var config = ComparerConfigurator.Configure<Person>();

            //Act.
            Action act = () => config.CreateComparer();

            //Assert.
            act.Should().NotThrow<NoTypeInformationException>();
        }

        [Test]
        [Description("Makes sure attributed property mappings are included by default.")]
        public void ConfigurationIncludesAttributedPropertyMappings()
        {
            //Arrange.
            PersonExWithPropMap p1 = ModelsHelper.CreatePersonExWithPropMapping();
            PersonEx p2 = ModelsHelper.CreatePersonEx();
            var config = ComparerConfigurator.Configure<PersonExWithPropMap, PersonEx>();
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result[nameof(PersonExWithPropMap.NewNickName)].MapUsed.Should().NotBeNull();
        }

        [Test]
        [Description("Makes sure attribute property mappings are not included when requested.")]
        public void ConfigurationDoesNotIncludeAttributedPropertyPamppings()
        {
            //Arrange.
            PersonExWithPropMap p1 = ModelsHelper.CreatePersonExWithPropMapping();
            PersonEx p2 = ModelsHelper.CreatePersonEx();
            var config = ComparerConfigurator.Configure<PersonExWithPropMap, PersonEx>(true);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            result[nameof(PersonExWithPropMap.NewNickName)].MapUsed.Should().BeNull();
        }

        [Test]
        [Description("Makes sure property mappings added via configuration are used.")]
        public void CompareUsesMappingsFromConfiguration()
        {
            //Arrange.
            PersonEx p1 = ModelsHelper.CreatePersonEx();
            PersonExWithPropMap p2 = ModelsHelper.CreatePersonExWithPropMapping();
            var config = ComparerConfigurator.Configure<PersonEx, PersonExWithPropMap>(true)
                .MapProperty(src => src.NickName, dst => dst.NewNickName);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            PropertyMap map = result[nameof(PersonEx.NickName)].MapUsed;
            map.Should().NotBeNull();
            map.TargetType.Should().Be(typeof(PersonExWithPropMap));
            map.TargetProperty.Should().Be(nameof(PersonExWithPropMap.NewNickName));
        }

        [Test]
        [Description("Makes sure comparers added via configuration are used.")]
        public void CompareUsesComparersFromConfiguration()
        {
            //Arrange.
            PersonEx p1 = ModelsHelper.CreatePersonEx();
            PersonExWithPropMap p2 = ModelsHelper.CreatePersonExWithPropMapping();
            Mock<IComparer> customComparer = new Mock<IComparer>();
            customComparer.Setup(m => m.Compare(It.IsAny<Genders>(), It.IsAny<object>()))
                .Returns(-1)
                .Verifiable($"{nameof(IComparer.Compare)}() method was not invoked.");
            var config = ComparerConfigurator.Configure<PersonEx, PersonExWithPropMap>(true)
                .MapProperty(src => src.NickName, dst => dst.NewNickName)
                .AddComparer<Genders>(customComparer.Object);
            ;
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            customComparer.VerifyAll();
        }

        [Test]
        [Description("Makes sure string coercion happens on demand via configuration.")]
        public void CompareCoercesToStringOnDemandFromConfiguration()
        {
            //Arrange.
            PersonEx p1 = ModelsHelper.CreatePersonEx();
            PersonExWithPropMap p2 = ModelsHelper.CreatePersonExWithPropMapping();
            var config = ComparerConfigurator.Configure<PersonEx, PersonExWithPropMap>(true)
                .MapProperty(src => src.Gender, dst => dst.Gender, true);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            PropertyComparisonResult pcr = result[nameof(Person.Gender)];
            pcr.Should().NotBeNull();
            (pcr.Result & ComparisonResult.StringCoercion).Should().Be(ComparisonResult.StringCoercion);
            pcr.MapUsed.Should().NotBeNull();
            pcr.MapUsed.ForceStringValue.Should().BeTrue();
            pcr.Value1.Should().NotBeNull();
            pcr.Value1.Should().BeOfType<string>();
            pcr.Value2.Should().NotBeNull();
            pcr.Value2.Should().BeOfType<string>();
        }

        [Test]
        [Description("Makes sure format strings given via configuration are used.")]
        public void CompareUsesFormatStringsFromConfiguration()
        {
            //Arrange.
            PersonEx2 p1 = ModelsHelper.CreatePersonEx2();
            PersonEx2 p2 = ModelsHelper.CreatePersonEx2();
            DateTime birthDate = DateTime.Now.AddYears(-20);
            p1.BirthDate = p2.BirthDate = birthDate;
            string fs1 = "yyyyMMdd";
            string fs2 = "ddMMyyyy";
            var config = ComparerConfigurator.Configure<PersonEx2>(true)
                .MapProperty(src => src.BirthDate, dst => dst.BirthDate, true, fs1, fs2);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            PropertyComparisonResult pcr = result[nameof(PersonEx2.BirthDate)];
            pcr.Should().NotBeNull();
            (pcr.Result & ComparisonResult.StringCoercion).Should().Be(ComparisonResult.StringCoercion);
            (pcr.Result & ComparisonResult.NotEqual).Should().Be(ComparisonResult.NotEqual);
            pcr.MapUsed.Should().NotBeNull();
            pcr.MapUsed.ForceStringValue.Should().BeTrue();
            pcr.MapUsed.FormatString.Should().Be(fs1);
            pcr.MapUsed.TargetFormatString.Should().Be(fs2);
            pcr.Value1.Should().NotBeNull();
            pcr.Value1.Should().BeOfType<string>();
            pcr.Value1.Should().Be(birthDate.ToString(fs1));
            pcr.Value2.Should().NotBeNull();
            pcr.Value2.Should().BeOfType<string>();
            pcr.Value2.Should().Be(birthDate.ToString(fs2));
        }

        [Test]
        [Description("Makes sure ignored properties are ignored when ignored for a specific type.")]
        public void PropertyIsIgnoredForSpecificType()
        {
            //Arrange.
            Person p1 = ModelsHelper.CreatePerson();
            PersonEx p2 = ModelsHelper.CreatePersonEx();
            var config = ComparerConfigurator.Configure<Person, PersonEx>()
                .IgnoreProperty(src => src.Email);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            PropertyComparisonResult pcr = result[nameof(Person.Email)];
            pcr.Should().NotBeNull();
            pcr.Result.Should().Be(ComparisonResult.PropertyIgnored);
            pcr.MapUsed.Should().NotBeNull();
            pcr.MapUsed.Operation.Should().Be(PropertyMapOperation.IgnoreProperty);
            pcr.Property1.IgnoreProperty.Should().Be(IgnorePropertyOptions.DoNotIgnore);
        }

        [Test]
        [Description("Makes sure an ignored property does not affect the overall isDifferent Boolean result.")]
        public void IgnoredPropertyDoesNotResultInDifferent()
        {
            //Arrange.
            Person p1 = ModelsHelper.CreatePerson();
            Person p2 = ModelsHelper.CreatePerson();
            p2.Id = p1.Id + 1;
            var config = ComparerConfigurator.Configure<Person>()
                .IgnoreProperty(src => src.Id);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool isDifferent);

            //Assert.
            result.Should().NotBeNull();
            isDifferent.Should().BeFalse();
            result[nameof(Person.Id)].Result.Should().Be(ComparisonResult.PropertyIgnored);
        }
        #endregion
    }
}
