using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController;

public class GetAllCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser);

    public GetAllCustomerControllerTests(CustomerApiFactory appFactory)
    {
        _client = appFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllCustomers_WhenCustomersExists()
    {
        //arrange
        var customer = _customerGenerator.Generate();
        var createdResponse = await _client.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        
        //act
        var response = await _client.GetAsync("customers");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customersResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        customersResponse!.Customers.Single().Should().BeEquivalentTo(createdCustomer);
        
        //cleanup
        await _client.DeleteAsync($"customers/{createdCustomer!.Id}");
    }
    
    [Fact]
    public async Task GetAll_ReturnsEmptyResult_WhenNoCustomersExists()
    {
        //act
        var response = await _client.GetAsync("customers");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customersResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        customersResponse!.Customers.Should().BeEmpty();
    }
}