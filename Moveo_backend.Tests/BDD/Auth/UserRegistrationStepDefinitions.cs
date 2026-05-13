using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moveo_backend.IAM.Application.Internal;
using Moveo_backend.IAM.Domain.Model.Commands;
using Moveo_backend.IAM.Infrastructure.Hashing;
using Moveo_backend.IAM.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Reqnroll;

namespace Moveo_backend.Tests.BDD.Auth;

[Binding]
public class UserRegistrationStepDefinitions
{
    private AppDbContext _context = null!;
    private AuthService _authService = null!;
    private AuthenticatedUserResource? _registrationResult;
    private AuthenticatedUserResource? _loginResult;

    private void InitializeContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"BddAuthDb_{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _authService = new AuthService(_context, new BcryptHashingService());
    }

    [Given(@"no user exists with email ""(.*)""")]
    public void GivenNoUserExistsWithEmail(string email)
    {
        InitializeContext();
    }

    [Given(@"a user already exists with email ""(.*)""")]
    public async Task GivenAUserAlreadyExistsWithEmail(string email)
    {
        InitializeContext();
        var command = new RegisterCommand("Existing", "User", email, "InitialPass1!");
        await _authService.RegisterAsync(command);
    }

    [Given(@"a registered user with email ""(.*)"" and password ""(.*)""")]
    public async Task GivenARegisteredUserWithEmailAndPassword(string email, string password)
    {
        InitializeContext();
        var command = new RegisterCommand("Driver", "Test", email, password);
        await _authService.RegisterAsync(command);
    }

    [When(@"I register with name ""(.*)"", email ""(.*)"" and password ""(.*)""")]
    public async Task WhenIRegisterWithNameEmailAndPassword(string fullName, string email, string password)
    {
        var parts = fullName.Split(' ', 2);
        var command = new RegisterCommand(parts[0], parts[1], email, password);
        _registrationResult = await _authService.RegisterAsync(command);
    }

    [When(@"I register again with email ""(.*)"" and password ""(.*)""")]
    public async Task WhenIRegisterAgainWithEmailAndPassword(string email, string password)
    {
        var command = new RegisterCommand("Carlos", "Lopez", email, password);
        _registrationResult = await _authService.RegisterAsync(command);
    }

    [When(@"I login with email ""(.*)"" and password ""(.*)""")]
    public async Task WhenILoginWithEmailAndPassword(string email, string password)
    {
        var command = new LoginCommand(email, password);
        _loginResult = await _authService.LoginAsync(command);
    }

    [Then(@"the registration should succeed")]
    public void ThenTheRegistrationShouldSucceed()
    {
        _registrationResult.Should().NotBeNull();
    }

    [Then(@"the returned user email should be ""(.*)""")]
    public void ThenTheReturnedUserEmailShouldBe(string email)
    {
        _registrationResult!.Email.Should().Be(email);
    }

    [Then(@"the registration should fail with null result")]
    public void ThenTheRegistrationShouldFailWithNullResult()
    {
        _registrationResult.Should().BeNull();
    }

    [Then(@"the login should succeed")]
    public void ThenTheLoginShouldSucceed()
    {
        _loginResult.Should().NotBeNull();
    }

    [Then(@"the authenticated user email should be ""(.*)""")]
    public void ThenTheAuthenticatedUserEmailShouldBe(string email)
    {
        _loginResult!.Email.Should().Be(email);
    }
}
