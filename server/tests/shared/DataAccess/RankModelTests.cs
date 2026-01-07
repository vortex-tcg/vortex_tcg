using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class RankModelTests
    {
        [Fact]
        public void Rank_WithUsers()
        {
            DateTime now = DateTime.UtcNow;
            Rank rank = new Rank
            {
                Id = Guid.NewGuid(),
                Label = "Gold",
                nbVictory = 10,
                Users = new List<User>(),
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "R",
                LastName = "U",
                Username = "ru",
                Password = "pwd",
                Email = "r@u.com",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.CONNECTED,
                RankId = rank.Id,
                Rank = rank,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            rank.Users.Add(user);

            User stored = Assert.Single(rank.Users);
            Assert.Equal(rank.Id, stored.RankId);
            Assert.Same(rank, stored.Rank);
            Assert.Equal("Gold", rank.Label);
            Assert.Equal(10, rank.nbVictory);
        }
    }
}
