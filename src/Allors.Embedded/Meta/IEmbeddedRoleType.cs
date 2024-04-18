namespace Allors.Embedded.Meta
{
    public interface IEmbeddedRoleType
    {
        IEmbeddedAssociationType AssociationType { get; }

        EmbeddedObjectType ObjectType { get; }

        string SingularName { get; }

        string PluralName { get; }

        string Name { get; }

        void Deconstruct(out IEmbeddedRoleType roleType, out IEmbeddedAssociationType associationType);
    }
}
