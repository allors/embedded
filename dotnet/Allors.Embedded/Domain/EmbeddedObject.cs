namespace Allors.Embedded.Domain
{
    using System;
    using System.Collections.Generic;
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
                    return this[roleType];
                }

                if (this.ObjectType.AssociationTypeByName.TryGetValue(name, out var associationType))
                {
                    return this[associationType];
                }

                throw new ArgumentException("Unknown role or association", name);
            }

            set
            {
                if (!this.ObjectType.RoleTypeByName.TryGetValue(name, out var roleType))
                {
                    throw new ArgumentException("Unknown role", name);
                }

                this[roleType] = value;
            }
        }

        public object? this[IEmbeddedRoleType roleType]
        {
            get => roleType switch
            {
                EmbeddedUnitRoleType unitRoleType => this[unitRoleType],
                IEmbeddedToOneRoleType toOneRoleType => this[toOneRoleType],
                IEmbeddedToManyRoleType toManyRoleType => this[toManyRoleType],
                _ => throw new InvalidOperationException(),
            };
            set
            {
                switch (roleType)
                {
                case EmbeddedUnitRoleType unitRoleType:
                    this[unitRoleType] = value;
                    return;

                case IEmbeddedToOneRoleType toOneRoleType:
                    this[toOneRoleType] = (IEmbeddedObject?)value;
                    return;

                case IEmbeddedToManyRoleType toManyRoleType:
                    this[toManyRoleType] = (IEnumerable<IEmbeddedObject>)(value ?? Array.Empty<IEmbeddedObject>());
                    return;

                default:
                    throw new InvalidOperationException();
                }
            }
        }

        public object? this[EmbeddedUnitRoleType roleType]
        {
            get => this.Population.GetUnitRole(this, roleType);
            set => this.Population.SetUnitRole(this, roleType, value);
        }

        public IEmbeddedObject? this[IEmbeddedToOneRoleType roleType]
        {
            get => this.Population.GetToOneRole(this, roleType);
            set
            {
                this.Population.SetToOneRole(this, roleType, value);
            }
        }

        public IEnumerable<IEmbeddedObject> this[IEmbeddedToManyRoleType roleType]
        {
            get => this.Population.GetToManyRole(this, roleType) ?? [];
            set => this.Population.SetToManyRole(this, roleType, value);
        }

        public object? this[IEmbeddedAssociationType associationType] => associationType switch
        {
            IEmbeddedOneToAssociationType oneToAssociationType => this[oneToAssociationType],
            IEmbeddedManyToAssociationType manyToAssociationType => this[manyToAssociationType],
            _ => throw new InvalidOperationException(),
        };

        public IEmbeddedObject? this[IEmbeddedOneToAssociationType associationType] => this.Population.GetToOneAssociation(this, associationType);

        public IEnumerable<IEmbeddedObject> this[IEmbeddedManyToAssociationType associationType] => this.Population.GetToManyAssociation(this, associationType) ?? [];

        public void Add(IEmbeddedToManyRoleType roleType, IEmbeddedObject item) => this.Population.AddToManyRole(this, roleType, item);

        public void Add(IEmbeddedToManyRoleType roleType, params IEmbeddedObject[] items) => this.Population.AddToManyRole(this, roleType, items);

        public void Remove(IEmbeddedToManyRoleType roleType, IEmbeddedObject item) => this.Population.RemoveToManyRole(this, roleType, item);

        public void Remove(IEmbeddedToManyRoleType roleType, params IEmbeddedObject[] items) => this.Population.RemoveToManyRole(this, roleType, items);
    }
}
