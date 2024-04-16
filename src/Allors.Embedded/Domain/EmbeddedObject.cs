namespace Allors.Embedded.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Allors.Embedded.Meta;

    public sealed class EmbeddedObject
    {
        internal EmbeddedObject(EmbeddedPopulation population, EmbeddedObjectType objectType)
        {
            this.Population = population;
            this.ObjectType = objectType;
        }

        public EmbeddedPopulation Population { get; }

        public EmbeddedObjectType ObjectType { get; }

        public object? this[string name]
        {
            get
            {
                if (this.ObjectType.RoleTypeByName.TryGetValue(name, out var roleType))
                {
                    return roleType switch
                    {
                        EmbeddedUnitRoleType unitRoleType => this.Population.GetRole(this, unitRoleType),
                        IEmbeddedToOneRoleType toOneRoleType => (EmbeddedObject?)this.Population.GetRole(this, toOneRoleType),
                        IEmbeddedToManyRoleType toManyRoleType => (IEnumerable<EmbeddedObject>?)this.Population.GetRole(this, toManyRoleType) ?? [],
                        _ => throw new InvalidOperationException(),
                    };
                }

                if (this.ObjectType.AssociationTypeByName.TryGetValue(name, out var associationType))
                {
                    return associationType switch
                    {
                        IEmbeddedOneToAssociationType oneToAssociationType => (EmbeddedObject?)this.Population.GetAssociation(this, oneToAssociationType),
                        IEmbeddedManyToAssociationType oneToAssociationType => (IEnumerable<EmbeddedObject>?)this.Population.GetAssociation(this, oneToAssociationType) ?? [],
                        _ => throw new InvalidOperationException(),
                    };
                }

                throw new ArgumentException("Unknown role or association", name);
            }

            set
            {
                if (this.ObjectType.RoleTypeByName.TryGetValue(name, out var roleType))
                {
                    switch (roleType)
                    {
                    case EmbeddedUnitRoleType unitRoleType:
                        this.Population.SetUnitRole(this, unitRoleType, value);
                        return;

                    case IEmbeddedToOneRoleType toOneRoleType:
                        EmbeddedObject? value1 = (EmbeddedObject?)value;
                        this.Population.SetToOneRole(this, toOneRoleType, value1);
                        return;

                    case IEmbeddedToManyRoleType toManyRoleType:
                        this.Population.SetToManyRole(this, toManyRoleType, value);
                        return;

                    default:
                        throw new InvalidOperationException();
                    }
                }

                throw new ArgumentException("Unknown role", name);
            }
        }

        public object? this[IEmbeddedRoleType roleType]
        {
            get => roleType switch
            {
                EmbeddedUnitRoleType unitRoleType => this.Population.GetRole(this, unitRoleType),
                IEmbeddedToOneRoleType toOneRoleType => (EmbeddedObject?)this.Population.GetRole(this, toOneRoleType),
                IEmbeddedToManyRoleType toManyRoleType => (IEnumerable<EmbeddedObject>?)this.Population.GetRole(this, toManyRoleType) ?? [],
                _ => throw new InvalidOperationException(),
            };
            set
            {
                switch (roleType)
                {
                case EmbeddedUnitRoleType unitRoleType:
                    this.Population.SetUnitRole(this, unitRoleType, value);
                    return;

                case IEmbeddedToOneRoleType toOneRoleType:
                    EmbeddedObject? value1 = (EmbeddedObject?)value;
                    this.Population.SetToOneRole(this, toOneRoleType, value1);
                    return;

                case IEmbeddedToManyRoleType toManyRoleType:
                    this.Population.SetToManyRole(this, toManyRoleType, value);
                    return;

                default:
                    throw new InvalidOperationException();
                }
            }
        }

        public object? this[EmbeddedUnitRoleType roleType]
        {
            get => this.Population.GetRole(this, roleType);
            set => this.Population.SetUnitRole(this, roleType, value);
        }

        public EmbeddedObject? this[EmbeddedOneToOneRoleType roleType]
        {
            get => (EmbeddedObject?)this.Population.GetRole(this, roleType);
            set => this.Population.SetToOneRole(this, roleType, value);
        }

        public EmbeddedObject? this[EmbeddedManyToOneRoleType roleType]
        {
            get => (EmbeddedObject?)this.Population.GetRole(this, roleType);
            set => this.Population.SetToOneRole(this, roleType, value);
        }

        public IReadOnlySet<EmbeddedObject> this[EmbeddedOneToManyRoleType roleType]
        {
            get => (IReadOnlySet<EmbeddedObject>?)this.Population.GetRole(this, roleType) ?? ImmutableHashSet<EmbeddedObject>.Empty;
            set => this.Population.SetToManyRole(this, roleType, value);
        }

        public IReadOnlySet<EmbeddedObject> this[EmbeddedManyToManyRoleType roleType]
        {
            get => (IReadOnlySet<EmbeddedObject>?)this.Population.GetRole(this, roleType) ?? ImmutableHashSet<EmbeddedObject>.Empty;
            set => this.Population.SetToManyRole(this, roleType, value);
        }

        public object? this[IEmbeddedAssociationType associationType] => associationType switch
        {
            IEmbeddedOneToAssociationType oneToAssociationType => (EmbeddedObject?)this.Population.GetAssociation(this, oneToAssociationType),
            IEmbeddedManyToAssociationType oneToAssociationType => (IEnumerable<EmbeddedObject>?)this.Population.GetAssociation(this, oneToAssociationType) ?? [],
            _ => throw new InvalidOperationException(),
        };

        public EmbeddedObject? this[EmbeddedOneToOneAssociationType associationType] => (EmbeddedObject?)this.Population.GetAssociation(this, associationType);

        public EmbeddedObject? this[EmbeddedOneToManyAssociationType associationType] => (EmbeddedObject?)this.Population.GetAssociation(this, associationType);

        public IReadOnlySet<EmbeddedObject> this[EmbeddedManyToOneAssociationType associationType] => (IReadOnlySet<EmbeddedObject>?)this.Population.GetAssociation(this, associationType) ?? ImmutableHashSet<EmbeddedObject>.Empty;

        public IReadOnlySet<EmbeddedObject> this[EmbeddedManyToManyAssociationType associationType] => (IReadOnlySet<EmbeddedObject>?)this.Population.GetAssociation(this, associationType) ?? ImmutableHashSet<EmbeddedObject>.Empty;

        public void Add(IEmbeddedToManyRoleType roleType, EmbeddedObject item) => this.Population.AddToRole(this, roleType, item);

        public void Add(IEmbeddedToManyRoleType roleType, params EmbeddedObject[] items) => this.Population.AddToRole(this, roleType, items);

        public void Add(IEmbeddedToManyRoleType roleType, IEnumerable<EmbeddedObject> items) => this.Population.AddToRole(this, roleType, items as EmbeddedObject[] ?? items.ToArray());

        public void Remove(IEmbeddedToManyRoleType roleType, EmbeddedObject item) => this.Population.RemoveFromRole(this, roleType, item);

        public void Remove(IEmbeddedToManyRoleType roleType, params EmbeddedObject[] items) => this.Population.RemoveFromRole(this, roleType, items);

        public void Remove(IEmbeddedToManyRoleType roleType, IEnumerable<EmbeddedObject> items) => this.Population.RemoveFromRole(this, roleType, items as EmbeddedObject[] ?? items.ToArray());
    }
}
