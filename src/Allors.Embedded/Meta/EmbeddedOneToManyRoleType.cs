﻿namespace Allors.Embedded.Meta
{
    public sealed class EmbeddedOneToManyRoleType : IEmbeddedToManyRoleType
    {
        internal EmbeddedOneToManyRoleType(EmbeddedObjectType objectType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedAssociationType IEmbeddedRoleType.AssociationType => this.AssociationType;

        IEmbeddedCompositeAssociationType IEmbeddedCompositeRoleType.AssociationType => this.AssociationType;

        public EmbeddedOneToManyAssociationType AssociationType { get; internal set; } = null!;

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }

        public bool IsOne => false;

        public bool IsMany => true;

        void IEmbeddedRoleType.Deconstruct(out IEmbeddedAssociationType associationType, out IEmbeddedRoleType roleType)
        {
            associationType = this.AssociationType;
            roleType = this;
        }

        public void Deconstruct(out EmbeddedOneToManyAssociationType associationType, out EmbeddedOneToManyRoleType roleType)
        {
            associationType = this.AssociationType;
            roleType = this;
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal string SingularNameForAssociationType(EmbeddedObjectType embeddedObjectType)
        {
            return $"{embeddedObjectType.Name}Where{this.SingularName}";
        }

        internal string PluralNameForAssociationType(EmbeddedObjectType embeddedObjectType)
        {
            return $"{this.ObjectType.Meta.Pluralize(embeddedObjectType.Name)}Where{this.SingularName}";
        }
    }
}
