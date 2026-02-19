using Scrypt;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using System;
using System.Linq;
using System.Collections.Generic;

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

        private static void StampNew(AuditableEntity e, DateTime utc, string actor)
        {
            e.CreatedAtUtc = utc;
            e.CreatedBy = actor;
            e.UpdatedAtUtc = utc;
            e.UpdatedBy = actor;
        }

        private static void StampUpdate(AuditableEntity e, DateTime utc, string actor)
        {
            e.UpdatedAtUtc = utc;
            e.UpdatedBy = actor;
        }

        public void Seed()
        {
            DateTime utc = DateTime.UtcNow;
            string actor = "Seeder";

            User john = _db.Users.SingleOrDefault(u => u.Username == "Superman");
            if (john == null)
            {
                john = new User
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
                    RankId = null,
                    CollectionId = null
                };
                StampNew(john, utc, actor);
                _db.Users.Add(john);
            }
            
            User jane = _db.Users.SingleOrDefault(u => u.Username == "Batman");
            if (jane == null)
            {
                jane = new User
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
                    CollectionId = null
                };
                StampNew(jane, utc, actor);
                _db.Users.Add(jane);
            }

            if (john.CollectionId == null || !_db.Set<Collection>().Any(c => c.Id == john.CollectionId.Value))
            {
                Collection c = new Collection
                {
                    Id = Guid.NewGuid(),
                    User = john,
                    Cards = new List<CollectionCard>(),
                    Champions = new List<CollectionChampion>()
                };
                StampNew(c, utc, actor);
                _db.Set<Collection>().Add(c);
                john.CollectionId = c.Id;
                StampUpdate(john, utc, actor);
            }

            if (jane.CollectionId == null || !_db.Set<Collection>().Any(c => c.Id == jane.CollectionId.Value))
            {
                Collection c = new Collection
                {
                    Id = Guid.NewGuid(),
                    User = jane,
                    Cards = new List<CollectionCard>(),
                    Champions = new List<CollectionChampion>()
                };
                StampNew(c, utc, actor);
                _db.Set<Collection>().Add(c);
                jane.CollectionId = c.Id;
                StampUpdate(jane, utc, actor);
            }

            _db.SaveChanges();
        }
    }
}
