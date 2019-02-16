using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using wj.ObjectComparer.TestModels;

namespace wj.ObjectComparer.Tests
{
    [TestFixture(Category = "TypeConfiguration")]
    public class TypeConfigurationTests
    {
        #region Setups
        [TearDown]
        public void CleanScanner()
        {
            Scanner.UnregisterType(typeof(Person));
            Scanner.UnregisterType(typeof(PersonEx));
        }
        #endregion

        #region Tests
        [Test]
        [Description("Makes sure that starting type configuration on an unregistered type does not" +
            "throw and updates the scanner's cache.")]
        public void TargetTypeConfigurationRegistersUnregisteredType()
        {
            //Arrange.
            bool beforeConfigTypeExists = Scanner.TryGetTypeInformation(typeof(Person), out TypeInfo ti);

            //Act.
            var config = Scanner.ConfigureType<Person>();

            //Assert.
            bool afterConfigTypeExists = Scanner.TryGetTypeInformation(typeof(Person), out ti);
            beforeConfigTypeExists.Should().BeFalse();
            afterConfigTypeExists.Should().BeTrue();
            ti.Should().NotBeNull();
        }

        [Test]
        [Description("Makes sure that type configuration can add property maps.")]
        public void TypeConfigurationAddsPropertyMaps()
        {
            //Arrange.
            var config = Scanner.ConfigureType<Person>();
            var personConfig = config.ForType<PersonEx>();

            //Act.
            personConfig.MapProperty(src => src.Id, dst => dst.Id)
                .MapProperty(src => src.LastName, dst => dst.LastName);

            //Assert.
            bool exists = Scanner.TryGetTypeInformation(typeof(Person), out TypeInfo ti);
            exists.Should().BeTrue();
            ti.Properties[nameof(Person.Id)].Maps.Count.Should().Be(1);
            ti.Properties[nameof(Person.LastName)].Maps.Count.Should().Be(1);
        }

        [Test]
        [Description("Makes sure that type configuration can ignore a property.")]
        public void TypeConfigurationCanSetPropertyToIgnore()
        {
            //Arrange.
            var config = Scanner.ConfigureType<Person>();
            Scanner.TryGetTypeInformation(typeof(Person), out TypeInfo ti);
            IgnorePropertyOptions beforeIgnore = ti.Properties[nameof(Person.Email)].IgnoreProperty;

            //Act.
            config.IgnoreProperty(src => src.Email, IgnorePropertyOptions.IgnoreForAll);

            //Assert.
            beforeIgnore.Should().Be(IgnorePropertyOptions.DoNotIgnore);
            ti.Properties[nameof(Person.Email)].IgnoreProperty.Should().Be(IgnorePropertyOptions.IgnoreForAll);
        }

        [Test]
        [Description("Makes sure type configuration can ignore a property for a specific type.")]
        public void TargetTypeConfigurationCanIgnoreProperty()
        {
            //Arrange.
            var config = Scanner.ConfigureType<PersonEx>().ForType<Person>();
            Scanner.TryGetTypeInformation(typeof(PersonEx), out TypeInfo ti);
            PropertyMap personMap = null;
            try
            {
                personMap = ti.Properties[nameof(PersonEx.NickName)].Maps[typeof(Person)];
            }
            catch (KeyNotFoundException)
            { }

            //Act.
            config.IgnoreProperty(src => src.NickName);

            //Assert.
            personMap.Should().BeNull();
            personMap = ti.Properties[nameof(PersonEx.NickName)].Maps[typeof(Person)];
            personMap.Should().NotBeNull();
            personMap.Operation.Should().Be(PropertyMapOperation.IgnoreProperty);
        }
        #endregion
    }
}
