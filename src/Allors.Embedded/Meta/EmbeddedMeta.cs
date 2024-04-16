namespace Allors.Embedded.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class EmbeddedMeta
    {
        private readonly Dictionary<string, EmbeddedObjectType> objectTypeByName;

        public EmbeddedMeta()
        {
            this.objectTypeByName = [];

            this.ObjectTypeByName = new ReadOnlyDictionary<string, EmbeddedObjectType>(this.objectTypeByName);
        }

        public IReadOnlyDictionary<string, EmbeddedObjectType> ObjectTypeByName { get; }

        public EmbeddedUnitRoleType AddUnit<TRole>(EmbeddedObjectType associationObjectType, string roleName, string? associationName = null) => associationObjectType.AddUnit(this.Unit(typeof(TRole)), roleName, associationName);

        public EmbeddedOneToOneRoleType AddOneToOne(EmbeddedObjectType associationObjectType, EmbeddedObjectType roleObjectType, string? roleName = null, string? associationName = null) => associationObjectType.AddOneToOne(roleObjectType, roleName, associationName);

        public EmbeddedManyToOneRoleType AddManyToOne(EmbeddedObjectType associationObjectType, EmbeddedObjectType roleObjectType, string? roleName = null, string? associationName = null) => associationObjectType.AddManyToOne(roleObjectType, roleName, associationName);

        public EmbeddedOneToManyRoleType AddOneToMany(EmbeddedObjectType associationObjectType, EmbeddedObjectType roleObjectType, string? roleName = null, string? associationName = null) => associationObjectType.AddOneToMany(roleObjectType, roleName, associationName);

        public EmbeddedManyToManyRoleType AddManyToMany(EmbeddedObjectType associationObjectType, EmbeddedObjectType roleObjectType, string? roleName = null, string? associationName = null) => associationObjectType.AddManyToMany(roleObjectType, roleName, associationName);

        public EmbeddedObjectType AddInterface(string name, params EmbeddedObjectType[] supertypes)
        {
            var objectType = new EmbeddedObjectType(this, EmbeddedObjectTypeKind.Interface, name, supertypes);
            this.objectTypeByName.Add(objectType.Name, objectType);
            return objectType;
        }

        public EmbeddedObjectType AddClass(string name, params EmbeddedObjectType[] supertypes)
        {
            var objectType = new EmbeddedObjectType(this, EmbeddedObjectTypeKind.Class, name, supertypes);
            this.objectTypeByName.Add(objectType.Name, objectType);
            return objectType;
        }

        internal string Pluralize(string singular)
        {
            static bool EndsWith(string word, string ending) => word.EndsWith(ending, StringComparison.InvariantCultureIgnoreCase);

            if (EndsWith(singular, "y") &&
                !EndsWith(singular, "ay") &&
                !EndsWith(singular, "ey") &&
                !EndsWith(singular, "iy") &&
                !EndsWith(singular, "oy") &&
                !EndsWith(singular, "uy"))
            {
                return singular.Substring(0, singular.Length - 1) + "ies";
            }

            if (EndsWith(singular, "us"))
            {
                return singular + "es";
            }

            if (EndsWith(singular, "ss"))
            {
                return singular + "es";
            }

            if (EndsWith(singular, "x") ||
                EndsWith(singular, "ch") ||
                EndsWith(singular, "sh"))
            {
                return singular + "es";
            }

            if (EndsWith(singular, "f") && singular.Length > 1)
            {
                return singular.Substring(0, singular.Length - 1) + "ves";
            }

            if (EndsWith(singular, "fe") && singular.Length > 2)
            {
                return singular.Substring(0, singular.Length - 2) + "ves";
            }

            return singular + "s";
        }

        internal void ResetDerivations()
        {
            foreach ((_, EmbeddedObjectType? objectType) in this.ObjectTypeByName)
            {
                objectType.ResetDerivations();
            }
        }

        private EmbeddedObjectType Unit(Type type)
        {
            if (!this.ObjectTypeByName.TryGetValue(type.Name, out var objectType))
            {
                objectType = new EmbeddedObjectType(this, type);
                this.objectTypeByName.Add(objectType.Name, objectType);
            }

            return objectType;
        }
    }
}
