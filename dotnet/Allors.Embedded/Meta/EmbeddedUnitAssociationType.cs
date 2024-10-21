namespace Allors.Embedded.Meta
{
    public sealed class EmbeddedUnitAssociationType : IEmbeddedAssociationType
    {
        internal EmbeddedUnitAssociationType(EmbeddedObjectType objectType, EmbeddedUnitRoleType roleType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.RoleType = roleType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedRoleType IEmbeddedAssociationType.RoleType => this.RoleType;

        public EmbeddedUnitRoleType RoleType { get; }

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }
    }
}
