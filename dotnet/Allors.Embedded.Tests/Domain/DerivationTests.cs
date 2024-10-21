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
            var fullName = meta.AddUnit<string>(person, "FullName");
            meta.AddUnit<DateTime>(person, "DerivedAt");

            var population = new EmbeddedPopulation(meta)
            {
                DerivationById =
                {
                    ["FullName"] = new FullNameDerivation(firstName, lastName),
                },
            };

            var john = population.Build(person);
            john[firstName] = "John";
            john[lastName] = "Doe";

            population.Derive();

            Assert.Equal("John Doe", john[fullName]);

            population.DerivationById["FullName"] = new GreetingDerivation(population.DerivationById["FullName"], firstName, lastName);

            var jane = population.Build(person);
            jane[firstName] = "Jane";
            jane[lastName] = "Doe";

            population.Derive();

            Assert.Equal("Jane Doe Chained", jane[fullName]);
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
