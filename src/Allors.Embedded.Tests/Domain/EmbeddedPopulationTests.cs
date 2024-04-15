namespace Allors.Embedded.Tests.Domain
{
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class EmbeddedPopulationTests
    {
        [Fact]
        public void New()
        {
            var meta = new EmbeddedMeta();
            var named = meta.AddInterface("Named");
            var organization = meta.AddClass("Organization", named);
            var person = meta.AddClass("Person", named);
            meta.AddUnit<string>(named, "Name");
            meta.AddOneToOne(organization, person, "Owner");

            var population = new EmbeddedPopulation();

            var acme = population.Create(organization, v =>
            {
                v["Name"] = "Acme";
                v["Owner"] = population.Create(person, w => w["Name"] = "Jane");
            });

            var jane = (EmbeddedObject)acme["Owner"]!;

            Assert.Equal("Acme", acme["Name"]);
            Assert.Equal("Jane", jane["Name"]);

            Assert.Equal(acme, jane["OrganizationWhereOwner"]);
        }
    }
}
