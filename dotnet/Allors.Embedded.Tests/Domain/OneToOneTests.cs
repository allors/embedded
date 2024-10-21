namespace Allors.Embedded.Tests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Allors.Embedded.Domain;
    using Allors.Embedded.Meta;
    using Allors.Embedded.Tests.Domain.Static;
    using MoreLinq;
    using Xunit;

    public class OneToOneTests
    {
        private readonly Func<(
               EmbeddedOneToOneAssociationType Association,
               EmbeddedOneToOneRoleType Role,
               Func<EmbeddedPopulation, IEmbeddedObject>[] Builders,
               Func<EmbeddedPopulation, IEmbeddedObject> FromBuilder,
               Func<EmbeddedPopulation, IEmbeddedObject> FromAnotherBuilder,
               Func<EmbeddedPopulation, IEmbeddedObject> ToBuilder,
               Func<EmbeddedPopulation, IEmbeddedObject> ToAnotherBuilder)>[] fixtures;

        private readonly Action<EmbeddedPopulation>[] preActs;

        public OneToOneTests()
        {
            this.Meta = new Meta();

            this.fixtures =
            [
                () =>
                {
                    // C1 <-> C1
                    var association = this.Meta.C1WhereC1OneToOne;
                    var role = this.Meta.C1C1OneToOne;

                    return (association, role, [C1Builder], C1Builder, C1Builder, C1Builder, C1Builder);

                    IEmbeddedObject C1Builder(EmbeddedPopulation transaction) => transaction.Build(this.Meta.C1);
                },
                () =>
                {
                    // C1 <-> I1
                    var association = this.Meta.C1WhereI1OneToOne;
                    var role = this.Meta.C1I1OneToOne;

                    return (association, role, [C1Builder], C1Builder, C1Builder, C1Builder, C1Builder);

                    IEmbeddedObject C1Builder(EmbeddedPopulation transaction) => transaction.Build(this.Meta.C1);
                },
                () =>
                {
                    // C1 <-> C2
                    var association = this.Meta.C1WhereC2OneToOne;
                    var role = this.Meta.C1C2OneToOne;

                    return (association, role, [C1Builder, C2Builder],  C1Builder, C1Builder, C2Builder, C2Builder);

                    IEmbeddedObject C1Builder(EmbeddedPopulation transaction) => transaction.Build(this.Meta.C1);
                    IEmbeddedObject C2Builder(EmbeddedPopulation transaction) => transaction.Build(this.Meta.C2);
                },
                () =>
                {
                    // C1 <-> I2
                    var association = this.Meta.C1WhereI2OneToOne;
                    var role = this.Meta.C1I2OneToOne;

                    return (association, role, [C1Builder, C2Builder],  C1Builder, C1Builder, C2Builder, C2Builder);

                    IEmbeddedObject C1Builder(EmbeddedPopulation transaction) => transaction.Build(this.Meta.C1);
                    IEmbeddedObject C2Builder(EmbeddedPopulation transaction) => transaction.Build(this.Meta.C2);
                },
            ];

            this.preActs =
            [
                _ => { },
                v => v.Checkpoint(),
                v =>
                {
                    v.Checkpoint();
                    v.Checkpoint();
                }
            ];
        }

        public Meta Meta { get; }

        [Fact]
        public void FromToInitial()
        {
            this.FromTo(
                () =>
                [
                ],
                () =>
                [
                    (association, _, _, to) => Assert.Null(to[association]),
                    (_, role, from, _) => Assert.Null(from[role])
                ]);
        }

        [Fact]
        public void FromToSet()
        {
            this.FromTo(
                () =>
                [
                    (_, role, from, to) => from[role] = to
                ],
                () =>
                [
                    (association, _, from, to) => Assert.Equal(from, to[association]),
                    (_, role, from, to) => Assert.Equal(to, from[role])
                ]);
        }

        [Fact]
        public void FromToSetReset()
        {
            this.FromTo(
                () =>
                [
                    (_, role, from, to) => from[role] = to,
                    (_, role, from, _) => from[role] = null
                ],
                () =>
                [
                    (association, _, _, to) => Assert.Null(to[association]),
                    (_, role, from, _) => Assert.Null(from[role])
                ]);
        }

        [Fact]
        public void FromToSetAndReset()
        {
            this.FromTo(
                () =>
                [
                    (_, role, from, to) =>
                    {
                        from[role] = to;
                        from[role] = null;
                    },
                ],
                () =>
                [
                    (association, _, _, to) => Assert.Null(to[association]),
                    (_, role, from, _) => Assert.Null(from[role])
                ]);
        }

        [Fact]
        public void FromFromAnotherToInitial()
        {
            this.FromFromAnotherTo(
                () =>
                [
                ],
                () =>
                [
                    (association, _, _, _, to) => Assert.Null(to[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, _) => Assert.Null(fromAnother[role])
                ]);
        }

        [Fact]
        public void FromFromAnotherToSetSet()
        {
            this.FromFromAnotherTo(
                () =>
                [
                    (_, role, from, _, to) => from[role] = to,
                    (_, role, _, fromAnother, to) => fromAnother[role] = to
                ],
                () =>
                [
                    (association, _, _, fromAnother, to) => Assert.Equal(fromAnother, to[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, to) =>
                    {
                        if (!Equals(to, fromAnother[role]))
                        {
                            Debugger.Break();
                        }

                        Assert.Equal(to, fromAnother[role]);
                    }
                ]);
        }

        [Fact]
        public void FromFromAnotherToSetAndSet()
        {
            this.FromFromAnotherTo(
                () =>
                [
                    (_, role, from, fromAnother, to) =>
                    {
                        from[role] = to;
                        fromAnother[role] = to;
                    },
                ],
                () =>
                [
                    (association, _, _, fromAnother, to) => Assert.Equal(fromAnother, to[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, to) => Assert.Equal(to, fromAnother[role])
                ]);
        }

        [Fact]
        public void FromFromAnotherToSetSetReset()
        {
            this.FromFromAnotherTo(
                () =>
                [
                    (_, role, from, _, to) => from[role] = to,
                    (_, role, _, fromAnother, to) => fromAnother[role] = to,
                    (_, role, _, fromAnother, _) => fromAnother[role] = null
                ],
                () =>
                [
                    (association, _, _, _, to) => Assert.Null(to[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, _) => Assert.Null(fromAnother[role])
                ]);
        }

        [Fact]
        public void FromFromAnotherToSetSetAndReset()
        {
            this.FromFromAnotherTo(
                () =>
                [
                    (_, role, from, fromAnother, to) =>
                    {
                        from[role] = to;
                        fromAnother[role] = to;
                        fromAnother[role] = null;
                    }
                ],
                () =>
                [
                    (association, _, _, _, to) => Assert.Null(to[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, _) => Assert.Null(fromAnother[role])
                ]);
        }

        [Fact]
        public void FromToToAnotherInitial()
        {
            this.FromToToAnother(
                () =>
                [
                ],
                () =>
                [
                    (association, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, _, _, toAnother) => Assert.Null(toAnother[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                ]);
        }

        [Fact]
        public void FromToToAnotherSetSet()
        {
            this.FromToToAnother(
                () =>
                [
                    (_, role, from, to, _) => from[role] = to,
                    (_, role, from, _, toAnother) => from[role] = toAnother
                ],
                () =>
                [
                    (association, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, from, _, toAnother) => Assert.Equal(from, toAnother[association]),
                    (_, role, from, _, toAnother) => Assert.Equal(toAnother, from[role])
                ]);
        }

        [Fact]
        public void FromToToAnotherSetAndSet()
        {
            this.FromToToAnother(
                () =>
                [
                    (_, role, from, to, toAnother) =>
                    {
                        from[role] = to;
                        from[role] = toAnother;
                    },
                ],
                () =>
                [
                    (association, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, from, _, toAnother) => Assert.Equal(from, toAnother[association]),
                    (_, role, from, _, toAnother) => Assert.Equal(toAnother, from[role])
                ]);
        }

        [Fact]
        public void FromToToAnotherSetSetReset()
        {
            this.FromToToAnother(
                () =>
                [
                    (_, role, from, to, _) => from[role] = to,
                    (_, role, from, _, toAnother) => from[role] = toAnother,
                    (_, role, from, _, _) => from[role] = null
                ],
                () =>
                [
                    (association, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, _, _, toAnother) => Assert.Null(toAnother[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                ]);
        }

        [Fact]
        public void FromToToAnotherSetSetAndReset()
        {
            this.FromToToAnother(
                () =>
                [
                    (_, role, from, to, toAnother) =>
                    {
                        from[role] = to;
                        from[role] = toAnother;
                        from[role] = null;
                    }
                ],
                () =>
                [
                    (association, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, _, _, toAnother) => Assert.Null(toAnother[association]),
                    (_, role, from, _, _) => Assert.Null(from[role]),
                ]);
        }

        [Fact]
        public void FromFromAnotherToToAnotherInitial()
        {
            this.FromFromAnotherToToAnother(
                () =>
                [
                ],
                () =>
                [
                    (association, _, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, _, _, _, toAnother) => Assert.Null(toAnother[association]),
                    (_, role, from, _, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, _, _) => Assert.Null(fromAnother[role]),
                ]);
        }

        [Fact]
        public void FromFromAnotherToToAnotherSetSetSet()
        {
            this.FromFromAnotherToToAnother(
                () =>
                [
                    (_, role, from, _, to, _) => from[role] = to,
                    (_, role, _, fromAnother, _, toAnother) => fromAnother[role] = toAnother,
                    (_, role, from, _, _, toAnother) => from[role] = toAnother,
                ],
                () =>
                [
                    (association, _, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, from, _, _, toAnother) => Assert.Equal(from, toAnother[association]),
                    (_, role, from, _, _, toAnother) => Assert.Equal(toAnother, from[role]),
                    (_, role, _, fromAnother, _, _) => Assert.Null(fromAnother[role]),
                ]);
        }

        [Fact]
        public void FromFromAnotherToToAnotherSetAndSetAndSet()
        {
            this.FromFromAnotherToToAnother(
                () =>
                [
                    (_, role, from, fromAnother, to, toAnother) =>
                    {
                        from[role] = to;
                        fromAnother[role] = toAnother;
                        from[role] = toAnother;
                    },
                ],
                () =>
                [
                    (association, _, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, from, _, _, toAnother) => Assert.Equal(from, toAnother[association]),
                    (_, role, from, _, _, toAnother) => Assert.Equal(toAnother, from[role]),
                    (_, role, _, fromAnother, _, _) => Assert.Null(fromAnother[role]),
                ]);
        }

        [Fact]
        public void FromFromAnotherToToAnotherSetSetSetReset()
        {
            this.FromFromAnotherToToAnother(
                () =>
                [
                    (_, role, from, _, to, _) => from[role] = to,
                    (_, role, _, fromAnother, _, toAnother) => fromAnother[role] = toAnother,
                    (_, role, from, _, _, toAnother) => from[role] = toAnother,
                    (_, role, from, _, _, _) => from[role] = null
                ],
                () =>
                [
                    (association, _, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, _, _, _, toAnother) => Assert.Null(toAnother[association]),
                    (_, role, from, _, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, _, _) => Assert.Null(fromAnother[role]),
                ]);
        }

        [Fact]
        public void FromFromAnotherToToAnotherSetAndSetAndSetAndReset()
        {
            this.FromFromAnotherToToAnother(
                () =>
                [
                    (_, role, from, fromAnother, to, toAnother) =>
                    {
                        from[role] = to;
                        fromAnother[role] = toAnother;
                        from[role] = toAnother;
                        from[role] = null;
                    }
                ],
                () =>
                [
                    (association, _, _, _, to, _) => Assert.Null(to[association]),
                    (association, _, _, _, _, toAnother) => Assert.Null(toAnother[association]),
                    (_, role, from, _, _, _) => Assert.Null(from[role]),
                    (_, role, _, fromAnother, _, _) => Assert.Null(fromAnother[role]),
                ]);
        }

        [Fact]
        public void BeginMiddleEndSet()
        {
            foreach (var fixture in this.fixtures)
            {
                var database = this.CreatePopulation();

                var (association, role, builders, _, _, _, _) =
                    fixture();

                if (builders.Length == 1)
                {
                    var builder = builders[0];

                    // Begin - Middle - End
                    var begin = builder(database);
                    var middle = builder(database);
                    var end = builder(database);

                    begin[role] = middle;
                    middle[role] = end;
                    begin[role] = end;

                    Assert.Null(middle[association]);
                    Assert.Equal(begin, end[association]);
                    Assert.Null(begin[association]);

                    Assert.Equal(end, begin[role]);
                    Assert.Null(middle[role]);
                    Assert.Null(end[role]);
                }
            }
        }

        [Fact]
        public void BeginMiddleEndRingSet()
        {
            foreach (var fixture in this.fixtures)
            {
                var database = this.CreatePopulation();

                var (association, role, builders, fromBuilder, fromAnotherBuilder, toBuilder, toAnotherBuilder) =
                    fixture();

                if (builders.Length == 1)
                {
                    var builder = builders[0];

                    // Begin - Middle - End
                    var begin = builder(database);
                    var middle = builder(database);
                    var end = builder(database);

                    begin[role] = middle;
                    middle[role] = end;
                    end[role] = begin;

                    Assert.Equal(begin, middle[association]);
                    Assert.Equal(middle, end[association]);
                    Assert.Equal(end, begin[association]);

                    Assert.Equal(middle, begin[role]);
                    Assert.Equal(end, middle[role]);
                    Assert.Equal(begin, end[role]);
                }
            }
        }

        private EmbeddedPopulation CreatePopulation()
        {
            return new EmbeddedPopulation(this.Meta.EmbeddedMeta);
        }

        private void FromTo(
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject>>> acts,
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject>>> asserts)
        {
            var assertPermutations = asserts().Permutations().ToArray();

            foreach (var preact in this.preActs)
            {
                foreach (var actRepeats in new[] { 1, 2 })
                {
                    foreach (var assertPermutation in assertPermutations)
                    {
                        foreach (var assertRepeats in new[] { 1, 2 })
                        {
                            foreach (var fixture in this.fixtures)
                            {
                                var (association, role, _, fromBuilder, _, toBuilder, _) = fixture();

                                var population = this.CreatePopulation();

                                var from = fromBuilder(population);
                                var to = toBuilder(population);

                                foreach (var act in acts())
                                {
                                    for (var actRepeat = 0; actRepeat < actRepeats; actRepeat++)
                                    {
                                        preact(population);
                                        act(association, role, from, to);
                                    }
                                }

                                foreach (var assert in assertPermutation)
                                {
                                    for (var assertRepeat = 0; assertRepeat < assertRepeats; assertRepeat++)
                                    {
                                        assert(association, role, from, to);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FromFromAnotherTo(
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject>>> acts,
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject>>> asserts)
        {
            var assertPermutations = asserts().Permutations().ToArray();

            foreach (var preact in this.preActs)
            {
                foreach (var snapshot in new bool[] { false, true })
                {
                    foreach (var actRepeats in new[] { 1, 2 })
                    {
                        foreach (var assertPermutation in assertPermutations)
                        {
                            foreach (var assertRepeats in new[] { 1, 2 })
                            {
                                foreach (var fixture in this.fixtures)
                                {
                                    var (association, role, _, fromBuilder, fromAnotherBuilder, toBuilder, _) =
                                        fixture();

                                    var population = this.CreatePopulation();

                                    var from = fromBuilder(population);
                                    var fromAnother = fromAnotherBuilder(population);
                                    var to = toBuilder(population);

                                    foreach (var act in acts())
                                    {
                                        for (var actRepeat = 0; actRepeat < actRepeats; actRepeat++)
                                        {
                                            preact(population);
                                            act(association, role, from, fromAnother, to);
                                        }
                                    }

                                    foreach (var assert in assertPermutation)
                                    {
                                        for (var assertRepeat = 0; assertRepeat < assertRepeats; assertRepeat++)
                                        {
                                            assert(association, role, from, fromAnother, to);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FromToToAnother(
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject>>> acts,
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject>>> asserts)
        {
            var assertPermutations = asserts().Permutations().ToArray();

            foreach (var preact in this.preActs)
            {
                foreach (var snapshot in new bool[] { false, true })
                {
                    foreach (var actRepeats in new[] { 1, 2 })
                    {
                        foreach (var assertPermutation in assertPermutations)
                        {
                            foreach (var assertRepeats in new[] { 1, 2 })
                            {
                                foreach (var fixture in this.fixtures)
                                {
                                    var (association, role, _, fromBuilder, _, toBuilder, toAnotherBuilder) = fixture();

                                    var population = this.CreatePopulation();

                                    var from = fromBuilder(population);
                                    var to = toBuilder(population);
                                    var toAnother = toAnotherBuilder(population);

                                    foreach (var act in acts())
                                    {
                                        for (var actRepeat = 0; actRepeat < actRepeats; actRepeat++)
                                        {
                                            preact(population);
                                            act(association, role, from, to, toAnother);
                                        }
                                    }

                                    foreach (var assert in assertPermutation)
                                    {
                                        for (var assertRepeat = 0; assertRepeat < assertRepeats; assertRepeat++)
                                        {
                                            assert(association, role, from, to, toAnother);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FromFromAnotherToToAnother(
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject>>> acts,
           Func<IEnumerable<Action<EmbeddedOneToOneAssociationType, EmbeddedOneToOneRoleType, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject, IEmbeddedObject>>> asserts)
        {
            var assertPermutations = asserts().Permutations().ToArray();

            foreach (var preact in this.preActs)
            {
                foreach (var snapshot in new bool[] { false, true })
                {
                    foreach (var actRepeats in new[] { 1, 2 })
                    {
                        foreach (var assertPermutation in assertPermutations)
                        {
                            foreach (var assertRepeats in new[] { 1, 2 })
                            {
                                foreach (var fixture in this.fixtures)
                                {
                                    var (association, role, _, fromBuilder, fromAnotherBuilder, toBuilder,
                                        toAnotherBuilder) = fixture();

                                    var population = this.CreatePopulation();

                                    var from = fromBuilder(population);
                                    var fromAnother = fromAnotherBuilder(population);
                                    var to = toBuilder(population);
                                    var toAnother = toAnotherBuilder(population);

                                    foreach (var act in acts())
                                    {
                                        for (var actRepeat = 0; actRepeat < actRepeats; actRepeat++)
                                        {
                                            preact(population);
                                            act(association, role, from, fromAnother, to, toAnother);
                                        }
                                    }

                                    foreach (var assert in assertPermutation)
                                    {
                                        for (var assertRepeat = 0; assertRepeat < assertRepeats; assertRepeat++)
                                        {
                                            assert(association, role, from, fromAnother, to, toAnother);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
