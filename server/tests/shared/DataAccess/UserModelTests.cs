using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class UserModelTests
    {
        [Fact]
        public void User_CollectionId_And_Collection_AreNullByDefault()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Password = "pwd",
                Email = "test@example.com",
                Language = "fr",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            Assert.Null(user.CollectionId);
            Assert.Null(user.Collection);
        }

        [Fact]
        public void User_CollectionId_And_Collection_CanBeAssigned()
        {
            Guid collectionId = Guid.NewGuid();
            Collection collection = new Collection
            {
                Id = collectionId,
                Cards = new List<CollectionCard>(),
                Champions = new List<CollectionChampion>()
            };

            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Password = "pwd",
                Email = "test@example.com",
                Language = "fr",
                CollectionId = collectionId,
                Collection = collection,
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            Assert.Equal(collectionId, user.CollectionId);
            Assert.Same(collection, user.Collection);
            Assert.Equal(collectionId, user.Collection.Id);
        }

        [Fact]
        public void User_Decks_IsNullByDefault_And_CanBeAssigned()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Password = "pwd",
                Email = "test@example.com",
                Language = "fr",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            Assert.Null(user.Decks);

            Deck deck = new Deck { Id = Guid.NewGuid(), Label = "Deck1", UserId = user.Id };
            user.Decks = new List<Deck> { deck };

            Deck stored = Assert.Single(user.Decks);
            Assert.Equal("Deck1", stored.Label);
            Assert.Equal(user.Id, stored.UserId);
        }

        [Fact]
        public void User_Logs_IsNullByDefault_And_CanBeAssigned()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                Password = "pwd",
                Email = "test@example.com",
                Language = "fr",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            Assert.Null(user.Logs);

            Game game = new Game { Id = Guid.NewGuid(), UserId = user.Id, Status = GameEndStatus.WIN };
            user.Logs = new List<Game> { game };

            Game stored = Assert.Single(user.Logs);
            Assert.Equal(GameEndStatus.WIN, stored.Status);
            Assert.Equal(user.Id, stored.UserId);
        }

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
