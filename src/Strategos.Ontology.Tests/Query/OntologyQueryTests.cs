using Strategos.Ontology.Query;

namespace Strategos.Ontology.Tests.Query;

public class OntologyQueryTests
{
    [Test]
    public async Task IOntologyQuery_InterfaceExists()
    {
        // Arrange & Act
        var type = typeof(IOntologyQuery);

        // Assert
        await Assert.That(type.IsInterface).IsTrue();
    }

    [Test]
    public async Task OntologyQueryResult_Create_HasObjectTypes()
    {
        // Arrange
        var objectTypes = new List<string> { "Contact", "Deal" };

        // Act
        var result = new OntologyQueryResult(objectTypes);

        // Assert
        await Assert.That(result.ObjectTypes).HasCount().EqualTo(2);
        await Assert.That(result.ObjectTypes[0]).IsEqualTo("Contact");
        await Assert.That(result.ObjectTypes[1]).IsEqualTo("Deal");
    }
}
