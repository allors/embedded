namespace Allors.Embedded.Meta
{
    public sealed class EmbeddedManyToOneAssociationType : IEmbeddedManyToAssociationType
    {
        internal EmbeddedManyToOneAssociationType(EmbeddedObjectType objectType, EmbeddedManyToOneRoleType roleType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.RoleType = roleType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedRoleType IEmbeddedAssociationType.RoleType => this.RoleType;

        public EmbeddedManyToOneRoleType RoleType { get; }

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }

        public bool IsOne => false;

        public bool IsMany => true;
    }
}
