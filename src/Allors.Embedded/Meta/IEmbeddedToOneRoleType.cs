namespace Allors.Embedded.Meta
{
    using System;
    using Allors.Embedded.Domain;

    public interface IEmbeddedToOneRoleType : IEmbeddedCompositeRoleType
    {
        public object? Normalize(object? value)
        {
            if (value is not null)
            {
                if (value is EmbeddedObject embeddedObject)
                {
                    if (!this.ObjectType.IsAssignableFrom(embeddedObject.ObjectType))
                    {
                        throw new ArgumentException($"{this.Name} should be assignable to {this.ObjectType.Name} but was a {embeddedObject.ObjectType.Name}");
                    }
                }
                else
                {
                    throw new ArgumentException($"{this.Name} should be an embedded object but was a {value.GetType()}");
                }
            }

            return value;
        }
    }
}
