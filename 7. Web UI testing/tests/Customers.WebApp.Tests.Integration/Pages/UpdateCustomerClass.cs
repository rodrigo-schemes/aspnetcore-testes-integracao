using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages;

[Collection("Test collection")]
public class UpdateCustomerClass
{
    private readonly SharedTestContext _testContext;
    
    private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date))
        .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername);

    public UpdateCustomerClass(SharedTestContext testContext)
    {
        _testContext = testContext;
    }
    
    [Fact]
    public async Task Update_UpdateCustomer_WhenDataIsValid()
    {
        //arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });

        var customer = await CreateCustomer(page).ConfigureAwait(true);
        await page.GotoAsync($"update-customer/{customer.Id}");

        //act
        await page.FillAsync("input[id=fullname]", "change name");
        await page.ClickAsync("button[type=submit]");
        
        //assert
        var linkElement = page.Locator("article>p>a").First;
        var link = await linkElement.GetAttributeAsync("href");
        await page.GotoAsync(link!);

        (await page.Locator("p[id=fullname-field]").InnerTextAsync())
            .Should().Be("change name");
        (await page.Locator("p[id=email-field]").InnerTextAsync())
            .Should().Be(customer.Email);
        (await page.Locator("p[id=github-username-field]").InnerTextAsync())
            .Should().Be(customer.GitHubUsername);
        (await page.Locator("p[id=dob-field]").InnerTextAsync())
            .Should().Be(customer.DateOfBirth.ToString("dd/MM/yyyy"));

        //cleanup
        await page.CloseAsync();
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

        var element = page.Locator("article>p>a").First;
        var link = await element.GetAttributeAsync("href");
        var idInText = link!.Split('/').Last();
        customer.Id = Guid.Parse(idInText);
        
        return customer;
    }
}