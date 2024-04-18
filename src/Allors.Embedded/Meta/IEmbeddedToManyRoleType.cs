namespace Allors.Embedded.Meta
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Embedded.Domain;

    public interface IEmbeddedToManyRoleType : IEmbeddedCompositeRoleType
    {
        public object? Normalize(object? value)
        {
            return value switch
            {
                null => null,
                ICollection collection => this.Normalize(collection).ToArray(),
                _ => throw new ArgumentException($"{value.GetType()} is not a collection Type"),
            };
        }

        private IEnumerable<EmbeddedObject> Normalize(ICollection role)
        {
            foreach (var @object in role)
            {
                if (@object != null)
                {
                    if (@object is EmbeddedObject embeddedObject)
                    {
                        if (!this.ObjectType.IsAssignableFrom(embeddedObject.ObjectType))
                        {
                            throw new ArgumentException($"{this.Name} should be assignable to {this.ObjectType.Name} but was a {embeddedObject.ObjectType.Name}");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"{this.Name} should be an embedded object but was a {@object.GetType()}");
                    }

                    yield return embeddedObject;
                }
            }
        }
    }
}
