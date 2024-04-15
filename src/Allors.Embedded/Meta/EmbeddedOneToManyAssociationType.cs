namespace Allors.Embedded.Meta
{
    public sealed class EmbeddedOneToManyAssociationType : IEmbeddedOneToAssociationType
    {
        internal EmbeddedOneToManyAssociationType(EmbeddedObjectType objectType, EmbeddedOneToManyRoleType roleType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.RoleType = roleType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedRoleType IEmbeddedAssociationType.RoleType => this.RoleType;

        public EmbeddedOneToManyRoleType RoleType { get; }

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }

        public bool IsOne => true;

        public bool IsMany => false;
    }
}
