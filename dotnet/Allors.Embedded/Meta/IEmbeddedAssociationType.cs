namespace Allors.Embedded.Meta
{
    public interface IEmbeddedAssociationType
    {
        EmbeddedObjectType ObjectType { get; }

        IEmbeddedRoleType RoleType { get; }

        string SingularName { get; }

        string PluralName { get; }

        string Name { get; }
    }
}
