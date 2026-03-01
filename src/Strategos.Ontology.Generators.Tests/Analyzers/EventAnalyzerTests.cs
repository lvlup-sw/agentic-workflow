namespace Strategos.Ontology.Generators.Tests.Analyzers;

public class EventAnalyzerTests
{
    [Test]
    public async Task ONTO009_MaterializesLinkDeclaredLink_NoError()
    {
        var source = """
            using System;
            using Strategos.Ontology;
            using Strategos.Ontology.Builder;

            public class TestEntity
            {
                public string Id { get; set; }
            }

            public class RelatedEntity { }

            public class TestEvent
            {
                public string RelatedId { get; set; }
            }

            public class TestOntology : DomainOntology
            {
                public override string DomainName => "test";
                protected override void Define(IOntologyBuilder builder)
                {
                    builder.Object<TestEntity>(obj =>
                    {
                        obj.Key(e => e.Id);
                        obj.HasOne<RelatedEntity>("Related");
                        obj.Event<TestEvent>(evt =>
                        {
                            evt.MaterializesLink<TestEntity>("Related", e => e.RelatedId);
                        });
                    });
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(d => d.Id == "ONTO009")).IsFalse();
    }

    [Test]
    public async Task ONTO009_MaterializesLinkUndeclaredLink_ReportsError()
    {
        var source = """
            using System;
            using Strategos.Ontology;
            using Strategos.Ontology.Builder;

            public class TestEntity
            {
                public string Id { get; set; }
            }

            public class TestEvent
            {
                public string SomeId { get; set; }
            }

            public class TestOntology : DomainOntology
            {
                public override string DomainName => "test";
                protected override void Define(IOntologyBuilder builder)
                {
                    builder.Object<TestEntity>(obj =>
                    {
                        obj.Key(e => e.Id);
                        obj.Event<TestEvent>(evt =>
                        {
                            evt.MaterializesLink<TestEntity>("NonExistentLink", e => e.SomeId);
                        });
                    });
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(d => d.Id == "ONTO009")).IsTrue();
    }
}
