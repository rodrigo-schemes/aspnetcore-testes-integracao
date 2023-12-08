using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController;

public class GetCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser);

    public GetCustomerControllerTests(CustomerApiFactory appFactory)
    {
        _client = appFactory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsCustomer_WhenCustomerExists()
    {
        //arrange
        var customer = _customerGenerator.Generate();
        var createdResponse = await _client.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        
        //act
        var response = await _client.GetAsync($"customers/{createdCustomer!.Id}");
        
        //assert
        var retrievedCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        retrievedCustomer.Should().BeEquivalentTo(createdCustomer);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExists()
    {
        //act
        var response = await _client.GetAsync($"customers/{Guid.NewGuid()}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}