namespace Allors.Embedded.Domain
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Allors.Embedded.Meta;

    public sealed class EmbeddedChangeSet(
        IReadOnlyDictionary<IEmbeddedRoleType, Dictionary<IEmbeddedObject, object?>> roleByAssociationByRoleType,
        IReadOnlyDictionary<IEmbeddedCompositeAssociationType, Dictionary<IEmbeddedObject, object?>> associationByRoleByAssociationType)
    {
        private static readonly IReadOnlyDictionary<IEmbeddedObject, object?> Empty = ReadOnlyDictionary<IEmbeddedObject, object?>.Empty;

        public bool HasChanges =>
            roleByAssociationByRoleType.Any(v => v.Value.Count > 0) ||
            associationByRoleByAssociationType.Any(v => v.Value.Count > 0);

        public IReadOnlyDictionary<IEmbeddedObject, object?> ChangedRoles(EmbeddedObjectType objectType, string name)
        {
            var roleType = objectType.RoleTypeByName[name];
            return this.ChangedRoles(roleType);
        }

        public IReadOnlyDictionary<IEmbeddedObject, object?> ChangedRoles(IEmbeddedRoleType roleType)
        {
            roleByAssociationByRoleType.TryGetValue(roleType, out var changedRelations);
            return changedRelations ?? Empty;
        }
    }
}
