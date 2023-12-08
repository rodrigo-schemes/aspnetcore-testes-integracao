using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController;

public class CreateCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser);

    public CreateCustomerControllerTests(CustomerApiFactory appFactory)
    {
        _client = appFactory.CreateClient();
    }

    [Fact]
    public async Task Create_CreateUser_WhenDataIsValid()
    {
        // arrange
        var customer = _customerGenerator.Generate();
        
        //act
        var response = await _client.PostAsJsonAsync("customers", customer);
        
        //assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().Be($"http://localhost/customers/{customerResponse!.Id}");
    }
    
    [Fact]
    public async Task Create_ReturnsValidationError_WhenEmailIsInvalid()
    {
        // Arrange
        const string invalidEmail = "dasdja9d3j";
        var customer = _customerGenerator.Clone()
            .RuleFor(x => x.Email, invalidEmail).Generate();

        // Act
        var response = await _client.PostAsJsonAsync("customers", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error.Title.Should().Be("One or more validation errors occurred.");
        error.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
    }
    
    [Fact]
    public async Task Create_ReturnsValidationError_WhenGitHubUserDoestNotExist()
    {
        // Arrange
        const string invalidGitHubUser = "dasdja9d3j";
        var customer = _customerGenerator.Clone()
            .RuleFor(x => x.GitHubUsername, invalidGitHubUser).Generate();

        // Act
        var response = await _client.PostAsJsonAsync("customers", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error.Title.Should().Be("One or more validation errors occurred.");
        error.Errors["GitHubUsername"][0].Should().Be($"There is no GitHub user with username {invalidGitHubUser}");
    }
    
    [Fact]
    public async Task Create_ReturnsValidationError_WhenGithubIsThrottled()
    {
        // arrange
        var customer = _customerGenerator.Clone()
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ThrottledUser)
            .Generate();
        
        //act
        var response = await _client.PostAsJsonAsync("customers", customer);
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}