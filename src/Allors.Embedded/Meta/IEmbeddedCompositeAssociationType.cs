namespace Allors.Embedded.Meta
{
    public interface IEmbeddedCompositeAssociationType : IEmbeddedAssociationType
    {
        bool IsOne { get; }

        bool IsMany { get; }
    }
}
