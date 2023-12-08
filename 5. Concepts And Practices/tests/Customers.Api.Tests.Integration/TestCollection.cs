using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Customers.Api.Tests.Integration;

[CollectionDefinition("CustomerApi Collection")]
public class TestCollection : ICollectionFixture<WebApplicationFactory<IApiMarker>>
{
    
}