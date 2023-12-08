using Xunit;

namespace Customers.WebApp.Tests.Integration;

[CollectionDefinition("Test collection")]
public class SharedTestCollection : ICollectionFixture<SharedTestContext>
{
    
}