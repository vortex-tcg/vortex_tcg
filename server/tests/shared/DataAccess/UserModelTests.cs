using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class UserModelTests
    {
        [Fact]
        public void User_WithFriendsAndStatus()
        {
            DateTime now = DateTime.UtcNow;
            User alice = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Doe",
                Username = "alice",
                Password = "pwd",
                Email = "alice@example.com",
                Language = "fr",
                Role = Role.USER,
                Status = UserStatus.IN_QUEUE,
                CurrencyQuantity = 100,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            User bob = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Smith",
                Username = "bob",
                Password = "pwd",
                Email = "bob@example.com",
                Language = "en",
                Role = Role.ADMIN,
                Status = UserStatus.CONNECTED,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            Friend friendship = new Friend
            {
                Id = Guid.NewGuid(),
                FriendUserId = bob.Id,
                FriendUser = bob,
                UserId = alice.Id,
                User = alice,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            alice.Friends.Add(friendship);
            bob.OtherFriends.Add(friendship);

            Assert.Equal(Role.USER, alice.Role);
            Assert.Equal(UserStatus.IN_QUEUE, alice.Status);
            Assert.Equal(100, alice.CurrencyQuantity);
            Friend stored = Assert.Single(alice.Friends);
            Assert.Same(bob, stored.FriendUser);
            Assert.Same(alice, stored.User);
        }
    }
}
