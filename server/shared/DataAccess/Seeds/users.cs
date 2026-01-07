using Scrypt;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using System;
using System.Linq;

namespace VortexTCG.DataAccess.Seeds
{
    public class UsersInitializer
    {
        private readonly VortexDbContext _db;
        private static readonly ScryptEncoder encoder = new ScryptEncoder();
        private const string SeederName = "Seeder";

        public UsersInitializer(VortexDbContext db)
        {
            _db = db;
        }

        public void Seed()
        {
            // Vérifie si la table Users est vide
            if (_db.Users.Any())
                return;

            var utcDate = DateTime.UtcNow;

            // Ajoute les utilisateurs
            _db.Users.AddRange(
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Username = "Superman",
                    Password = encoder.Encode("Password123"),
                    Email = "john.doe@email.com",
                    CurrencyQuantity = 10,
                    Language = "fr",
                    Role = Role.USER,
                    Status = UserStatus.DISCONNECTED,

                    RankId = null,        // null pour éviter violation FK
                    CollectionId = null,  // null pour éviter violation FK

                    CreatedAtUtc = utcDate,
                    CreatedBy = SeederName,
                    UpdatedAtUtc = utcDate,
                    UpdatedBy = SeederName,
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Doe",
                    Username = "Batman",
                    Password = encoder.Encode("Password456"),
                    Email = "jane.doe@email.com",
                    CurrencyQuantity = 100,
                    Language = "en",
                    Role = Role.USER,
                    Status = UserStatus.DISCONNECTED,

                    RankId = null,
                    CollectionId = null,

                    CreatedAtUtc = utcDate,
                    CreatedBy = "Seeder",
                    UpdatedAtUtc = utcDate,
                    UpdatedBy = "Seeder",
                }
            );

            // Sauvegarde en base
            _db.SaveChanges();
        }
    }
}
