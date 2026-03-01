namespace Strategos.Ontology.Generators.Tests.Analyzers;

public class InterfaceAnalyzerTests
{
    [Test]
    public async Task ONTO005_CompatiblePropertyMapping_NoError()
    {
        var source = """
            using System;
            using Strategos.Ontology;
            using Strategos.Ontology.Builder;

            public interface ISearchable
            {
                string Title { get; }
            }

            public class TestEntity
            {
                public string Id { get; set; }
                public string Name { get; set; }
            }

            public class TestOntology : DomainOntology
            {
                public override string DomainName => "test";
                protected override void Define(IOntologyBuilder builder)
                {
                    builder.Object<TestEntity>(obj =>
                    {
                        obj.Key(e => e.Id);
                        obj.Property(e => e.Name);
                        obj.Implements<ISearchable>(map =>
                        {
                            map.Via(e => e.Name, s => s.Title);
                        });
                    });
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(d => d.Id == "ONTO005")).IsFalse();
    }

    [Test]
    public async Task ONTO005_IncompatiblePropertyTypes_ReportsError()
    {
        var source = """
            using System;
            using Strategos.Ontology;
            using Strategos.Ontology.Builder;

            public interface ISearchable
            {
                string Title { get; }
            }

            public class TestEntity
            {
                public string Id { get; set; }
                public int Rank { get; set; }
            }

            public class TestOntology : DomainOntology
            {
                public override string DomainName => "test";
                protected override void Define(IOntologyBuilder builder)
                {
                    builder.Object<TestEntity>(obj =>
                    {
                        obj.Key(e => e.Id);
                        obj.Property(e => e.Rank);
                        obj.Implements<ISearchable>(map =>
                        {
                            map.Via(e => e.Rank, s => s.Title);
                        });
                    });
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(d => d.Id == "ONTO005")).IsTrue();
    }
}
