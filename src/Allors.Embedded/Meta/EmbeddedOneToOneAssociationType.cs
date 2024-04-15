namespace Allors.Embedded.Meta
{
    using System.Runtime.InteropServices.Marshalling;

    public sealed class EmbeddedOneToOneAssociationType : IEmbeddedOneToAssociationType
    {
        internal EmbeddedOneToOneAssociationType(EmbeddedObjectType objectType, EmbeddedOneToOneRoleType roleType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.RoleType = roleType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedRoleType IEmbeddedAssociationType.RoleType => this.RoleType;

        public EmbeddedOneToOneRoleType RoleType { get; }

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }

        public bool IsOne => true;

        public bool IsMany => false;
    }
}
