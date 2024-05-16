namespace Allors.Embedded.Tests.Domain
{
    using System;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class SnapshotTests
    {
        [Fact]
        public void Unit()
        {
            var meta = new EmbeddedMeta();
            var person = meta.AddClass("Person");
            var firstName = meta.AddUnit<string>(person, "FirstName");
            var lastName = meta.AddUnit<string>(person, "LastName");

            var population = new EmbeddedPopulation(meta);

            var john = population.Build(person);
            var jane = population.Build(person);

            john["FirstName"] = "John";
            john["LastName"] = "Doe";

            var snapshot1 = population.Checkpoint();

            jane["FirstName"] = "Jane";
            jane["LastName"] = "Doe";

            var changedFirstNames = snapshot1.ChangedRoles(firstName);
            var changedLastNames = snapshot1.ChangedRoles(lastName);

            Assert.Single(changedFirstNames.Keys);
            Assert.Single(changedLastNames.Keys);
            Assert.Contains(john, changedFirstNames.Keys);
            Assert.Contains(john, changedLastNames.Keys);

            var snapshot2 = population.Checkpoint();

            changedFirstNames = snapshot2.ChangedRoles(firstName);
            changedLastNames = snapshot2.ChangedRoles(lastName);

            Assert.Single(changedFirstNames.Keys);
            Assert.Single(changedLastNames.Keys);
            Assert.Contains(jane, changedFirstNames.Keys);
            Assert.Contains(jane, changedLastNames.Keys);
        }

        [Fact]
        public void Composites()
        {
            var meta = new EmbeddedMeta();
            var person = meta.AddClass("Person");
            var organization = meta.AddClass("Organization");
            meta.AddUnit<string>(person, "FirstName");
            meta.AddUnit<string>(person, "LastName");
            meta.AddUnit<string>(organization, "Name");
            EmbeddedManyToManyRoleType employees = meta.AddManyToMany(organization, person, "Employee");

            var population = new EmbeddedPopulation(meta);

            var john = population.Build(person);
            var jane = population.Build(person);

            john["FirstName"] = "John";
            john["LastName"] = "Doe";

            jane["FirstName"] = "Jane";
            jane["LastName"] = "Doe";

            var acme = population.Build(organization);

            acme["Name"] = "Acme";

            acme["Employees"] = new[] { john, jane };

            var snapshot = population.Checkpoint();
            var changedEmployees = snapshot.ChangedRoles(employees);
            Assert.Single(changedEmployees);

            acme["Employees"] = new[] { jane, john };

            snapshot = population.Checkpoint();
            changedEmployees = snapshot.ChangedRoles(employees);
            Assert.Empty(changedEmployees);

            acme["Employees"] = Array.Empty<IEmbeddedObject>();

            acme["Employees"] = new[] { jane, john };

            snapshot = population.Checkpoint();
            changedEmployees = snapshot.ChangedRoles(employees);
            Assert.Empty(changedEmployees);
        }
    }
}
