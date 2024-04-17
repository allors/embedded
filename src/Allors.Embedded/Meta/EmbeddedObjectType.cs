namespace Allors.Embedded.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class EmbeddedObjectType
    {
        private readonly Dictionary<string, IEmbeddedAssociationType> assignedAssociationTypeByName;
        private readonly Dictionary<string, IEmbeddedRoleType> assignedRoleTypeByName;
        private readonly HashSet<EmbeddedObjectType> directSupertypes;

        private IDictionary<string, IEmbeddedAssociationType>? derivedAssociationTypeByName;
        private IDictionary<string, IEmbeddedRoleType>? derivedRoleTypeByName;
        private HashSet<EmbeddedObjectType>? derivedSupertypes;

        internal EmbeddedObjectType(EmbeddedMeta meta, EmbeddedObjectTypeKind kind, string name, EmbeddedObjectType[] directSupertypes)
        {
            this.Meta = meta;
            this.Kind = kind;
            this.Name = name;

            this.directSupertypes = [.. directSupertypes];
            this.assignedAssociationTypeByName = [];
            this.assignedRoleTypeByName = [];

            this.Meta.ResetDerivations();
        }

        internal EmbeddedObjectType(EmbeddedMeta meta, Type type)
            : this(meta, EmbeddedObjectTypeKind.Unit, type.Name, [])
        {
            this.Type = type;
            this.TypeCode = Type.GetTypeCode(type);
        }

        public EmbeddedMeta Meta { get; }

        public EmbeddedObjectTypeKind Kind { get; set; }

        public string Name { get; }

        public TypeCode? TypeCode { get; }

        public Type? Type { get; }

        public IReadOnlySet<EmbeddedObjectType> DirectSupertypes => this.directSupertypes;

        public IReadOnlySet<EmbeddedObjectType> Supertypes
        {
            get
            {
                if (this.derivedSupertypes != null)
                {
                    return this.derivedSupertypes;
                }

                this.derivedSupertypes = [];
                this.AddSupertypes(this.derivedSupertypes);
                return this.derivedSupertypes;
            }
        }

        public IDictionary<string, IEmbeddedAssociationType> AssociationTypeByName
        {
            get
            {
                if (this.derivedAssociationTypeByName == null)
                {
                    this.derivedAssociationTypeByName = new Dictionary<string, IEmbeddedAssociationType>(this.assignedAssociationTypeByName);
                    foreach (var item in this.Supertypes.SelectMany(v => v.assignedAssociationTypeByName))
                    {
                        this.derivedAssociationTypeByName[item.Key] = item.Value;
                    }
                }

                return this.derivedAssociationTypeByName;
            }
        }

        public IDictionary<string, IEmbeddedRoleType> RoleTypeByName
        {
            get
            {
                if (this.derivedRoleTypeByName != null)
                {
                    return this.derivedRoleTypeByName;
                }

                this.derivedRoleTypeByName = new Dictionary<string, IEmbeddedRoleType>(this.assignedRoleTypeByName);
                foreach (var item in this.Supertypes.SelectMany(v => v.assignedRoleTypeByName))
                {
                    this.derivedRoleTypeByName[item.Key] = item.Value;
                }

                return this.derivedRoleTypeByName;
            }
        }

        public override string ToString() => this.Name;

        public void AddDirectSupertype(EmbeddedObjectType directSupertype)
        {
            this.directSupertypes.Add(directSupertype);
            this.Meta.ResetDerivations();
        }

        public bool IsAssignableFrom(EmbeddedObjectType other)
        {
            return this == other || other.Supertypes.Contains(this);
        }

        internal EmbeddedUnitRoleType AddUnit(EmbeddedObjectType objectType, string? roleSingularName, string? associationSingularName)
        {
            roleSingularName ??= objectType.Name;
            string rolePluralName = this.Meta.Pluralize(roleSingularName);

            var roleType = new EmbeddedUnitRoleType(
                objectType,
                roleSingularName,
                rolePluralName,
                roleSingularName);

            string associationPluralName;
            if (associationSingularName != null)
            {
                associationPluralName = this.Meta.Pluralize(associationSingularName);
            }
            else
            {
                associationSingularName = roleType.SingularNameForAssociationType(this);
                associationPluralName = roleType.PluralNameForAssociationType(this);
            }

            roleType.AssociationType = new EmbeddedUnitAssociationType(
                this,
                roleType,
                associationSingularName,
                associationPluralName,
                associationSingularName);

            this.AddRoleType(roleType);
            objectType.AddAssociationType(roleType.AssociationType);

            this.Meta.ResetDerivations();

            return roleType;
        }

        internal EmbeddedOneToOneRoleType AddOneToOne(EmbeddedObjectType objectType, string? roleSingularName, string? associationSingularName)
        {
            roleSingularName ??= objectType.Name;
            string rolePluralName = this.Meta.Pluralize(roleSingularName);

            var roleType = new EmbeddedOneToOneRoleType(
                objectType,
                roleSingularName,
                rolePluralName,
                roleSingularName);

            string associationPluralName;
            if (associationSingularName != null)
            {
                associationPluralName = this.Meta.Pluralize(associationSingularName);
            }
            else
            {
                associationSingularName = roleType.SingularNameForAssociationType(this);
                associationPluralName = roleType.PluralNameForAssociationType(this);
            }

            roleType.AssociationType = new EmbeddedOneToOneAssociationType(
                this,
                roleType,
                associationSingularName,
                associationPluralName,
                associationSingularName);

            this.AddRoleType(roleType);
            objectType.AddAssociationType(roleType.AssociationType);

            this.Meta.ResetDerivations();

            return roleType;
        }

        internal EmbeddedManyToOneRoleType AddManyToOne(EmbeddedObjectType objectType, string? roleSingularName, string? associationSingularName)
        {
            roleSingularName ??= objectType.Name;
            string rolePluralName = this.Meta.Pluralize(roleSingularName);

            var roleType = new EmbeddedManyToOneRoleType(
                objectType,
                roleSingularName,
                rolePluralName,
                roleSingularName);

            string associationPluralName;
            if (associationSingularName != null)
            {
                associationPluralName = this.Meta.Pluralize(associationSingularName);
            }
            else
            {
                associationSingularName = roleType.SingularNameForAssociationType(this);
                associationPluralName = roleType.PluralNameForAssociationType(this);
            }

            roleType.AssociationType = new EmbeddedManyToOneAssociationType(
                this,
                roleType,
                associationSingularName,
                associationPluralName,
                associationPluralName);

            this.AddRoleType(roleType);
            objectType.AddAssociationType(roleType.AssociationType);

            this.Meta.ResetDerivations();

            return roleType;
        }

        internal EmbeddedOneToManyRoleType AddOneToMany(EmbeddedObjectType objectType, string? roleSingularName, string? associationSingularName)
        {
            roleSingularName ??= objectType.Name;
            string rolePluralName = this.Meta.Pluralize(roleSingularName);

            var roleType = new EmbeddedOneToManyRoleType(
                objectType,
                roleSingularName,
                rolePluralName,
                rolePluralName);

            string associationPluralName;
            if (associationSingularName != null)
            {
                associationPluralName = this.Meta.Pluralize(associationSingularName);
            }
            else
            {
                associationSingularName = roleType.SingularNameForAssociationType(this);
                associationPluralName = roleType.PluralNameForAssociationType(this);
            }

            roleType.AssociationType = new EmbeddedOneToManyAssociationType(
                this,
                roleType,
                associationSingularName,
                associationPluralName,
                associationSingularName);

            this.AddRoleType(roleType);
            objectType.AddAssociationType(roleType.AssociationType);

            this.Meta.ResetDerivations();

            return roleType;
        }

        internal EmbeddedManyToManyRoleType AddManyToMany(EmbeddedObjectType objectType, string? roleSingularName, string? associationSingularName)
        {
            roleSingularName ??= objectType.Name;
            string rolePluralName = this.Meta.Pluralize(roleSingularName);

            var roleType = new EmbeddedManyToManyRoleType(
                objectType,
                roleSingularName,
                rolePluralName,
                rolePluralName);

            string associationPluralName;
            if (associationSingularName != null)
            {
                associationPluralName = this.Meta.Pluralize(associationSingularName);
            }
            else
            {
                associationSingularName = roleType.SingularNameForAssociationType(this);
                associationPluralName = roleType.PluralNameForAssociationType(this);
            }

            roleType.AssociationType = new EmbeddedManyToManyAssociationType(
                this,
                roleType,
                associationSingularName,
                associationPluralName,
                associationPluralName);

            this.AddRoleType(roleType);
            objectType.AddAssociationType(roleType.AssociationType);

            this.Meta.ResetDerivations();

            return roleType;
        }

        internal void ResetDerivations()
        {
            this.derivedSupertypes = null;
            this.derivedAssociationTypeByName = null;
            this.derivedRoleTypeByName = null;
        }

        private void AddSupertypes(HashSet<EmbeddedObjectType> newDerivedSupertypes)
        {
            foreach (var supertype in this.directSupertypes.Where(supertype => !newDerivedSupertypes.Contains(supertype)))
            {
                newDerivedSupertypes.Add(supertype);
                supertype.AddSupertypes(newDerivedSupertypes);
            }
        }

        private void AddAssociationType(IEmbeddedAssociationType associationType)
        {
            this.CheckNames(associationType.SingularName, associationType.PluralName);

            this.assignedAssociationTypeByName.Add(associationType.Name, associationType);
        }

        private void AddRoleType(IEmbeddedRoleType roleType)
        {
            this.CheckNames(roleType.SingularName, roleType.PluralName);

            this.assignedRoleTypeByName.Add(roleType.Name, roleType);
        }

        private void CheckNames(string singularName, string pluralName)
        {
            if (this.RoleTypeByName.ContainsKey(singularName) ||
                this.AssociationTypeByName.ContainsKey(singularName))
            {
                throw new ArgumentException($"{singularName} is not unique");
            }

            if (this.RoleTypeByName.ContainsKey(pluralName) ||
                this.AssociationTypeByName.ContainsKey(pluralName))
            {
                throw new ArgumentException($"{pluralName} is not unique");
            }
        }
    }
}
