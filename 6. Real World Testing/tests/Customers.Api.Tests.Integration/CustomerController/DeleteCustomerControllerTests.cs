using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController;

public class DeleteCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser);

    public DeleteCustomerControllerTests(CustomerApiFactory appFactory)
    {
        _client = appFactory.CreateClient();
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenCustomerExists()
    {
        //arrange
        var customer = _customerGenerator.Generate();
        var createdResponse = await _client.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        
        //act
        var response = await _client.DeleteAsync($"customers/{createdCustomer!.Id}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCustomerDoesNotExists()
    {
        //act
        var response = await _client.GetAsync($"customers/{Guid.NewGuid()}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}