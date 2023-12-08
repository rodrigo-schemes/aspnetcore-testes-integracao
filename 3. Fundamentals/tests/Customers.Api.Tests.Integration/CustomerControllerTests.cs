using System.Net;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerControllerTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:5001")
    };

    public CustomerControllerTests()
    {
        
    }
    
    [Theory]
    [InlineData("C15B0148-D96B-4C66-A4BF-7EFD5D175218")]
    [InlineData("5D4D3485-AB39-424D-9670-892DE2C6FE91")]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist(string id)
    {
        //act
        var response = await _httpClient.GetAsync($"customers/{id}");
        
        //assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}