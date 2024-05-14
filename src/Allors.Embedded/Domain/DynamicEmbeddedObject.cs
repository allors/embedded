namespace Allors.Embedded.Domain
{
    using Allors.Embedded.Meta;

    internal class DynamicEmbeddedObject : EmbeddedObject
    {
        internal DynamicEmbeddedObject(EmbeddedPopulation population, EmbeddedObjectType objectType)
            : base(population, objectType)
        {
        }
    }
}
