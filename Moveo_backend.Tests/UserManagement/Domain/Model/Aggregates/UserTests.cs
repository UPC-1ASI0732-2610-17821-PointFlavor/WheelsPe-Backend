using FluentAssertions;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Model.ValueObjects;
using Xunit;

namespace Moveo_backend.Tests.UserManagement.Domain.Model.Aggregates;

public class UserTests
{
    private static CreateUserCommand DefaultCommand(string role = "tenant") =>
        new CreateUserCommand(
            FirstName: "Juan",
            LastName: "García",
            Email: "juan@example.com",
            Password: "hashed_password",
            Phone: "999000111",
            Dni: "12345678",
            LicenseNumber: "LIC-001",
            Role: role);

    // HU05 / HU06 - Registro de usuario
    [Fact]
    public void Constructor_FromCommand_SetsPersonalDataCorrectly()
    {
        var user = new User(DefaultCommand("tenant"));

        user.FirstName.Should().Be("Juan");
        user.LastName.Should().Be("García");
        user.Email.Should().Be("juan@example.com");
        user.Role.Should().Be("tenant");
        user.Phone.Should().Be("999000111");
        user.Dni.Should().Be("12345678");
        user.FullName.Should().Be("Juan García");
    }

    [Fact]
    public void Constructor_DefaultsVerificationFlagsToFalse()
    {
        var user = new User(DefaultCommand());

        user.EmailVerified.Should().BeFalse();
        user.PhoneVerified.Should().BeFalse();
        user.DniVerified.Should().BeFalse();
        user.LicenseVerified.Should().BeFalse();
    }

    [Fact]
    public void Constructor_DefaultsStatisticsToZero()
    {
        var user = new User(DefaultCommand());

        user.TotalRentals.Should().Be(0);
        user.ActiveRentals.Should().Be(0);
        user.CompletedRentals.Should().Be(0);
        user.CanceledRentals.Should().Be(0);
        user.TotalSpent.Should().Be(0m);
        user.TotalEarned.Should().Be(0m);
    }

    // HU15 - Verificación de identidad
    [Fact]
    public void VerifyEmail_SetsEmailVerifiedToTrue()
    {
        var user = new User(DefaultCommand());
        user.VerifyEmail();
        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void VerifyDni_SetsDniVerifiedToTrue()
    {
        var user = new User(DefaultCommand());
        user.VerifyDni();
        user.DniVerified.Should().BeTrue();
    }

    [Fact]
    public void VerifyPhone_SetsPhoneVerifiedToTrue()
    {
        var user = new User(DefaultCommand());
        user.VerifyPhone();
        user.PhoneVerified.Should().BeTrue();
    }

    [Fact]
    public void VerifyLicense_SetsLicenseVerifiedToTrue()
    {
        var user = new User(DefaultCommand());
        user.VerifyLicense();
        user.LicenseVerified.Should().BeTrue();
    }

    // HU07 / HU11 - Estadísticas de alquiler
    [Fact]
    public void IncrementActiveRentals_IncreasesActiveAndTotalRentals()
    {
        var user = new User(DefaultCommand());

        user.IncrementActiveRentals();

        user.ActiveRentals.Should().Be(1);
        user.TotalRentals.Should().Be(1);
    }

    // HU14 - Completar alquiler (inquilino)
    [Fact]
    public void CompleteRental_AsRenter_IncreasesTotalSpent()
    {
        var user = new User(DefaultCommand());
        user.IncrementActiveRentals();

        user.CompleteRental(500m, isOwner: false);

        user.CompletedRentals.Should().Be(1);
        user.ActiveRentals.Should().Be(0);
        user.TotalSpent.Should().Be(500m);
        user.TotalEarned.Should().Be(0m);
    }

    // HU14 - Liberar pago al propietario
    [Fact]
    public void CompleteRental_AsOwner_IncreasesTotalEarned()
    {
        var user = new User(DefaultCommand("owner"));
        user.IncrementActiveRentals();

        user.CompleteRental(450m, isOwner: true);

        user.TotalEarned.Should().Be(450m);
        user.TotalSpent.Should().Be(0m);
        user.CompletedRentals.Should().Be(1);
    }

    [Fact]
    public void CancelRental_DecrementsActiveAndIncrementsCanceled()
    {
        var user = new User(DefaultCommand());
        user.IncrementActiveRentals();

        user.CancelRental();

        user.ActiveRentals.Should().Be(0);
        user.CanceledRentals.Should().Be(1);
    }

    // HU05/HU06 - Cambio de contraseña
    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        var user = new User(DefaultCommand());

        user.ChangePassword("new_hashed_password");

        user.PasswordHash.Should().Be("new_hashed_password");
    }

    [Fact]
    public void UpdateBankAccount_SetsBankingInfo()
    {
        var user = new User(DefaultCommand("owner"));

        user.UpdateBankAccount("BCP", "Ahorros", "1234567890", true);

        user.BankName.Should().Be("BCP");
        user.BankAccountType.Should().Be("Ahorros");
        user.BankAccountNumber.Should().Be("1234567890");
        user.BankAccountVerified.Should().BeTrue();
    }

    [Fact]
    public void Update_ChangesPersonalInfo()
    {
        var user = new User(DefaultCommand());
        var prefs = new UserPreferences("en", true, true, false, false, 1, false);

        user.Update("Carlos", "López", "988000222", "Av. Lima 123", prefs);

        user.FirstName.Should().Be("Carlos");
        user.LastName.Should().Be("López");
        user.Phone.Should().Be("988000222");
        user.PreferredLanguage.Should().Be("en");
    }
}
