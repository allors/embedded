namespace Allors.Embedded.Tests.Meta.Diagrams
{
    using Allors.Embedded.Meta;
    using Allors.Embedded.Meta.Diagrams;
    using Xunit;

    public class ClassDiagramTests
    {
        [Fact]
        public void Inheritance()
        {
            var meta = new EmbeddedMeta();
            var s1 = meta.AddInterface("S1");
            var i1 = meta.AddInterface("I1", s1);
            var c1 = meta.AddClass("C1", i1);

            var diagram = new ClassDiagram(meta).Render();

            Assert.Equal(
@"classDiagram
    class C1
    I1 <|-- C1
    class I1
    S1 <|-- I1
    class S1
",
diagram);
        }

        [Fact]
        public void Roles()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddOneToMany(organization, person, "Employee");

            var diagram = new ClassDiagram(meta).Render();

            Assert.Equal(
                """
                classDiagram
                    class Organization
                    Organization o-- Person : Employees
                    class Person

                """,
                diagram);
        }

        [Fact]
        public void InheritedRoles()
        {
            var meta = new EmbeddedMeta();
            var internalOrganization = meta.AddClass("InternalOrganization");
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");

            organization.AddDirectSupertype(internalOrganization);

            meta.AddOneToMany(internalOrganization, person, "Employee");
            meta.AddOneToMany(organization, person, "Customer");

            var diagram = new ClassDiagram(meta).Render();

            Assert.Equal(
                """
                classDiagram
                    class InternalOrganization
                    InternalOrganization o-- Person : Employees
                    class Organization
                    InternalOrganization <|-- Organization
                    Organization o-- Person : Customers
                    class Person

                """,
                diagram);
        }

        [Fact]
        public void Title()
        {
            var meta = new EmbeddedMeta();

            var config = new ClassDiagram.Config { Title = "My Empty Diagram" };
            var diagram = new ClassDiagram(meta, config).Render();

            Assert.Equal(
                """
                ---
                title: My Empty Diagram
                ---
                classDiagram

                """,
                diagram);
        }

        [Fact]
        public void Multiplicity()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddOneToMany(organization, person, "Employee");

            var config = new ClassDiagram.Config { OneMultiplicity = "1", ManyMultiplicity = "1..*" };
            var diagram = new ClassDiagram(meta, config).Render();

            Assert.Equal(
                """
                classDiagram
                    class Organization
                    Organization "1" o-- "1..*" Person : Employees
                    class Person

                """,
                diagram);
        }

        [Fact]
        public void MultiplicityOne()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddOneToMany(organization, person, "Employee");

            var config = new ClassDiagram.Config { OneMultiplicity = "one" };
            var diagram = new ClassDiagram(meta, config).Render();

            Assert.Equal(
                """
                classDiagram
                    class Organization
                    Organization "one" o-- Person : Employees
                    class Person

                """,
                diagram);
        }

        [Fact]
        public void MultiplicityMany()
        {
            var meta = new EmbeddedMeta();
            var organization = meta.AddClass("Organization");
            var person = meta.AddClass("Person");
            meta.AddOneToMany(organization, person, "Employee");

            var config = new ClassDiagram.Config { ManyMultiplicity = "many" };
            var diagram = new ClassDiagram(meta, config).Render();

            Assert.Equal(
                """
                classDiagram
                    class Organization
                    Organization o-- "many" Person : Employees
                    class Person

                """,
                diagram);
        }
    }
}
