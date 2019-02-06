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
            PersonExWithPropMapping p1 = ModelsHelper.CreatePersonExWithPropMapping();
            PersonEx p2 = ModelsHelper.CreatePersonEx();
            var config = ComparerConfigurator.Configure<PersonExWithPropMapping, PersonEx>();
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result[nameof(PersonExWithPropMapping.NewNickName)].MappingUsed.Should().NotBeNull();
        }

        [Test]
        [Description("Makes sure attribute property mappings are not included when requested.")]
        public void ConfigurationDoesNotIncludeAttributedPropertyPamppings()
        {
            //Arrange.
            PersonExWithPropMapping p1 = ModelsHelper.CreatePersonExWithPropMapping();
            PersonEx p2 = ModelsHelper.CreatePersonEx();
            var config = ComparerConfigurator.Configure<PersonExWithPropMapping, PersonEx>(true);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            result[nameof(PersonExWithPropMapping.NewNickName)].MappingUsed.Should().BeNull();
        }

        [Test]
        [Description("Makes sure property mappings added via configuration are used.")]
        public void CompareUsesMappingsFromConfiguration()
        {
            //Arrange.
            PersonEx p1 = ModelsHelper.CreatePersonEx();
            PersonExWithPropMapping p2 = ModelsHelper.CreatePersonExWithPropMapping();
            var config = ComparerConfigurator.Configure<PersonEx, PersonExWithPropMapping>(true)
                .MapProperty(src => src.NickName, dst => dst.NewNickName);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            PropertyMapping map = result[nameof(PersonEx.NickName)].MappingUsed;
            map.Should().NotBeNull();
            map.TargetType.Should().Be(typeof(PersonExWithPropMapping));
            map.TargetProperty.Should().Be(nameof(PersonExWithPropMapping.NewNickName));
        }

        [Test]
        [Description("Makes sure comparers added via configuration are used.")]
        public void CompareUsesComparersFromConfiguration()
        {
            //Arrange.
            PersonEx p1 = ModelsHelper.CreatePersonEx();
            PersonExWithPropMapping p2 = ModelsHelper.CreatePersonExWithPropMapping();
            Mock<IComparer> customComparer = new Mock<IComparer>();
            customComparer.Setup(m => m.Compare(It.IsAny<Genders>(), It.IsAny<object>()))
                .Returns(-1)
                .Verifiable($"{nameof(IComparer.Compare)}() method was not invoked.");
            var config = ComparerConfigurator.Configure<PersonEx, PersonExWithPropMapping>(true)
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
            PersonExWithPropMapping p2 = ModelsHelper.CreatePersonExWithPropMapping();
            var config = ComparerConfigurator.Configure<PersonEx, PersonExWithPropMapping>(true)
                .MapProperty(src => src.Gender, dst => dst.Gender, true);
            ObjectComparer comparer = config.CreateComparer();

            //Act.
            var result = comparer.Compare(p1, p2, out bool _);

            //Assert.
            result.Should().NotBeNull();
            PropertyComparisonResult pcr = result[nameof(Person.Gender)];
            pcr.Should().NotBeNull();
            (pcr.Result & ComparisonResult.StringCoercion).Should().Be(ComparisonResult.StringCoercion);
            pcr.MappingUsed.Should().NotBeNull();
            pcr.MappingUsed.ForceStringValue.Should().BeTrue();
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
            pcr.MappingUsed.Should().NotBeNull();
            pcr.MappingUsed.ForceStringValue.Should().BeTrue();
            pcr.MappingUsed.FormatString.Should().Be(fs1);
            pcr.MappingUsed.TargetFormatString.Should().Be(fs2);
            pcr.Value1.Should().NotBeNull();
            pcr.Value1.Should().BeOfType<string>();
            pcr.Value1.Should().Be(birthDate.ToString(fs1));
            pcr.Value2.Should().NotBeNull();
            pcr.Value2.Should().BeOfType<string>();
            pcr.Value2.Should().Be(birthDate.ToString(fs2));
        }
        #endregion
    }
}
