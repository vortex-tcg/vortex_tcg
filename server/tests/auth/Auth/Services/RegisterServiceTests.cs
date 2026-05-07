using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Services;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace VortexTCG.Tests.Auth.Services
{
    public class RegisterServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static RegisterDTO ValidRequest() => new RegisterDTO
        {
            first_name = "Jean",
            last_name = "Dupont",
            username = "jeandupont",
            email = "jean@example.com",
            password = "SecurePass1!",
            password_confirmation = "SecurePass1!"
        };

        [Fact]
        public async Task Register_Returns400_WhenFirstNameMissing()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.first_name = "";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Register_Returns400_WhenLastNameMissing()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.last_name = "   ";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Register_Returns400_WhenUsernameMissing()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.username = "";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Register_Returns400_WhenEmailMissing()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.email = "";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Register_Returns400_WhenPasswordMissing()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.password = "";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Register_Returns400_WhenPasswordConfirmationMissing()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.password_confirmation = "";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Register_Returns400_WhenPasswordsDoNotMatch()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.password_confirmation = "DifferentPass1!";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("correspondent pas", result.Message);
        }

        [Fact]
        public async Task Register_Returns400_WhenPasswordTooWeak()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);
            RegisterDTO request = ValidRequest();
            request.password = "weakpassword";
            request.password_confirmation = "weakpassword";

            RegisterService.RegisterResult result = await service.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("mot de passe", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Register_Returns409_WhenEmailAlreadyUsed()
        {
            using VortexDbContext db = CreateDb();
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "Existing", LastName = "User", Username = "existing", Email = "jean@example.com", Password = "x", Language = "fr", CurrencyQuantity = 0 });
            await db.SaveChangesAsync();

            RegisterService service = new RegisterService(db);

            RegisterService.RegisterResult result = await service.RegisterAsync(ValidRequest());

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("Email", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Register_Returns409_WhenUsernameAlreadyTaken()
        {
            using VortexDbContext db = CreateDb();
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "Other", LastName = "User", Username = "jeandupont", Email = "other@example.com", Password = "x", Language = "fr", CurrencyQuantity = 0 });
            await db.SaveChangesAsync();

            RegisterService service = new RegisterService(db);

            RegisterService.RegisterResult result = await service.RegisterAsync(ValidRequest());

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("utilisateur", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Register_Returns201_WhenSuccess()
        {
            using VortexDbContext db = CreateDb();
            RegisterService service = new RegisterService(db);

            RegisterService.RegisterResult result = await service.RegisterAsync(ValidRequest());

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(1, db.Users.Count());
        }
    }
}