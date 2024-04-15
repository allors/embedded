namespace Allors.Embedded.Meta
{
    public interface IEmbeddedCompositeRoleType : IEmbeddedRoleType
    {
        bool IsOne { get; }

        bool IsMany { get; }
    }
}
