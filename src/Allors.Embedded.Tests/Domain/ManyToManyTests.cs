namespace Allors.Embedded.Tests.Domain
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Allors.Embedded.Meta.Diagrams;
    using Xunit;

    public class ManyToManyTests
    {
        [Fact]
        public void AddSingleActiveLink()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            var name = meta.AddUnit<string>(organization, "Name");
            var (employees, organizationWhereEmployee) = meta.AddManyToMany(organization, person, "Employee");

            var diagram = new ClassDiagram(meta).Render();

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization, v => v[name] = "Acme");
            var hooli = population.Create(organization, v => v[name] = "Hooli");

            var jane = population.Create(person);
            var john = population.Create(person);
            var jenny = population.Create(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            Assert.Single(jane[organizationWhereEmployee]);
            Assert.Contains(acme, jane[organizationWhereEmployee]);

            Assert.Single(john[organizationWhereEmployee]);
            Assert.Contains(acme, john[organizationWhereEmployee]);

            Assert.Single(jenny[organizationWhereEmployee]);
            Assert.Contains(acme, jenny[organizationWhereEmployee]);

            Assert.Equal(3, acme[employees].Count);
            Assert.Contains(jane, acme[employees]);
            Assert.Contains(john, acme[employees]);
            Assert.Contains(jenny, acme[employees]);

            Assert.Empty(hooli[employees]);
        }

        [Fact]
        public void SetSingleActiveLink()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            var name = meta.AddUnit<string>(organization, "Name");
            (EmbeddedManyToManyRoleType employees, _) = meta.AddManyToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization, v => v[name] = "Acme");
            var hooli = population.Create(organization, v => v[name] = "Hooli");

            var jane = population.Create(person);
            var john = population.Create(person);
            var jenny = population.Create(person);

            acme[employees] = new[] { jane }.ToFrozenSet();

            Assert.Single((IReadOnlySet<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IReadOnlySet<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            acme["Employees"] = new[] { jane, john };

            Assert.Single((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)acme["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            acme["Employees"] = new[] { jane, john, jenny };

            Assert.Single((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Equal(3, ((IEnumerable<IEmbeddedObject>)acme["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            acme["Employees"] = Array.Empty<IEmbeddedObject>();

            Assert.Empty((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Empty((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Empty((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
        }

        [Fact]
        public void RemoveSingleActiveLink()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddUnit<string>(organization, "Name");
            (EmbeddedManyToManyRoleType employees, _) = meta.AddManyToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization, v => v["Name"] = "Acme");
            var hooli = population.Create(organization, v => v["Name"] = "Hooli");

            var jane = population.Create(person);
            var john = population.Create(person);
            var jenny = population.Create(person);

            acme["Employees"] = new[] { jane, john, jenny };

            acme.Remove(employees, jenny);

            Assert.Single((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)acme["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            acme.Remove(employees, john);

            Assert.Single((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            acme.Remove(employees, jane);

            Assert.Empty((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Empty((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Empty((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Empty((IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Empty((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
        }

        [Fact]
        public void MultipleActiveLinks()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddUnit<string>(organization, "Name");
            (EmbeddedManyToManyRoleType employees, _) = meta.AddManyToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization, v => v["Name"] = "Acme");
            var hooli = population.Create(organization, v => v["Name"] = "Hooli");

            var jane = population.Create(person);
            var john = population.Create(person);
            var jenny = population.Create(person);

            acme.Add(employees, jane);
            acme.Add(employees, john);
            acme.Add(employees, jenny);

            hooli.Add(employees, jane);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!).Count());
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(hooli, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Equal(3, ((IEnumerable<IEmbeddedObject>)acme["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            hooli.Add(employees, john);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!).Count());
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(hooli, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!).Count());
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Contains(hooli, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Equal(3, ((IEnumerable<IEmbeddedObject>)acme["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)hooli["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);

            hooli.Add(employees, jenny);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!).Count());
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);
            Assert.Contains(hooli, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWhereEmployee"]!);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!).Count());
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);
            Assert.Contains(hooli, (IEnumerable<IEmbeddedObject>)john["OrganizationsWhereEmployee"]!);

            Assert.Equal(2, ((IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!).Count());
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);
            Assert.Contains(hooli, (IEnumerable<IEmbeddedObject>)jenny["OrganizationsWhereEmployee"]!);

            Assert.Equal(3, ((IEnumerable<IEmbeddedObject>)acme["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)acme["Employees"]!);

            Assert.Equal(3, ((IEnumerable<IEmbeddedObject>)hooli["Employees"]!).Count());
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
            Assert.Contains(john, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
            Assert.Contains(jenny, (IEnumerable<IEmbeddedObject>)hooli["Employees"]!);
        }

        [Fact]
        public void DefaultRoleName()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddUnit<string>(organization, "Name");
            (EmbeddedManyToManyRoleType people, _) = meta.AddManyToMany(organization, person);

            var population = new EmbeddedPopulation(meta);

            var acme = population.Create(organization, v => v["Name"] = "Acme");

            var jane = population.Create(person);

            acme.Add(people, jane);

            Assert.Single((IEnumerable<IEmbeddedObject>)jane["OrganizationsWherePerson"]!);
            Assert.Contains(acme, (IEnumerable<IEmbeddedObject>)jane["OrganizationsWherePerson"]!);

            Assert.Single((IEnumerable<IEmbeddedObject>)acme["Persons"]!);
            Assert.Contains(jane, (IEnumerable<IEmbeddedObject>)acme["Persons"]!);
        }
    }
}
