namespace Allors.Embedded.Tests.Domain
{
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Xunit;

    public class UnitTests
    {
        [Fact]
        public void SameRoleTypeName()
        {
            var meta = new EmbeddedMeta();
            var c1 = meta.AddClass("C1");
            var c2 = meta.AddClass("C2");
            meta.AddUnit<string>(c1, "Same");
            meta.AddUnit<string>(c2, "Same");

            var population = new EmbeddedPopulation();

            var c1a = population.Create(c1, v =>
            {
                v["Same"] = "c1";
            });

            var c2a = population.Create(c2, v =>
            {
                v["Same"] = "c2";
            });

            Assert.Equal("c1", c1a["Same"]);
            Assert.Equal("c2", c2a["Same"]);
        }

        [Fact]
        public void PropertySetByString()
        {
            var meta = new EmbeddedMeta();
            var person = meta.AddClass("Person");
            var unitRoleType = meta.AddUnit<string>(person, "FirstName");

            var population = new EmbeddedPopulation();

            var john = population.Create(person);
            var jane = population.Create(person);

            john["FirstName"] = "John";
            jane["FirstName"] = "Jane";

            Assert.Equal("John", john["FirstName"]);
            Assert.Equal("Jane", jane["FirstName"]);
            Assert.Equal("John", john[unitRoleType]);
            Assert.Equal("Jane", jane[unitRoleType]);

            jane["FirstName"] = null;

            Assert.Equal("John", john["FirstName"]);
            Assert.Null(jane["FirstName"]);
            Assert.Equal("John", john[unitRoleType]);
            Assert.Null(jane[unitRoleType]);
        }

        [Fact]
        public void PropertySetByUnitRoleType()
        {
            var meta = new EmbeddedMeta();
            var person = meta.AddClass("Person");
            var unitRoleType = meta.AddUnit<string>(person, "FirstName");

            var population = new EmbeddedPopulation();

            var john = population.Create(person);
            var jane = population.Create(person);

            john[unitRoleType] = "John";
            jane[unitRoleType] = "Jane";

            Assert.Equal("John", john["FirstName"]);
            Assert.Equal("Jane", jane["FirstName"]);
            Assert.Equal("John", john[unitRoleType]);
            Assert.Equal("Jane", jane[unitRoleType]);

            jane[unitRoleType] = null;

            Assert.Equal("John", john["FirstName"]);
            Assert.Null(jane["FirstName"]);
            Assert.Equal("John", john[unitRoleType]);
            Assert.Null(jane[unitRoleType]);
        }
    }
}
