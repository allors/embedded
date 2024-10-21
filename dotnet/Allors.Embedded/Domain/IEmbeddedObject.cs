namespace Allors.Embedded.Domain
{
    using System.Collections.Generic;
    using Allors.Embedded.Meta;

    public interface IEmbeddedObject
    {
        EmbeddedPopulation Population { get; }

        EmbeddedObjectType ObjectType { get; }

        object? this[string name] { get; set; }

        object? this[IEmbeddedRoleType roleType] { get; set; }

        object? this[EmbeddedUnitRoleType roleType] { get; set; }

        IEmbeddedObject? this[IEmbeddedToOneRoleType roleType] { get; set; }

        IEnumerable<IEmbeddedObject> this[IEmbeddedToManyRoleType roleType] { get; set; }

        object? this[IEmbeddedAssociationType associationType] { get; }

        IEmbeddedObject? this[IEmbeddedOneToAssociationType associationType] { get; }

        IEnumerable<IEmbeddedObject> this[IEmbeddedManyToAssociationType associationType] { get; }

        void Add(IEmbeddedToManyRoleType roleType, IEmbeddedObject item);

        void Add(IEmbeddedToManyRoleType roleType, params IEmbeddedObject[] items);

        void Remove(IEmbeddedToManyRoleType roleType, IEmbeddedObject item);

        void Remove(IEmbeddedToManyRoleType roleType, params IEmbeddedObject[] items);
    }
}
