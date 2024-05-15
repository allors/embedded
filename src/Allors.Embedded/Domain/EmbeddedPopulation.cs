namespace Allors.Embedded.Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Allors.Embedded.Meta;

    public sealed class EmbeddedPopulation(EmbeddedMeta meta)
    {
        private readonly Dictionary<IEmbeddedRoleType, Dictionary<IEmbeddedObject, object>> roleByAssociationByRoleType = [];
        private readonly Dictionary<IEmbeddedCompositeAssociationType, Dictionary<IEmbeddedObject, object>> associationByRoleByAssociationType = [];

        private Dictionary<IEmbeddedRoleType, Dictionary<IEmbeddedObject, object>> changedRoleByAssociationByRoleType = [];
        private Dictionary<IEmbeddedCompositeAssociationType, Dictionary<IEmbeddedObject, object>> changedAssociationByRoleByAssociationType = [];

        private IImmutableList<IEmbeddedObject> objects = ImmutableArray<IEmbeddedObject>.Empty;

        public EmbeddedMeta Meta { get; } = meta;

        public Dictionary<string, IEmbeddedDerivation> DerivationById { get; } = [];

        public IReadOnlyList<IEmbeddedObject> Objects => this.objects;

        public IEmbeddedObject Create(EmbeddedObjectType @class, params Action<IEmbeddedObject>[] builders)
        {
            var @new = @class.Type != null ? (IEmbeddedObject)Activator.CreateInstance(@class.Type, this, @class)! : new EmbeddedObject(this, @class);
            this.objects = this.objects.Add(@new);

            foreach (var builder in builders)
            {
                builder(@new);
            }

            return @new;
        }

        public IEmbeddedObject Create<T>(params Action<T>[] builders)
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

        public EmbeddedChangeSet Snapshot()
        {
            foreach (var roleType in this.changedRoleByAssociationByRoleType.Keys.ToArray())
            {
                var changedRoleByAssociation = this.changedRoleByAssociationByRoleType[roleType];
                var roleByAssociation = this.RoleByAssociation(roleType);

                foreach (var association in changedRoleByAssociation.Keys.ToArray())
                {
                    var role = changedRoleByAssociation[association];
                    roleByAssociation.TryGetValue(association, out var originalRole);

                    var compositeRoleType = roleType as IEmbeddedCompositeRoleType;

                    var areEqual = ReferenceEquals(originalRole, role) ||
                                   (compositeRoleType?.IsOne == true && Equals(originalRole, role)) ||
                                   (compositeRoleType?.IsMany == true && Same(originalRole, role));

                    if (areEqual)
                    {
                        changedRoleByAssociation.Remove(association);
                        continue;
                    }

                    roleByAssociation[association] = role;
                }

                if (roleByAssociation.Count == 0)
                {
                    this.changedRoleByAssociationByRoleType.Remove(roleType);
                }
            }

            foreach (var associationType in this.changedAssociationByRoleByAssociationType.Keys.ToArray())
            {
                var changedAssociationByRole = this.changedAssociationByRoleByAssociationType[associationType];
                var associationByRole = this.AssociationByRole(associationType);

                foreach (var role in changedAssociationByRole.Keys.ToArray())
                {
                    var changedAssociation = changedAssociationByRole[role];
                    associationByRole.TryGetValue(role, out var originalAssociation);

                    var areEqual = ReferenceEquals(originalAssociation, changedAssociation) ||
                                   (associationType.IsOne && Equals(originalAssociation, changedAssociation)) ||
                                   (associationType.IsMany && Same(originalAssociation, changedAssociation));

                    if (areEqual)
                    {
                        changedAssociationByRole.Remove(role);
                        continue;
                    }

                    associationByRole[role] = changedAssociation;
                }

                if (associationByRole.Count == 0)
                {
                    this.changedAssociationByRoleByAssociationType.Remove(associationType);
                }
            }

            var snapshot = new EmbeddedChangeSet(this.changedRoleByAssociationByRoleType, this.changedAssociationByRoleByAssociationType);

            foreach (var kvp in this.changedRoleByAssociationByRoleType)
            {
                var roleType = kvp.Key;
                var changedRoleByAssociation = kvp.Value;

                var roleByAssociation = this.RoleByAssociation(roleType);

                foreach (var kvp2 in changedRoleByAssociation)
                {
                    var association = kvp2.Key;
                    var changedRole = kvp2.Value;
                    roleByAssociation[association] = changedRole;
                }
            }

            foreach (var kvp in this.changedAssociationByRoleByAssociationType)
            {
                var associationType = kvp.Key;
                var changedAssociationByRole = kvp.Value;

                var associationByRole = this.AssociationByRole(associationType);

                foreach (var kvp2 in changedAssociationByRole)
                {
                    var role = kvp2.Key;
                    var changedAssociation = kvp2.Value;
                    associationByRole[role] = changedAssociation;
                }
            }

            this.changedRoleByAssociationByRoleType = [];
            this.changedAssociationByRoleByAssociationType = [];

            return snapshot;
        }

        public void Derive()
        {
            var changeSet = this.Snapshot();

            while (changeSet.HasChanges)
            {
                foreach (var derivation in this.DerivationById.Select(kvp => kvp.Value))
                {
                    derivation.Derive(changeSet);
                }

                changeSet = this.Snapshot();
            }
        }

        internal object? GetRole(IEmbeddedObject association, IEmbeddedRoleType roleType)
        {
            if (this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation) &&
                changedRoleByAssociation.TryGetValue(association, out var role))
            {
                return role;
            }

            this.RoleByAssociation(roleType).TryGetValue(association, out role);
            return role;
        }

        internal void SetUnitRole(IEmbeddedObject association, EmbeddedUnitRoleType roleType, object? role)
        {
            var normalizedRole = roleType.Normalize(role);

            if (normalizedRole == null)
            {
                this.RemoveUnitRole(association, roleType);
                return;
            }

            // Role
            this.ChangedRoleByAssociation(roleType)[association] = normalizedRole;
        }

        internal void SetToOneRole(IEmbeddedObject association, IEmbeddedToOneRoleType roleType, object? role)
        {
            var normalizedRole = roleType.Normalize(role);

            if (normalizedRole == null)
            {
                this.RemoveCompositeRole(association, roleType);
                return;
            }

            var associationType = roleType.AssociationType;
            var previousRole = this.GetRole(association, roleType);

            var roleObject = (IEmbeddedObject)normalizedRole;

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            changedRoleByAssociation[association] = roleObject;

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            if (associationType.IsOne)
            {
                var previousAssociation = this.GetAssociation(roleObject, associationType);

                // One to One
                var previousAssociationObject = (IEmbeddedObject?)previousAssociation;
                if (previousAssociationObject != null)
                {
                    changedRoleByAssociation.Remove(previousAssociationObject);
                }

                if (previousRole != null)
                {
                    var previousRoleObject = (IEmbeddedObject)previousRole;
                    changedAssociationByRole.Remove(previousRoleObject);
                }

                changedAssociationByRole[roleObject] = association;
            }
            else
            {
                // Many to One
                var previousAssociation = (IImmutableSet<IEmbeddedObject>?)this.GetAssociation(roleObject, associationType);
                if (previousAssociation?.Contains(roleObject) == true)
                {
                    changedAssociationByRole[roleObject] = previousAssociation.Remove(roleObject);
                }
            }
        }

        internal void SetToManyRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, object? role)
        {
            var normalizedRole = roleType.Normalize(role);

            if (normalizedRole == null)
            {
                this.RemoveCompositeRole(association, roleType);
                return;
            }

            var previousRole = this.GetRole(association, roleType);

            var roles = ((IEnumerable)normalizedRole).Cast<IEmbeddedObject>().ToArray();
            var previousRoles = (IImmutableSet<IEmbeddedObject>?)previousRole;

            if (previousRoles != null)
            {
                // Use Diff (Add/Remove)
                var addedRoles = roles.Except(previousRoles);
                var removedRoles = previousRoles.Except(roles);

                foreach (var addedRole in addedRoles)
                {
                    this.AddRole(association, roleType, addedRole);
                }

                foreach (var removeRole in removedRoles)
                {
                    this.RemoveRole(association, roleType, removeRole);
                }
            }
            else
            {
                foreach (var addedRole in roles)
                {
                    this.AddRole(association, roleType, addedRole);
                }
            }
        }

        internal void AddRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject item)
        {
            var associationType = roleType.AssociationType;

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            var newRole = previousRole != null ? previousRole.Add(item) : ImmutableHashSet.Create(item);
            changedRoleByAssociation[association] = newRole;

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            if (associationType.IsOne)
            {
                var previousAssociation = (IEmbeddedObject?)this.GetAssociation(item, associationType);

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
                var previousAssociation = (IImmutableSet<IEmbeddedObject>?)this.GetAssociation(item, associationType);

                // Many to Many
                changedAssociationByRole[item] = previousAssociation != null ? previousAssociation.Add(association) : ImmutableHashSet.Create(association);
            }
        }

        internal void AddRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            changedRoleByAssociation[association] = previousRole != null ? previousRole.Union(items) : ImmutableHashSet.Create(items);

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            foreach (var item in items)
            {
                if (associationType.IsOne)
                {
                    var previousAssociation = (IEmbeddedObject?)this.GetAssociation(item, associationType);

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
                    var previousAssociation = (IImmutableSet<IEmbeddedObject>?)this.GetAssociation(item, associationType);

                    // Many to Many
                    changedAssociationByRole[item] = previousAssociation != null ? previousAssociation.Add(association) : ImmutableHashSet.Create(association);
                }
            }
        }

        internal void RemoveRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject item)
        {
            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole?.Contains(item) == true)
            {
                // Role
                var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                changedRoleByAssociation[association] = previousRole.Remove(item);

                // Association
                var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
                if (associationType.IsOne)
                {
                    // One to Many
                    changedAssociationByRole.Remove(item);
                }
                else
                {
                    var previousAssociation = (IImmutableSet<IEmbeddedObject>?)this.GetAssociation(item, associationType);

                    // Many to Many
                    if (previousAssociation?.Contains(association) == true)
                    {
                        changedAssociationByRole[item] = previousAssociation.Remove(association);
                    }
                }
            }
        }

        internal void RemoveRole(IEmbeddedObject association, IEmbeddedToManyRoleType roleType, IEmbeddedObject[]? items)
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
                var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                changedRoleByAssociation[association] = previousRole.Except(items);

                // Association
                var changedAssociationByRole = this.ChangedAssociationByRole(associationType);

                foreach (var item in items)
                {
                    if (associationType.IsOne)
                    {
                        // One to Many
                        changedAssociationByRole.Remove(item);
                    }
                    else
                    {
                        var previousAssociation = (IImmutableSet<IEmbeddedObject>?)this.GetAssociation(item, associationType);

                        // Many to Many
                        if (previousAssociation?.Contains(association) == true)
                        {
                            changedAssociationByRole[item] = previousAssociation.Remove(association);
                        }
                    }
                }
            }
        }

        internal object? GetAssociation(IEmbeddedObject role, IEmbeddedCompositeAssociationType associationType)
        {
            if (this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole) &&
                changedAssociationByRole.TryGetValue(role, out var association))
            {
                return association;
            }

            this.AssociationByRole(associationType).TryGetValue(role, out association);
            return association;
        }

        private static bool Same(object? source, object? destination)
        {
            if (source == null && destination == null)
            {
                return true;
            }

            if (source == null || destination == null)
            {
                return false;
            }

            if (source is IReadOnlySet<IEmbeddedObject> sourceSet)
            {
                return sourceSet.SetEquals((IEnumerable<IEmbeddedObject>)destination);
            }

            var destinationSet = (IReadOnlySet<IEmbeddedObject>)destination;
            return destinationSet.SetEquals((IEnumerable<IEmbeddedObject>)source);
        }

        private Dictionary<IEmbeddedObject, object> AssociationByRole(IEmbeddedCompositeAssociationType associationType)
        {
            if (!this.associationByRoleByAssociationType.TryGetValue(associationType, out var associationByRole))
            {
                associationByRole = [];
                this.associationByRoleByAssociationType[associationType] = associationByRole;
            }

            return associationByRole;
        }

        private Dictionary<IEmbeddedObject, object> RoleByAssociation(IEmbeddedRoleType roleType)
        {
            if (!this.roleByAssociationByRoleType.TryGetValue(roleType, out var roleByAssociation))
            {
                roleByAssociation = [];
                this.roleByAssociationByRoleType[roleType] = roleByAssociation;
            }

            return roleByAssociation;
        }

        private Dictionary<IEmbeddedObject, object> ChangedAssociationByRole(IEmbeddedCompositeAssociationType associationType)
        {
            if (!this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole))
            {
                changedAssociationByRole = [];
                this.changedAssociationByRoleByAssociationType[associationType] = changedAssociationByRole;
            }

            return changedAssociationByRole;
        }

        private Dictionary<IEmbeddedObject, object> ChangedRoleByAssociation(IEmbeddedRoleType roleType)
        {
            if (!this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation))
            {
                changedRoleByAssociation = [];
                this.changedRoleByAssociationByRoleType[roleType] = changedRoleByAssociation;
            }

            return changedRoleByAssociation;
        }

        private void RemoveUnitRole(IEmbeddedObject association, EmbeddedUnitRoleType roleType)
        {
            var previousRole = this.GetRole(association, roleType);
            if (previousRole != null)
            {
                // Role
                var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                changedRoleByAssociation.Remove(association);
            }
        }

        private void RemoveCompositeRole(IEmbeddedObject association, IEmbeddedCompositeRoleType roleType)
        {
            var associationType = roleType.AssociationType;

            var previousRole = (IImmutableSet<IEmbeddedObject>?)this.GetRole(association, roleType);
            if (previousRole != null)
            {
                // Role
                var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                changedRoleByAssociation.Remove(association);

                // Association
                var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
                foreach (var role in previousRole)
                {
                    if (associationType.IsOne)
                    {
                        // One to Many
                        changedAssociationByRole.Remove(role);
                    }
                    else
                    {
                        var previousAssociation = (IImmutableSet<IEmbeddedObject>?)this.GetAssociation(role, associationType);

                        // Many to Many
                        if (previousAssociation?.Contains(association) == true)
                        {
                            changedAssociationByRole[role] = previousAssociation.Remove(association);
                        }
                    }
                }
            }
        }
    }
}
