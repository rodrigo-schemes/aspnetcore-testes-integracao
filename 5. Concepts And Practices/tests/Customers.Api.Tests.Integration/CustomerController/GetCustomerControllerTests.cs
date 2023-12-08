using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController;

[Collection("CustomerApi Collection")]
public class GetCustomerControllerTests
{
    private readonly HttpClient _httpClient;
    
    public GetCustomerControllerTests(WebApplicationFactory<IApiMarker> appFactory)
    {
        _httpClient = appFactory.CreateClient();
    }
    
    [Theory]
    [InlineData("C15B0148-D96B-4C66-A4BF-7EFD5D175218")]
    [InlineData("5D4D3485-AB39-424D-9670-892DE2C6FE91")]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist(string id)
    {
        //act
        var response = await _httpClient.GetAsync($"customers/{id}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("404");

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Title.Should().Be("Not Found");
        problem.Status.Should().Be(404);
    }
}