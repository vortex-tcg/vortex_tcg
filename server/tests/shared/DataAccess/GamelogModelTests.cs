using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using GameModel = VortexTCG.DataAccess.Models.Game;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class GamelogModelTests
    {
        [Fact]
        public void Gamelog_WithActions_And_Game()
        {
            DateTime now = DateTime.UtcNow;
            Guid gamelogId = Guid.NewGuid();

            Gamelog log = new Gamelog
            {
                Id = gamelogId,
                Label = "Match1",
                TurnNumber = 3,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Actions = new List<ActionType>()
            };

            ActionType parent = new ActionType
            {
                Id = Guid.NewGuid(),
                actionDescription = "Parent",
                GameLogId = gamelogId,
                Gamelog = log,
                ParentId = Guid.Empty,
                Parent = null!,
                Childs = new List<ActionType>(),
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            ActionType child = new ActionType
            {
                Id = Guid.NewGuid(),
                actionDescription = "Child",
                GameLogId = gamelogId,
                Gamelog = log,
                ParentId = parent.Id,
                Parent = parent,
                Childs = new List<ActionType>(),
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            parent.Childs.Add(child);
            log.Actions!.Add(parent);
            log.Actions.Add(child);

            GameModel game = new GameModel
            {
                Id = Guid.NewGuid(),
                Status = GameEndStatus.WIN,
                UserId = Guid.NewGuid(),
                User = new User { Id = Guid.NewGuid(), FirstName = "G", LastName = "M", Username = "gm", Password = "p", Email = "g@m.com", Language = "en", Role = Role.USER, Status = UserStatus.IN_GAME, CreatedAtUtc = now, CreatedBy = "seed", Friends = new List<Friend>(), OtherFriends = new List<Friend>() },
                GamelogId = gamelogId,
                Gamelog = log,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            log.User = game;

            Assert.Equal("Match1", log.Label);
            Assert.Equal(3, log.TurnNumber);
            Assert.Equal(game, log.User);
            Assert.Equal(gamelogId, game.GamelogId);
            Assert.Same(log, game.Gamelog);
            Assert.Contains(parent, log.Actions!);
            Assert.Contains(child, log.Actions!);
            ActionType storedChild = Assert.Single(parent.Childs);
            Assert.Same(child, storedChild);
            Assert.Same(parent, child.Parent);
        }
    }
}
