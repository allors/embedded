namespace Allors.Embedded.Tests.Domain
{
    using System;
    using System.Linq;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class DerivationOverrideTests
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
            meta.AddUnit<string>(person, "Greeting");

            var population = new EmbeddedPopulation
            {
                DerivationById =
                {
                    ["FullName"] = new FullNameDerivation(firstName, lastName),
                    ["Greeting"] = new GreetingDerivation(fullName),
                },
            };

            var john = population.Create(person);
            john["FirstName"] = "John";
            john["LastName"] = "Doe";

            population.Derive();

            Assert.Equal("Hello John Doe!", john["Greeting"]);
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

        private class GreetingDerivation(IEmbeddedRoleType fullName) : IEmbeddedDerivation
        {
            public void Derive(EmbeddedChangeSet changeSet)
            {
                var fullNames = changeSet.ChangedRoles(fullName);

                if (!fullNames.Any())
                {
                    return;
                }

                var people = fullNames.Select(v => v.Key).Distinct();

                foreach (EmbeddedObject person in people)
                {
                    person["Greeting"] = $"Hello {person["FullName"]}!";
                }
            }
        }
    }
}
