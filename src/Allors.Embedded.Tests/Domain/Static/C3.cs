﻿namespace Allors.Embedded.Tests.Domain.Static
{
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;

    public class C3(EmbeddedPopulation population, EmbeddedObjectType objectType)
        : EmbeddedObject(population, objectType), I1;
}
