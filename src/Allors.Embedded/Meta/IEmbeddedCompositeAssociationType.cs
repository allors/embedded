namespace Allors.Embedded.Meta
{
    public interface IEmbeddedCompositeAssociationType : IEmbeddedAssociationType
    {
        new IEmbeddedCompositeRoleType RoleType { get; }

        bool IsOne { get; }

        bool IsMany { get; }
    }
}
