namespace Allors.Embedded.Meta
{
    using System;
    using System.Globalization;

    public sealed class EmbeddedUnitRoleType : IEmbeddedRoleType
    {
        internal EmbeddedUnitRoleType(EmbeddedObjectType objectType, string singularName, string pluralName, string name)
        {
            this.ObjectType = objectType;
            this.SingularName = singularName;
            this.PluralName = pluralName;
            this.Name = name;
        }

        IEmbeddedAssociationType IEmbeddedRoleType.AssociationType => this.AssociationType;

        public EmbeddedUnitAssociationType AssociationType { get; internal set; } = null!;

        public EmbeddedObjectType ObjectType { get; }

        public string SingularName { get; }

        public string PluralName { get; }

        public string Name { get; }

        void IEmbeddedRoleType.Deconstruct(out IEmbeddedRoleType roleType, out IEmbeddedAssociationType associationType)
        {
            associationType = this.AssociationType;
            roleType = this;
        }

        public void Deconstruct(out EmbeddedUnitRoleType roleType, out EmbeddedUnitAssociationType associationType)
        {
            associationType = this.AssociationType;
            roleType = this;
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal object? Normalize(object? value)
        {
            if (value == null)
            {
                return value;
            }

            if (value is DateTime dateTime && dateTime != DateTime.MinValue && dateTime != DateTime.MaxValue)
            {
                dateTime = dateTime.Kind switch
                {
                    DateTimeKind.Local => dateTime.ToUniversalTime(),
                    DateTimeKind.Unspecified => throw new ArgumentException(@"DateTime value is of DateTimeKind.Kind Unspecified.
Unspecified is only allowed for DateTime.MaxValue and DateTime.MinValue. 
Use DateTimeKind.Utc or DateTimeKind.Local."),
                    _ => dateTime,
                };

                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, DateTimeKind.Utc);
            }

            if (value.GetType() != this.ObjectType.Type && this.ObjectType.TypeCode.HasValue)
            {
                value = Convert.ChangeType(value, this.ObjectType.TypeCode.Value, CultureInfo.InvariantCulture);
            }

            return value;
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
