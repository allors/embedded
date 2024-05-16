namespace Allors.Embedded.Tests.Domain
{
    using System;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Allors.Embedded.Tests.Domain.Static;
    using Xunit;

    public class StaticTests
    {
        [Fact]
        public void C1C2OneToOne()
        {
            var meta = new EmbeddedMeta();
            var c1 = meta.AddClass<C1>();
            var c2 = meta.AddClass<C2>();
            var c1C2OneToOne = meta.AddOneToOne(c1, c2, "C2OneToOne");

            var population = new EmbeddedPopulation(meta);

            var c1a = population.Build<C1>();
            var c1b = population.Build<C1>();
            var c2a = population.Build<C2>();

            c1a[c1C2OneToOne] = c2a;

            Assert.Equal(c2a, c1a[c1C2OneToOne]);
            Assert.Null(c1b[c1C2OneToOne]);
            Assert.Equal(c1a, c2a[c1C2OneToOne.AssociationType]);
        }
    }
}
