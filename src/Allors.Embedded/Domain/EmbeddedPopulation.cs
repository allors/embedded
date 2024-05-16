namespace Allors.Embedded.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using Allors.Embedded.Meta;

    public sealed class EmbeddedPopulation(EmbeddedMeta meta)
    {
        private readonly EmbeddedRelations relations = new();
        private readonly EmbeddedRelations changedRelations = new();

        private IImmutableList<IEmbeddedObject> objects = ImmutableArray<IEmbeddedObject>.Empty;

        public EmbeddedMeta Meta { get; } = meta;

        public Dictionary<string, IEmbeddedDerivation> DerivationById { get; } = [];

        public IReadOnlyList<IEmbeddedObject> Objects => this.objects;

        public IEmbeddedObject Build(EmbeddedObjectType @class, params Action<IEmbeddedObject>[] builders)
        {
            var @new = @class.Type != null ? (IEmbeddedObject)Activator.CreateInstance(@class.Type, this, @class)! : new EmbeddedObject(this, @class);
            this.objects = this.objects.Add(@new);

            foreach (var builder in builders)
            {
                builder(@new);
            }

            return @new;
        }

        public IEmbeddedObject Build<T>(params Action<T>[] builders)
            where T : IEmbeddedObject
        {
            string className = typeof(T).Name;
            var @class = this.Meta.ObjectTypeByName[className] ?? throw new ArgumentException($"Class with name {className} not found");

            if (@class.Type == null)
            {
                throw new ArgumentException("Class has no static type");
            }

            var @new = (T)Activator.CreateInstance(@class.Type, this, @class)!;
            this.objects = this.objects.Add(@new);

            foreach (var builder in builders)
            {
                builder(@new);
            }

            return @new;
        }

        public EmbeddedChangeSet Checkpoint()
        {
            return this.changedRelations.Snapshot(this.relations);
        }

        public void Derive()
        {
            var changeSet = this.Checkpoint();

            while (changeSet.HasChanges)
            {
                foreach (var derivation in this.DerivationById.Select(kvp => kvp.Value))
                {
                    derivation.Derive(changeSet);
                }

                changeSet = this.Checkpoint();
            }
        }

        internal object? GetUnitRole(EmbeddedObject association, EmbeddedUnitRoleType roleType) => this.GetRole(association, roleType);

        internal void SetUnitRole(EmbeddedObject association, EmbeddedUnitRoleType roleType, object? role)
        {
            var normalizedRole = this.Normalize(roleType, role);

            var currentRole = this.GetUnitRole(association, roleType);
            if (Equals(currentRole, normalizedRole))
            {
                return;
            }

            // Role
            this.changedRelations.RoleByAssociation(roleType)[association] = normalizedRole;
        }

        internal IEmbeddedObject? GetToOneRole(IEmbeddedObject association, IEmbeddedToOneRoleType roleType) => (IEmbeddedObject?)this.GetRole(association, roleType);

        internal void SetToOneRole(EmbeddedObject association, IEmbeddedToOneRoleType roleType, IEmbeddedObject? value)
        {
            switch (roleType)
            {
            case EmbeddedOneToOneRoleType oneToOneRoleType:
                if (value == null)
                {
                    this.RemoveOneToOneRole(association, oneToOneRoleType);
                    return;
                }

                this.SetOneToOneRole(association, oneToOneRoleType, (EmbeddedObject)value);
                return;

            case EmbeddedManyToOneRoleType manyToOneRoleType:
                if (value == null)
                {
                    this.RemoveManyToOneRole(association, manyToOneRoleType);
                    return;
                }

                this.SetManyToOneRole(association, manyToOneRoleType, (EmbeddedObject)value);
                return;

            default:
                throw new InvalidOperationException();
            }
        }

        internal IImmutableSet<IEmbeddedObject>? GetToManyRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType) => (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);

        internal void SetToManyRole(EmbeddedObject association, IEmbeddedToManyRoleType roleType, IEnumerable<IEmbeddedObject> items)
        {
            var normalizedRole = this.Normalize(roleType, items).Distinct().ToArray();

            switch (roleType)
            {
            case EmbeddedOneToManyRoleType toManyRoleType:
                if (normalizedRole.Length == 0)
                {
                    this.RemoveOneToManyRole(association, toManyRoleType);
                    return;
                }

                this.SetOneToManyRole(association, toManyRoleType, normalizedRole);
                return;

            case EmbeddedManyToManyRoleType toManyRoleType:
                if (normalizedRole.Length == 0)
                {
                    this.RemoveManyToManyRole(association, toManyRoleType);
                    return;
                }

                this.SetManyToManyRole(association, toManyRoleType, normalizedRole);
                return;

            default:
                throw new InvalidOperationException();
            }
        }

        internal void AddToManyRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject item)
        {
            switch (roleType)
            {
            case EmbeddedOneToManyRoleType toManyRoleType:
                this.AddOneToManyRole(association, toManyRoleType, item);
                return;

            case EmbeddedManyToManyRoleType toManyRoleType:
                this.AddManyToManyRole(association, toManyRoleType, item);
                return;

            default:
                throw new InvalidOperationException();
            }
        }

        internal void AddToManyRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            switch (roleType)
            {
            case EmbeddedOneToManyRoleType toManyRoleType:
                this.AddOneToManyRole(association, toManyRoleType, items);
                return;

            case EmbeddedManyToManyRoleType toManyRoleType:
                this.AddManyToManyRole(association, toManyRoleType, items);
                return;

            default:
                throw new InvalidOperationException();
            }
        }

        internal void RemoveToManyRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject item)
        {
            switch (roleType)
            {
            case EmbeddedOneToManyRoleType toManyRoleType:
                this.RemoveOneToManyRole(association, toManyRoleType, item);
                return;

            case EmbeddedManyToManyRoleType toManyRoleType:
                this.RemoveManyToManyRole(association, toManyRoleType, item);
                return;

            default:
                throw new InvalidOperationException();
            }
        }

        internal void RemoveToManyRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            switch (roleType)
            {
            case EmbeddedOneToManyRoleType toManyRoleType:
                this.RemoveOneToManyRole(association, toManyRoleType, items);
                return;

            case EmbeddedManyToManyRoleType toManyRoleType:
                this.RemoveManyToManyRole(association, toManyRoleType, items);
                return;

            default:
                throw new InvalidOperationException();
            }
        }

        internal IEmbeddedObject? GetToOneAssociation(IEmbeddedObject role, IEmbeddedCompositeAssociationType associationType)
        {
            if (!this.changedRelations.TryGetAssociation(role, associationType, out var association))
            {
                this.relations.AssociationByRole(associationType).TryGetValue(role, out association);
            }

            return (IEmbeddedObject?)association;
        }

        internal IImmutableSet<IEmbeddedObject>? GetToManyAssociation(IEmbeddedObject role, IEmbeddedCompositeAssociationType associationType)
        {
            if (!this.changedRelations.TryGetAssociation(role, associationType, out var association))
            {
                this.relations.AssociationByRole(associationType).TryGetValue(role, out association);
            }

            return (IImmutableSet<IEmbeddedObject>?)association;
        }

        private void SetOneToOneRole(IEmbeddedObject association, EmbeddedOneToOneRoleType roleType, object value)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var role = this.Normalize(roleType, value);
            var previousRole = this.GetRole(association, roleType);

            // R = PR
            if (Equals(role, previousRole))
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // A --x-- PR
            if (previousRole != null)
            {
                this.RemoveOneToOneRole(association, roleType);
            }

            var roleAssociation = this.GetToOneAssociation(role, associationType);

            // RA --x-- R
            if (roleAssociation != null)
            {
                this.RemoveOneToOneRole(roleAssociation, roleType);
            }

            // A <---- R
            this.SetOneToAssociation(role, associationType, association);

            // A ----> R
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            changedRoleByAssociation[association] = role;
        }

        private void RemoveOneToOneRole(IEmbeddedObject association, EmbeddedOneToOneRoleType roleType)
        {
            /*                        delete
             *
             *   A ----- R    ->     A       R  =   A       R
             */

            var previousRole = (IEmbeddedObject?)this.GetRole(association, roleType);
            if (previousRole == null)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // A <---- R
            this.RemoveOneToAssociation(previousRole, associationType);

            // A ----> R
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            changedRoleByAssociation[association] = null;
        }

        private void SetManyToOneRole(IEmbeddedObject association, EmbeddedManyToOneRoleType roleType, object value)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var role = this.Normalize(roleType, value);

            var associationType = roleType.AssociationType;
            var previousRole = this.GetToOneRole(association, roleType);

            // R = PR
            if (role.Equals(previousRole))
            {
                return;
            }

            // A --x-- PR
            if (previousRole != null)
            {
                this.RemoveManyToAssociation(previousRole, associationType, association);
            }

            // A <---- R
            this.AddManyToAssociation(role, associationType, association);

            // A ----> R
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            changedRoleByAssociation[association] = role;
        }

        private void RemoveManyToOneRole(IEmbeddedObject association, EmbeddedManyToOneRoleType roleType)
        {
            /*                        delete
             *  RA --                                RA --
             *       -        ->                 =        -
             *   A ----- R           A --x-- R             -- R
             */

            var previousRole = (IEmbeddedObject?)this.GetRole(association, roleType);
            if (previousRole == null)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // A <---- R
            this.RemoveManyToAssociation(previousRole, associationType, association);

            // A ----> R
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            changedRoleByAssociation[association] = null;
        }

        private void SetOneToManyRole(IEmbeddedObject association, EmbeddedOneToManyRoleType roleType, IEmbeddedObject[] role)
        {
            var previousRole = this.GetToManyRole(association, roleType);

            if (previousRole == null)
            {
                foreach (var addedRole in role)
                {
                    this.AddOneToManyRole(association, roleType, addedRole);
                }
            }
            else
            {
                // Use Diff (Add/Remove)
                foreach (var addedRole in role.Except(previousRole))
                {
                    this.AddOneToManyRole(association, roleType, addedRole);
                }

                foreach (var removeRole in previousRole.Except(role))
                {
                    this.RemoveOneToManyRole(association, roleType, removeRole);
                }
            }
        }

        private void AddOneToManyRole(IEmbeddedObject association, EmbeddedOneToManyRoleType roleType, IEmbeddedObject role)
        {
            /*  [if exist]        [then remove]        add
             *
             *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
             *                ->                +        -        =       -
             *   A ----- PR         A       PR       A --    PR       A ----- PR
             */

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);

            // R in PR
            if (previousRole?.Contains(role) == true)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // RA --x-- R
            var roleAssociation = this.GetToOneAssociation(role, associationType);

            if (roleAssociation != null)
            {
                this.RemoveOneToManyRole(roleAssociation, roleType, role);
            }

            // A <---- R
            this.SetOneToAssociation(role, associationType, association);

            // A ----> R
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            changedRoleByAssociation[association] = previousRole != null ? previousRole.Add(role) : ImmutableHashSet.Create(role);
        }

        private void RemoveOneToManyRole(IEmbeddedObject association, EmbeddedOneToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole?.Overlaps(items) == true)
            {
                // Role
                var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
                changedRoleByAssociation[association] = previousRole.Except(items);

                // Association
                var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);

                foreach (var item in items)
                {
                    if (associationType.IsOne)
                    {
                        // One to Many
                        changedAssociationByRole.Remove(item);
                    }
                    else
                    {
                        var previousAssociation = this.GetToManyAssociation(item, associationType);

                        // Many to Many
                        if (previousAssociation?.Contains(association) == true)
                        {
                            changedAssociationByRole[item] = previousAssociation.Remove(association);
                        }
                    }
                }
            }
        }

        private void AddOneToManyRole(IEmbeddedObject association, EmbeddedOneToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // Role
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            changedRoleByAssociation[association] = previousRole != null ? previousRole.Union(items) : ImmutableHashSet.Create(items);

            // Association
            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            foreach (var item in items)
            {
                if (associationType.IsOne)
                {
                    var previousAssociation = this.GetToOneAssociation(item, associationType);

                    // One to Many
                    if (previousAssociation != null)
                    {
                        var previousAssociationRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(previousAssociation, roleType);
                        if (previousAssociationRole?.Contains(item) == true)
                        {
                            changedRoleByAssociation[previousAssociation] = previousAssociationRole.Remove(item);
                        }
                    }

                    changedAssociationByRole[item] = association;
                }
                else
                {
                    var previousAssociation = this.GetToManyAssociation(item, associationType);

                    // Many to Many
                    changedAssociationByRole[item] = previousAssociation != null ? previousAssociation.Add(association) : ImmutableHashSet.Create(association);
                }
            }
        }

        private void RemoveOneToManyRole(IEmbeddedObject association, EmbeddedOneToManyRoleType roleType)
        {
            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole != null)
            {
                // Role
                var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
                changedRoleByAssociation.Remove(association);

                // Association
                var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
                foreach (var role in previousRole)
                {
                    if (associationType.IsOne)
                    {
                        // One to Many
                        changedAssociationByRole[role] = null;
                    }
                    else
                    {
                        var previousAssociation = this.GetToManyAssociation(role, associationType);

                        // Many to Many
                        if (previousAssociation?.Contains(association) == true)
                        {
                            changedAssociationByRole[role] = previousAssociation.Remove(association);
                        }
                    }
                }
            }
        }

        private void RemoveOneToManyRole(IEmbeddedObject association, EmbeddedOneToManyRoleType roleType, IEmbeddedObject item)
        {
            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole?.Contains(item) == true)
            {
                // Role
                var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
                changedRoleByAssociation[association] = previousRole.Remove(item);

                // Association
                var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
                if (associationType.IsOne)
                {
                    // One to Many
                    changedAssociationByRole.Remove(item);
                }
                else
                {
                    var previousAssociation = this.GetToManyAssociation(item, associationType);

                    // Many to Many
                    if (previousAssociation?.Contains(association) == true)
                    {
                        changedAssociationByRole[item] = previousAssociation.Remove(association);
                    }
                }
            }
        }

        private void SetManyToManyRole(IEmbeddedObject association, EmbeddedManyToManyRoleType roleType, IEmbeddedObject[] normalizedRole)
        {
            var previousRole = this.GetRole(association, roleType);

            var roles = normalizedRole.ToArray();
            var previousRoles = (IImmutableSet<IEmbeddedObject>?)previousRole;

            if (previousRoles != null)
            {
                // Use Diff (Add/Remove)
                var addedRoles = roles.Except(previousRoles);
                var removedRoles = previousRoles.Except(roles);

                foreach (var addedRole in addedRoles)
                {
                    this.AddManyToManyRole(association, roleType, addedRole);
                }

                foreach (var removeRole in removedRoles)
                {
                    this.RemoveManyToManyRole(association, roleType, removeRole);
                }
            }
            else
            {
                foreach (var addedRole in roles)
                {
                    this.AddManyToManyRole(association, roleType, addedRole);
                }
            }
        }

        private void AddManyToManyRole(IEmbeddedObject association, EmbeddedManyToManyRoleType roleType, IEmbeddedObject item)
        {
            var associationType = roleType.AssociationType;

            // Role
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            var newRole = previousRole != null ? previousRole.Add(item) : ImmutableHashSet.Create(item);
            changedRoleByAssociation[association] = newRole;

            // Association
            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            if (associationType.IsOne)
            {
                var previousAssociation = this.GetToOneAssociation(item, associationType);

                // One to Many
                if (previousAssociation != null)
                {
                    var previousAssociationRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(previousAssociation, roleType);
                    if (previousAssociationRole?.Contains(item) == true)
                    {
                        changedRoleByAssociation[previousAssociation] = previousAssociationRole.Remove(item);
                    }
                }

                changedAssociationByRole[item] = association;
            }
            else
            {
                var previousAssociation = this.GetToManyAssociation(item, associationType);

                // Many to Many
                changedAssociationByRole[item] = previousAssociation != null ? previousAssociation.Add(association) : ImmutableHashSet.Create(association);
            }
        }

        private void AddManyToManyRole(IEmbeddedObject association, EmbeddedManyToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // Role
            var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            changedRoleByAssociation[association] = previousRole != null ? previousRole.Union(items) : ImmutableHashSet.Create(items);

            // Association
            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            foreach (var item in items)
            {
                if (associationType.IsOne)
                {
                    var previousAssociation = this.GetToOneAssociation(item, associationType);

                    // One to Many
                    if (previousAssociation != null)
                    {
                        var previousAssociationRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(previousAssociation, roleType);
                        if (previousAssociationRole?.Contains(item) == true)
                        {
                            changedRoleByAssociation[previousAssociation] = previousAssociationRole.Remove(item);
                        }
                    }

                    changedAssociationByRole[item] = association;
                }
                else
                {
                    var previousAssociation = this.GetToManyAssociation(item, associationType);

                    // Many to Many
                    changedAssociationByRole[item] = previousAssociation != null ? previousAssociation.Add(association) : ImmutableHashSet.Create(association);
                }
            }
        }

        private void RemoveManyToManyRole(IEmbeddedObject association, EmbeddedManyToManyRoleType roleType, IEmbeddedObject item)
        {
            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole?.Contains(item) == true)
            {
                // Role
                var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
                changedRoleByAssociation[association] = previousRole.Remove(item);

                // Association
                var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
                if (associationType.IsOne)
                {
                    // One to Many
                    changedAssociationByRole.Remove(item);
                }
                else
                {
                    var previousAssociation = this.GetToManyAssociation(item, associationType);

                    // Many to Many
                    if (previousAssociation?.Contains(association) == true)
                    {
                        changedAssociationByRole[item] = previousAssociation.Remove(association);
                    }
                }
            }
        }

        private void RemoveManyToManyRole(IEmbeddedObject association, EmbeddedManyToManyRoleType roleType)
        {
            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole != null)
            {
                // Role
                var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
                changedRoleByAssociation.Remove(association);

                // Association
                var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
                foreach (var role in previousRole)
                {
                    if (associationType.IsOne)
                    {
                        // One to Many
                        changedAssociationByRole[role] = null;
                    }
                    else
                    {
                        var previousAssociation = this.GetToManyAssociation(role, associationType);

                        // Many to Many
                        if (previousAssociation?.Contains(association) == true)
                        {
                            changedAssociationByRole[role] = previousAssociation.Remove(association);
                        }
                    }
                }
            }
        }

        private void RemoveManyToManyRole(IEmbeddedObject association, EmbeddedManyToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole?.Overlaps(items) == true)
            {
                // Role
                var changedRoleByAssociation = this.changedRelations.RoleByAssociation(roleType);
                changedRoleByAssociation[association] = previousRole.Except(items);

                // Association
                var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);

                foreach (var item in items)
                {
                    if (associationType.IsOne)
                    {
                        // One to Many
                        changedAssociationByRole.Remove(item);
                    }
                    else
                    {
                        var previousAssociation = this.GetToManyAssociation(item, associationType);

                        // Many to Many
                        if (previousAssociation?.Contains(association) == true)
                        {
                            changedAssociationByRole[item] = previousAssociation.Remove(association);
                        }
                    }
                }
            }
        }

        private void SetOneToAssociation(IEmbeddedObject role, IEmbeddedOneToAssociationType associationType, IEmbeddedObject association)
        {
            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            changedAssociationByRole[role] = association;
        }

        private void RemoveOneToAssociation(IEmbeddedObject role, IEmbeddedOneToAssociationType associationType)
        {
            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            changedAssociationByRole[role] = null;
        }

        private void AddManyToAssociation(IEmbeddedObject role, IEmbeddedManyToAssociationType associationType, IEmbeddedObject association)
        {
            var previousAssociation = this.GetToManyAssociation(role, associationType);

            if (previousAssociation?.Contains(role) != true)
            {
                return;
            }

            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            changedAssociationByRole[role] = previousAssociation.Remove(association);
        }

        private void RemoveManyToAssociation(IEmbeddedObject role, IEmbeddedManyToAssociationType associationType, IEmbeddedObject association)
        {
            var previousAssociation = this.GetToManyAssociation(role, associationType);

            if (previousAssociation == null || previousAssociation.Contains(role))
            {
                return;
            }

            var changedAssociationByRole = this.changedRelations.AssociationByRole(associationType);
            changedAssociationByRole[role] = previousAssociation.Remove(association);
        }

        private object? GetRole(IEmbeddedObject association, IEmbeddedRoleType roleType)
        {
            if (!this.changedRelations.TryGetRole(association, roleType, out var role))
            {
                this.relations.RoleByAssociation(roleType).TryGetValue(association, out role);
            }

            return role;
        }

        private object? Normalize(EmbeddedUnitRoleType roleType, object? value)
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

            if (value.GetType() != roleType.ObjectType.Type && roleType.ObjectType.TypeCode.HasValue)
            {
                value = Convert.ChangeType(value, roleType.ObjectType.TypeCode.Value, CultureInfo.InvariantCulture);
            }

            return value;
        }

        private IEmbeddedObject Normalize(IEmbeddedToOneRoleType roleType, object value)
        {
            if (value is EmbeddedObject embeddedObject)
            {
                if (!roleType.ObjectType.IsAssignableFrom(embeddedObject.ObjectType))
                {
                    throw new ArgumentException($"{roleType.Name} should be assignable to {roleType.ObjectType.Name} but was a {embeddedObject.ObjectType.Name}");
                }

                return embeddedObject;
            }

            throw new ArgumentException($"{roleType.Name} should be an embedded object but was a {value.GetType()}");
        }

        private IEnumerable<IEmbeddedObject> Normalize(IEmbeddedToManyRoleType roleType, IEnumerable<IEmbeddedObject?> value)
        {
            foreach (var @object in value)
            {
                if (@object == null)
                {
                    continue;
                }

                if (@object is IEmbeddedObject embeddedObject)
                {
                    if (!roleType.ObjectType.IsAssignableFrom(embeddedObject.ObjectType))
                    {
                        throw new ArgumentException($"{roleType.Name} should be assignable to {roleType.ObjectType.Name} but was a {embeddedObject.ObjectType.Name}");
                    }
                }
                else
                {
                    throw new ArgumentException($"{roleType.Name} should be an embedded object but was a {@object.GetType()}");
                }

                yield return embeddedObject;
            }
        }
    }
}
