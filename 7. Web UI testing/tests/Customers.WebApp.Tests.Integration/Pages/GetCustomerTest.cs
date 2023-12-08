using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages;

[Collection("Test collection")]
public class GetCustomerTest
{
    private readonly SharedTestContext _testContext;
    
    private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date))
        .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername);

    public GetCustomerTest(SharedTestContext testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public async Task Get_ReturnsCustomer_WhenCustomerExists()
    {
        //arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });

        var customer = await CreateCustomer(page);

        //act
        var linkElement = page.Locator("article>p>a").First;
        var link = await linkElement.GetAttributeAsync("href");
        await page.GotoAsync(link!);
        
        //assert
        (await page.Locator("p[id=fullname-field]").InnerTextAsync())
            .Should().Be(customer.FullName);
        (await page.Locator("p[id=email-field]").InnerTextAsync())
            .Should().Be(customer.Email);
        (await page.Locator("p[id=github-username-field]").InnerTextAsync())
            .Should().Be(customer.GitHubUsername);
        (await page.Locator("p[id=dob-field]").InnerTextAsync())
            .Should().Be(customer.DateOfBirth.ToString("dd/MM/yyyy"));

        //cleanup
        await page.CloseAsync();
    }

    [Fact]
    public async Task Get_ReturnsNoCustomer_WhenNoCustomerExists()
    {
        //arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });

        var customerUrl = $"{SharedTestContext.AppUrl}/customer/{Guid.NewGuid()}";
        
        //act
        await page.GotoAsync(customerUrl);
        
        //assert
        (await page.Locator("p").InnerTextAsync())
            .Should().Be("No customer found with this id");
    }

    private async Task<Customer> CreateCustomer(IPage page)
    {
        await page.GotoAsync("add-customer");
        var customer = _customerGenerator.Generate();

        await page.FillAsync("input[id=fullname]", customer.FullName);
        await page.FillAsync("input[id=email]", customer.Email);
        await page.FillAsync("input[id=github-username]", customer.GitHubUsername);
        await page.FillAsync("input[id=dob]", customer.DateOfBirth.ToString("yyyy-MM-dd"));

        await page.ClickAsync("button[type=submit]");
        return customer;
    }
}