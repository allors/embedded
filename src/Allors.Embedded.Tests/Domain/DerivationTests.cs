namespace Allors.Embedded.Tests.Domain
{
    using System;
    using System.Linq;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class DerivationTests
    {
        [Fact]
        public void Derivation()
        {
            var meta = new EmbeddedMeta();
            var person = meta.AddClass("Person");
            var firstName = meta.AddUnit<string>(person, "FirstName");
            var lastName = meta.AddUnit<string>(person, "LastName");
            meta.AddUnit<string>(person, "FullName");
            meta.AddUnit<DateTime>(person, "DerivedAt");

            var population = new EmbeddedPopulation
            {
                DerivationById =
                {
                    ["FullName"] = new FullNameDerivation(firstName, lastName),
                },
            };

            var john = population.Create(person);
            john["FirstName"] = "John";
            john["LastName"] = "Doe";

            population.Derive();

            Assert.Equal("John Doe", john["FullName"]);

            population.DerivationById["FullName"] = new GreetingDerivation(population.DerivationById["FullName"], firstName, lastName);

            var jane = population.Create(person);
            jane["FirstName"] = "Jane";
            jane["LastName"] = "Doe";

            population.Derive();

            Assert.Equal("Jane Doe Chained", jane["FullName"]);
        }

        private class FullNameDerivation(IEmbeddedRoleType firstName, IEmbeddedRoleType lastName) : IEmbeddedDerivation
        {
            public void Derive(EmbeddedChangeSet changeSet)
            {
                var firstNames = changeSet.ChangedRoles(firstName);
                var lastNames = changeSet.ChangedRoles(lastName);

                if (!firstNames.Any() && !lastNames.Any())
                {
                    return;
                }

                var people = firstNames.Union(lastNames).Select(v => v.Key).Distinct();

                foreach (EmbeddedObject person in people)
                {
                    // Dummy updates ...
                    person["FirstName"] = person["FirstName"];
                    person["LastName"] = person["LastName"];

                    person["DerivedAt"] = DateTime.Now;

                    person["FullName"] = $"{person["FirstName"]} {person["LastName"]}";
                }
            }
        }

        private class GreetingDerivation(IEmbeddedDerivation derivation, IEmbeddedRoleType firstName, IEmbeddedRoleType lastName) : IEmbeddedDerivation
        {
            public void Derive(EmbeddedChangeSet changeSet)
            {
                derivation.Derive(changeSet);

                var firstNames = changeSet.ChangedRoles(firstName);
                var lastNames = changeSet.ChangedRoles(lastName);

                if (!firstNames.Any() && !lastNames.Any())
                {
                    return;
                }

                var people = firstNames.Union(lastNames).Select(v => v.Key).Distinct();

                foreach (EmbeddedObject person in people)
                {
                    person["FullName"] = $"{person["FullName"]} Chained";
                }
            }
        }
    }
}
