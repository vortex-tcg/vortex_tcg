using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace Tests;

public class RegisterServiceTests
{
    private static VortexDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<VortexDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VortexDbContext(options);
    }

    [Fact]
    public async Task Missing_fields_returns_400()
    {
        using var db = CreateDb();
        var service = new RegisterService(db);
        var dto = new RegisterDTO
        {
            FirstName = "John",
            LastName = "Doe",
            // Username missing
            Email = "john@example.com",
            Password = "P@ssw0rd1!",
            PasswordConfirmation = "P@ssw0rd1!"
        };

        var result = await service.RegisterAsync(dto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Tous les champs sont requis", result.Message);
    }

    [Fact]
    public async Task Password_mismatch_returns_400()
    {
        using var db = CreateDb();
        var service = new RegisterService(db);
        var dto = new RegisterDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "john",
            Email = "john@example.com",
            Password = "P@ssw0rd1!",
            PasswordConfirmation = "P@ssw0rd2!"
        };

        var result = await service.RegisterAsync(dto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Les mots de passe ne correspondent pas", result.Message);
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("alllowercase")]
    [InlineData("NoNumberPassword!")]
    [InlineData("NoSpecialChar1")]
    public async Task Weak_password_returns_400(string password)
    {
        using var db = CreateDb();
        var service = new RegisterService(db);
        var dto = new RegisterDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "john",
            Email = "john@example.com",
            Password = password,
            PasswordConfirmation = password
        };

        var result = await service.RegisterAsync(dto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial", result.Message);
    }

    [Fact]
    public async Task Email_already_used_returns_409()
    {
        using var db = CreateDb();
        db.Users.Add(new User
        {
            FirstName = "Existing",
            LastName = "User",
            Username = "existing",
            Email = "john@example.com",
            Password = "hash",
            Language = "fr",
            CurrencyQuantity = 0
        });
        await db.SaveChangesAsync();

        var service = new RegisterService(db);
        var dto = new RegisterDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "john",
            Email = "john@example.com",
            Password = "P@ssw0rd1!",
            PasswordConfirmation = "P@ssw0rd1!"
        };

        var result = await service.RegisterAsync(dto);
        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Email déjà utilisé", result.Message);
    }

    [Fact]
    public async Task Username_already_used_returns_409()
    {
        using var db = CreateDb();
        db.Users.Add(new User
        {
            FirstName = "Existing",
            LastName = "User",
            Username = "john",
            Email = "other@example.com",
            Password = "hash",
            Language = "fr",
            CurrencyQuantity = 0
        });
        await db.SaveChangesAsync();

        var service = new RegisterService(db);
        var dto = new RegisterDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "john",
            Email = "john2@example.com",
            Password = "P@ssw0rd1!",
            PasswordConfirmation = "P@ssw0rd1!"
        };

        var result = await service.RegisterAsync(dto);
        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Nom d'utilisateur déjà pris", result.Message);
    }

    [Fact]
    public async Task Success_returns_201_and_persists_user()
    {
        using var db = CreateDb();
        var service = new RegisterService(db);
        var dto = new RegisterDTO
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Username = "ada",
            Email = "ada@example.com",
            Password = "P@ssw0rd1!",
            PasswordConfirmation = "P@ssw0rd1!"
        };

        var result = await service.RegisterAsync(dto);
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Contains("Utilisateur créé", result.Message);

        var saved = await db.Users.SingleOrDefaultAsync(u => u.Email == "ada@example.com");
        Assert.NotNull(saved);
        Assert.Equal("ada", saved!.Username);
    }
}