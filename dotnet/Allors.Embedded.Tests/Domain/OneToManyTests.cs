namespace Allors.Embedded.Tests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class OneToManyTests
    {
        [Fact]
        public void AddSameAssociation()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Equal(acme, jane["OrganizationWhereEmployee"]);
            Assert.Equal(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void AddSameAssociationParams()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane, john, jenny);

            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Equal(acme, jane["OrganizationWhereEmployee"]);
            Assert.Equal(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void AddSameAssociationArray()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, [jane, john, jenny]);

            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Equal(acme, jane["OrganizationWhereEmployee"]);
            Assert.Equal(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void AddDifferentAssociation()
        {
            var meta = new EmbeddedMeta();
            var named = meta.AddInterface("Named");
            var organization = meta.AddClass("Organization", named);
            var person = meta.AddClass("Person", named);
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);

            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            var hooli = population.Build(organization);

            hooli.Add(employees, jane);

            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Equal(hooli, jane["OrganizationWhereEmployee"]);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.Equal(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void Remove()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            acme.Remove(employees, jane);

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.Equal(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);

            acme.Remove(employees, john);

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);

            acme.Remove(employees, jenny);

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void RemoveParams()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            acme.Remove(employees, jane, john);

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void RemoveArray()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            acme.Remove(employees, [jane, john]);

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.Equal(acme, jenny["OrganizationWhereEmployee"]);
        }

        [Fact]
        public void RemoveAll()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            EmbeddedOneToManyRoleType employees = meta.AddOneToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Build(organization);
            var jane = population.Build(person);
            var john = population.Build(person);
            var jenny = population.Build(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            acme["Employees"] = null;

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, jenny["OrganizationWhereEmployee"]);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            acme["Employees"] = Array.Empty<IEmbeddedObject>();

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, jenny["OrganizationWhereEmployee"]);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            acme["Employees"] = ImmutableHashSet<IEmbeddedObject>.Empty;

            Assert.DoesNotContain(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.DoesNotContain(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.NotEqual(acme, jane["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, john["OrganizationWhereEmployee"]);
            Assert.NotEqual(acme, jenny["OrganizationWhereEmployee"]);
        }
    }
}
