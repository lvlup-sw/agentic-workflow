namespace Strategos.Ontology.Generators.Tests.Analyzers;

public class PropertyAnalyzerTests
{
    [Test]
    public async Task ONTO002_PropertyExpressionValidMember_NoError()
    {
        var source = """
            using System;
            using Strategos.Ontology;
            using Strategos.Ontology.Builder;

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
                    });
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(d => d.Id == "ONTO002")).IsFalse();
    }

    [Test]
    public async Task ONTO002_PropertyExpressionInvalidMember_ReportsError()
    {
        // The expression e => e.ToString() is a method call, not a property member access.
        // The analyzer should flag this because Property() expects a simple property accessor.
        var source = """
            using System;
            using Strategos.Ontology;
            using Strategos.Ontology.Builder;

            public class TestEntity
            {
                public string Id { get; set; }
            }

            public class TestOntology : DomainOntology
            {
                public override string DomainName => "test";
                protected override void Define(IOntologyBuilder builder)
                {
                    builder.Object<TestEntity>(obj =>
                    {
                        obj.Key(e => e.Id);
                        obj.Property(e => e.ToString());
                    });
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(d => d.Id == "ONTO002")).IsTrue();
    }
}
