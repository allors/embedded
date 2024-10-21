namespace Allors.Embedded.Meta
{
    public interface IEmbeddedCompositeRoleType : IEmbeddedRoleType
    {
        new IEmbeddedCompositeAssociationType AssociationType { get; }

        bool IsOne { get; }

        bool IsMany { get; }
    }
}
