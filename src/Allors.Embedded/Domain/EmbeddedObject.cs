namespace Allors.Embedded.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Allors.Embedded.Meta;

    public class EmbeddedObject(EmbeddedPopulation population, EmbeddedObjectType objectType)
        : IEmbeddedObject
    {
        public EmbeddedPopulation Population { get; } = population;

        public EmbeddedObjectType ObjectType { get; } = objectType;

        public object? this[string name]
        {
            get
            {
                if (this.ObjectType.RoleTypeByName.TryGetValue(name, out var roleType))
                {
                    return roleType switch
                    {
                        EmbeddedUnitRoleType unitRoleType => this.Population.GetRole(this, unitRoleType),
                        IEmbeddedToOneRoleType toOneRoleType => (IEmbeddedObject?)this.Population.GetRole(this, toOneRoleType),
                        IEmbeddedToManyRoleType toManyRoleType => (IEnumerable<IEmbeddedObject>?)this.Population.GetRole(this, toManyRoleType) ?? [],
                        _ => throw new InvalidOperationException(),
                    };
                }

                if (this.ObjectType.AssociationTypeByName.TryGetValue(name, out var associationType))
                {
                    return associationType switch
                    {
                        IEmbeddedOneToAssociationType oneToAssociationType => (IEmbeddedObject?)this.Population.GetAssociation(this, oneToAssociationType),
                        IEmbeddedManyToAssociationType oneToAssociationType => (IEnumerable<IEmbeddedObject>?)this.Population.GetAssociation(this, oneToAssociationType) ?? [],
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
                IEmbeddedToOneRoleType toOneRoleType => (IEmbeddedObject?)this.Population.GetRole(this, toOneRoleType),
                IEmbeddedToManyRoleType toManyRoleType => (IEnumerable<IEmbeddedObject>?)this.Population.GetRole(this, toManyRoleType) ?? [],
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

        public IEmbeddedObject? this[IEmbeddedToOneRoleType roleType]
        {
            get => (IEmbeddedObject?)this.Population.GetRole(this, roleType);
            set => this.Population.SetToOneRole(this, roleType, value);
        }

        public IReadOnlySet<IEmbeddedObject> this[IEmbeddedToManyRoleType roleType]
        {
            get => (IReadOnlySet<IEmbeddedObject>?)this.Population.GetRole(this, roleType) ?? ImmutableHashSet<IEmbeddedObject>.Empty;
            set => this.Population.SetToManyRole(this, roleType, value);
        }

        public object? this[IEmbeddedAssociationType associationType] => associationType switch
        {
            IEmbeddedOneToAssociationType oneToAssociationType => (IEmbeddedObject?)this.Population.GetAssociation(this, oneToAssociationType),
            IEmbeddedManyToAssociationType oneToAssociationType => (IEnumerable<IEmbeddedObject>?)this.Population.GetAssociation(this, oneToAssociationType) ?? [],
            _ => throw new InvalidOperationException(),
        };

        public IEmbeddedObject? this[IEmbeddedOneToAssociationType associationType] => (IEmbeddedObject?)this.Population.GetAssociation(this, associationType);

        public IReadOnlySet<IEmbeddedObject> this[IEmbeddedManyToAssociationType associationType] => (IReadOnlySet<IEmbeddedObject>?)this.Population.GetAssociation(this, associationType) ?? ImmutableHashSet<IEmbeddedObject>.Empty;

        public void Add(IEmbeddedToManyRoleType roleType, IEmbeddedObject item) => this.Population.AddRole(this, roleType, item);

        public void Add(IEmbeddedToManyRoleType roleType, params IEmbeddedObject[] items) => this.Population.AddRole(this, roleType, items);

        public void Add(IEmbeddedToManyRoleType roleType, IEnumerable<IEmbeddedObject> items) => this.Population.AddRole(this, roleType, items as IEmbeddedObject[] ?? items.ToArray());

        public void Remove(IEmbeddedToManyRoleType roleType, IEmbeddedObject item) => this.Population.RemoveRole(this, roleType, item);

        public void Remove(IEmbeddedToManyRoleType roleType, params IEmbeddedObject[] items) => this.Population.RemoveRole(this, roleType, items);

        public void Remove(IEmbeddedToManyRoleType roleType, IEnumerable<IEmbeddedObject> items) => this.Population.RemoveRole(this, roleType, items as EmbeddedObject[] ?? items.ToArray());
    }
}
