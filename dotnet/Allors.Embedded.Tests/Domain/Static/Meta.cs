namespace Allors.Embedded.Tests.Domain.Static
{
    using System;
    using Allors.Embedded.Meta;

    public class Meta
    {
        public Meta()
        {
            this.EmbeddedMeta = new EmbeddedMeta();

            var m = this.EmbeddedMeta;

            this.I1 = m.AddInterface("I1");
            this.I2 = m.AddInterface("I2");
            this.I12 = m.AddInterface("I12");

            this.C1 = m.AddClass("C1");
            this.C2 = m.AddClass("C2");
            this.C3 = m.AddClass("C3");
            this.C4 = m.AddClass("C4");

            this.I1.AddDirectSupertype(this.I12);
            this.I2.AddDirectSupertype(this.I12);

            this.C1.AddDirectSupertype(this.I1);
            this.C2.AddDirectSupertype(this.I2);

            (_, this.I1AllorsString) = m.AddUnit<string>(this.I1, "I1AllorsString");
            (_, this.C1AllorsString) = m.AddUnit<string>(this.C1, "C1AllorsString");
            (_, this.C2AllorsString) = m.AddUnit<string>(this.C2, "C2AllorsString");
            (_, this.C3AllorsString) = m.AddUnit<string>(this.C3, "C3AllorsString");
            (_, this.C4AllorsString) = m.AddUnit<string>(this.C4, "C4AllorsString");

            (this.C1WhereC1OneToOne, this.C1C1OneToOne) = m.AddOneToOne(this.C1, this.C1, "C1OneToOne");
            (this.C1WhereI1OneToOne, this.C1I1OneToOne) = m.AddOneToOne(this.C1, this.I1, "I1OneToOne");
            (this.C1WhereC2OneToOne, this.C1C2OneToOne) = m.AddOneToOne(this.C1, this.C2, "C2OneToOne");
            (this.C1WhereI2OneToOne, this.C1I2OneToOne) = m.AddOneToOne(this.C1, this.I2, "I2OneToOne");
            (this.C1sWhereC1ManyToOne, this.C1C1ManyToOne) = m.AddManyToOne(this.C1, this.C1, "C1ManyToOne");
            (this.C1WhereC1C1one2many, this.C1C1OneToManies) = m.AddOneToMany(this.C1, this.C1, "C1OneToMany");
        }

        public EmbeddedMeta EmbeddedMeta { get; }

        public EmbeddedObjectType C1 { get; }

        public EmbeddedOneToOneRoleType C1C1OneToOne { get; }

        public EmbeddedOneToOneAssociationType C1WhereC1OneToOne { get; }

        public EmbeddedOneToOneRoleType C1I1OneToOne { get; }

        public EmbeddedOneToOneAssociationType C1WhereI1OneToOne { get; }

        public EmbeddedOneToOneRoleType C1C2OneToOne { get; }

        public EmbeddedOneToOneAssociationType C1WhereC2OneToOne { get; }

        public EmbeddedOneToOneRoleType C1I2OneToOne { get; }

        public EmbeddedOneToOneAssociationType C1WhereI2OneToOne { get; }

        public EmbeddedManyToOneRoleType C1C1ManyToOne { get; }

        public EmbeddedManyToOneAssociationType C1sWhereC1ManyToOne { get; }

        public EmbeddedOneToManyRoleType C1C1OneToManies { get; }

        public EmbeddedOneToManyAssociationType C1WhereC1C1one2many { get; }

        public EmbeddedObjectType C2 { get; }

        public EmbeddedObjectType C3 { get; }

        public EmbeddedObjectType C4 { get; }

        public EmbeddedObjectType I1 { get; }

        public EmbeddedObjectType I2 { get; }

        public EmbeddedObjectType I12 { get; }

        public EmbeddedUnitRoleType I1AllorsString { get; }

        public EmbeddedUnitRoleType C1AllorsString { get; }

        public EmbeddedUnitRoleType C2AllorsString { get; }

        public EmbeddedUnitRoleType C3AllorsString { get; }

        public EmbeddedUnitRoleType C4AllorsString { get; }
    }
}
