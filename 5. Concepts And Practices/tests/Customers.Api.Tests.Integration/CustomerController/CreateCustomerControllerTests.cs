using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController;

[Collection("CustomerApi Collection")]
public class CreateCustomerControllerTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly List<Guid> _createdIds = new();

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, "nickchapsas")
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth);

    public CreateCustomerControllerTests(WebApplicationFactory<IApiMarker> appFactory)
    {
        _httpClient = appFactory.CreateClient();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenCustomerIsCreated()
    {
        //arrange
        var customer = _customerGenerator.Generate();
        
        //act
        var response = await _httpClient.PostAsJsonAsync("customers", customer);
        
        //assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer, opt => opt
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromDays(1)))
            .WhenTypeIs<DateTime>());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        _createdIds.Add(customerResponse!.Id);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var createdId in _createdIds)
        {
            await _httpClient.DeleteAsync($"customers/{createdId}");
        }
    }
}