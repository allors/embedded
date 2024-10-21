namespace Allors.Embedded.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Embedded.Meta;

    internal sealed class EmbeddedRelations
    {
        private Dictionary<IEmbeddedRoleType, Dictionary<IEmbeddedObject, object?>> roleByAssociationByRoleType = [];

        private Dictionary<IEmbeddedCompositeAssociationType, Dictionary<IEmbeddedObject, object?>> associationByRoleByAssociationType = [];

        internal Dictionary<IEmbeddedObject, object?> AssociationByRole(IEmbeddedCompositeAssociationType associationType)
        {
            if (!this.associationByRoleByAssociationType.TryGetValue(associationType, out var associationByRole))
            {
                associationByRole = [];
                this.associationByRoleByAssociationType[associationType] = associationByRole;
            }

            return associationByRole;
        }

        internal Dictionary<IEmbeddedObject, object?> RoleByAssociation(IEmbeddedRoleType roleType)
        {
            if (!this.roleByAssociationByRoleType.TryGetValue(roleType, out var roleByAssociation))
            {
                roleByAssociation = [];
                this.roleByAssociationByRoleType[roleType] = roleByAssociation;
            }

            return roleByAssociation;
        }

        internal EmbeddedChangeSet Snapshot(EmbeddedRelations relations)
        {
            foreach (var roleType in this.roleByAssociationByRoleType.Keys.ToArray())
            {
                var changedRoleByAssociation = this.roleByAssociationByRoleType[roleType];
                var roleByAssociation = relations.RoleByAssociation(roleType);

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
                    this.roleByAssociationByRoleType.Remove(roleType);
                }
            }

            foreach (var associationType in this.associationByRoleByAssociationType.Keys.ToArray())
            {
                var changedAssociationByRole = this.associationByRoleByAssociationType[associationType];
                var associationByRole = relations.AssociationByRole(associationType);

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
                    this.associationByRoleByAssociationType.Remove(associationType);
                }
            }

            var snapshot = new EmbeddedChangeSet(this.roleByAssociationByRoleType, this.associationByRoleByAssociationType);

            foreach (var kvp in this.roleByAssociationByRoleType)
            {
                var roleType = kvp.Key;
                var changedRoleByAssociation = kvp.Value;

                var roleByAssociation = relations.RoleByAssociation(roleType);

                foreach (var kvp2 in changedRoleByAssociation)
                {
                    var association = kvp2.Key;
                    var changedRole = kvp2.Value;
                    roleByAssociation[association] = changedRole;
                }
            }

            foreach (var kvp in this.associationByRoleByAssociationType)
            {
                var associationType = kvp.Key;
                var changedAssociationByRole = kvp.Value;

                var associationByRole = relations.AssociationByRole(associationType);

                foreach (var kvp2 in changedAssociationByRole)
                {
                    var role = kvp2.Key;
                    var changedAssociation = kvp2.Value;
                    associationByRole[role] = changedAssociation;
                }
            }

            this.roleByAssociationByRoleType = [];
            this.associationByRoleByAssociationType = [];

            return snapshot;
        }

        internal bool TryGetRole(IEmbeddedObject association, IEmbeddedRoleType roleType, out object? role)
        {
            if (this.roleByAssociationByRoleType.TryGetValue(roleType, out var roleByAssociation) && roleByAssociation.TryGetValue(association, out role))
            {
                return true;
            }

            role = null;
            return false;
        }

        internal bool TryGetAssociation(IEmbeddedObject role, IEmbeddedCompositeAssociationType associationType, out object? association)
        {
            if (this.associationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole) && changedAssociationByRole.TryGetValue(role, out association))
            {
                return true;
            }

            association = null;
            return false;
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
    }
}
