namespace Allors.Embedded.Domain
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Allors.Embedded.Meta;

    public sealed class EmbeddedChangeSet(
        IReadOnlyDictionary<IEmbeddedRoleType, Dictionary<EmbeddedObject, object>> roleByAssociationByRoleType,
        IReadOnlyDictionary<IEmbeddedCompositeAssociationType, Dictionary<EmbeddedObject, object>> associationByRoleByAssociationType)
    {
        private static readonly IReadOnlyDictionary<EmbeddedObject, object> Empty = new ReadOnlyDictionary<EmbeddedObject, object>(new Dictionary<EmbeddedObject, object>());

        public bool HasChanges =>
            roleByAssociationByRoleType.Any(v => v.Value.Count > 0) ||
            associationByRoleByAssociationType.Any(v => v.Value.Count > 0);

        public IReadOnlyDictionary<EmbeddedObject, object> ChangedRoles(EmbeddedObjectType objectType, string name)
        {
            var roleType = objectType.RoleTypeByName[name];
            return this.ChangedRoles(roleType);
        }

        public IReadOnlyDictionary<EmbeddedObject, object> ChangedRoles(IEmbeddedRoleType roleType)
        {
            roleByAssociationByRoleType.TryGetValue(roleType, out var changedRelations);
            return changedRelations ?? Empty;
        }
    }
}
