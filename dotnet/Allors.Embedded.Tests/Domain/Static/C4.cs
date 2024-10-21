namespace Allors.Embedded.Tests.Domain.Static
{
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;

    public class C4(EmbeddedPopulation population, EmbeddedObjectType objectType)
        : EmbeddedObject(population, objectType), I1;
}
