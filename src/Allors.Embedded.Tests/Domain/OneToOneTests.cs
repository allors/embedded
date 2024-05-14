namespace Allors.Embedded.Tests.Domain
{
    using System;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class OneToOneTests
    {
        [Fact]
        public void StaticPropertySet()
        {
            var meta = new EmbeddedMeta();
            var named = meta.AddInterface("Named");
            var organization = meta.AddClass("Organization", named);
            var person = meta.AddClass("Person", named);
            meta.AddOneToOne(organization, person, "Owner");
            meta.AddOneToOne(organization, named, "Named");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization);
            var gizmo = population.Create(organization);

            var jane = population.Create(person);
            var john = population.Create(person);

            acme["Owner"] = jane;

            Assert.Equal(jane, acme["Owner"]);
            Assert.Equal(acme, jane["OrganizationWhereOwner"]);

            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);

            acme["Named"] = jane;

            Assert.Equal(jane, acme["Named"]);
            Assert.Equal(acme, jane["OrganizationWhereNamed"]);

            Assert.Null(gizmo["Named"]);
            Assert.Null(john["OrganizationWhereNamed"]);
        }

        [Fact]
        public void EmbeddedPropertySet()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            var (owner, property) = meta.AddOneToOne(organization, person, "Owner");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization);
            var gizmo = population.Create(organization);

            var jane = population.Create(person);
            var john = population.Create(person);

            acme["Owner"] = jane;

            Assert.Equal(jane, acme["Owner"]);
            Assert.Equal(acme, jane["OrganizationWhereOwner"]);
            Assert.Equal(jane, acme[owner]);
            Assert.Equal(acme, jane[property]);

            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);
            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);
            Assert.Null(gizmo[owner]);
            Assert.Null(john[property]);

            // Wrong Type
            Assert.Throws<ArgumentException>(() =>
            {
                acme["Owner"] = gizmo;
            });
        }

        [Fact]
        public void IndexByNameSet()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            var (owner, property) = meta.AddOneToOne(organization, person, "Owner");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization);
            var gizmo = population.Create(organization);

            var jane = population.Create(person);
            var john = population.Create(person);

            acme["Owner"] = jane;

            Assert.Equal(jane, acme["Owner"]);
            Assert.Equal(acme, jane["OrganizationWhereOwner"]);
            Assert.Equal(jane, acme["Owner"]);
            Assert.Equal(acme, jane["OrganizationWhereOwner"]);
            Assert.Equal(jane, acme[owner]);
            Assert.Equal(acme, jane[property]);

            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);
            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);
            Assert.Null(gizmo[owner]);
            Assert.Null(john[property]);

            // Wrong Type
            Assert.Throws<ArgumentException>(() =>
            {
                acme["Owner"] = gizmo;
            });
        }

        [Fact]
        public void IndexByRoleSet()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            var (owner, property) = meta.AddOneToOne(organization, person, "Owner");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization);
            var gizmo = population.Create(organization);

            var jane = population.Create(person);
            var john = population.Create(person);

            acme[owner] = jane;

            Assert.Equal(jane, acme["Owner"]);
            Assert.Equal(acme, jane["OrganizationWhereOwner"]);
            Assert.Equal(jane, acme["Owner"]);
            Assert.Equal(acme, jane["OrganizationWhereOwner"]);
            Assert.Equal(jane, acme[owner]);
            Assert.Equal(acme, jane[property]);

            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);
            Assert.Null(gizmo["Owner"]);
            Assert.Null(john["OrganizationWhereOwner"]);
            Assert.Null(gizmo[owner]);
            Assert.Null(john[property]);

            // Wrong Type
            Assert.Throws<ArgumentException>(() =>
            {
                acme[owner] = gizmo;
            });
        }

        [Fact]
        public void DefaultRoleName()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddOneToOne(organization, person);

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization);

            var jane = population.Create(person);

            acme["Person"] = jane;

            Assert.Equal(jane, acme["Person"]);
            Assert.Equal(acme, jane["OrganizationWherePerson"]);
        }
    }
}
