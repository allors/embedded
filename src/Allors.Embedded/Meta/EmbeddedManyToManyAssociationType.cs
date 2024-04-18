namespace Allors.Embedded.Meta
{
    public sealed class EmbeddedManyToManyAssociationType : IEmbeddedManyToAssociationType
    {
        internal EmbeddedManyToManyAssociationType(EmbeddedObjectType objectType, EmbeddedManyToManyRoleType roleType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.RoleType = roleType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedRoleType IEmbeddedAssociationType.RoleType => this.RoleType;

        IEmbeddedCompositeRoleType IEmbeddedCompositeAssociationType.RoleType => this.RoleType;

        public EmbeddedManyToManyRoleType RoleType { get; }

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }

        public bool IsOne => false;

        public bool IsMany => true;
    }
}
