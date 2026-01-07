using System;
using game.Services;
using Xunit;

namespace game.Tests
{
    public class MatchmakerTests
    {
        [Fact]
        public void Enqueue_ShouldMatchTwoPlayers()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck1 = Guid.NewGuid();
            Guid deck2 = Guid.NewGuid();
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();

            (bool matchedFirst, string? roomFirst, _, _, _) = matchmaker.Enqueue("c1", user1, deck1);
            Assert.False(matchedFirst);
            Assert.Null(roomFirst);

            (bool matchedSecond, string? roomSecond, string? otherConnSecond, Guid? otherUserSecond, Guid? otherDeckSecond) = matchmaker.Enqueue("c2", user2, deck2);
            Assert.True(matchedSecond);
            Assert.False(string.IsNullOrWhiteSpace(roomSecond));
            Assert.NotNull(otherConnSecond);
            Assert.Equal(user1, otherUserSecond);
            Assert.Equal(deck1, otherDeckSecond);

            (string? opponent, string? room) = matchmaker.GetOpponent("c1");
            Assert.Equal("c2", opponent);
            Assert.Equal(roomSecond, room);
        }

        [Fact]
        public void LeaveOrDisconnect_ShouldCleanupMappings()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck1 = Guid.NewGuid();
            Guid deck2 = Guid.NewGuid();
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();

            matchmaker.Enqueue("c1", user1, deck1);
            matchmaker.Enqueue("c2", user2, deck2);

            matchmaker.LeaveOrDisconnect("c1");

            (string? opponent, string? room) = matchmaker.GetOpponent("c2");
            Assert.Null(opponent);
            Assert.Null(room);
            string? roomId = matchmaker.GetRoomId("c1");
            Assert.Null(roomId);
        }

        [Fact]
        public void SetName_ShouldStorePlayerName()
        {
            Matchmaker matchmaker = new Matchmaker();
            string connectionId = "conn-123";
            string name = "TestPlayer";

            matchmaker.SetName(connectionId, name);
            string result = matchmaker.GetName(connectionId);

            Assert.Equal(name, result);
        }

        [Fact]
        public void SetName_WithNull_ShouldUseDefaultName()
        {
            Matchmaker matchmaker = new Matchmaker();
            string connectionId = "conn-456";

            matchmaker.SetName(connectionId, null);
            string result = matchmaker.GetName(connectionId);

            Assert.StartsWith("Player-", result);
        }

        [Fact]
        public void GetName_WithUnknownConnection_ShouldReturnDefault()
        {
            Matchmaker matchmaker = new Matchmaker();
            string connectionId = "unknown-conn";

            string result = matchmaker.GetName(connectionId);

            Assert.StartsWith("Player-", result);
        }

        [Fact]
        public void GetRoomId_BeforeMatch_ShouldReturnNull()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck = Guid.NewGuid();
            Guid user = Guid.NewGuid();

            matchmaker.Enqueue("c1", user, deck);

            string? roomId = matchmaker.GetRoomId("c1");
            Assert.Null(roomId);
        }

        [Fact]
        public void GetRoomId_AfterMatch_ShouldReturnRoomId()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck1 = Guid.NewGuid();
            Guid deck2 = Guid.NewGuid();
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();

            matchmaker.Enqueue("c1", user1, deck1);
            (bool matched, string? roomId, _, _, _) = matchmaker.Enqueue("c2", user2, deck2);

            Assert.True(matched);
            Assert.Equal(roomId, matchmaker.GetRoomId("c1"));
            Assert.Equal(roomId, matchmaker.GetRoomId("c2"));
        }

        [Fact]
        public void GetOpponent_BeforeMatch_ShouldReturnNulls()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck = Guid.NewGuid();
            Guid user = Guid.NewGuid();

            matchmaker.Enqueue("c1", user, deck);

            (string? opponent, string? room) = matchmaker.GetOpponent("c1");
            Assert.Null(opponent);
            Assert.Null(room);
        }

        [Fact]
        public void GetOpponent_AfterMatch_ShouldReturnCorrectOpponent()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck1 = Guid.NewGuid();
            Guid deck2 = Guid.NewGuid();
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();

            matchmaker.Enqueue("c1", user1, deck1);
            matchmaker.Enqueue("c2", user2, deck2);

            (string? opponentForC1, string? roomForC1) = matchmaker.GetOpponent("c1");
            (string? opponentForC2, string? roomForC2) = matchmaker.GetOpponent("c2");

            Assert.Equal("c2", opponentForC1);
            Assert.Equal("c1", opponentForC2);
            Assert.Equal(roomForC1, roomForC2);
        }

        [Fact]
        public void GetOpponent_WithUnknownConnection_ShouldReturnNulls()
        {
            Matchmaker matchmaker = new Matchmaker();

            (string? opponent, string? room) = matchmaker.GetOpponent("unknown");
            Assert.Null(opponent);
            Assert.Null(room);
        }

        [Fact]
        public void LeaveOrDisconnect_BeforeMatch_ShouldRemoveFromQueue()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck = Guid.NewGuid();
            Guid user = Guid.NewGuid();

            matchmaker.Enqueue("c1", user, deck);
            matchmaker.LeaveOrDisconnect("c1");

            // Player 2 joins - should not match with disconnected player
            Guid deck2 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();
            (bool matched, string? roomId, _, _, _) = matchmaker.Enqueue("c2", user2, deck2);

            Assert.False(matched);
            Assert.Null(roomId);
        }

        [Fact]
        public void LeaveOrDisconnect_CleanupOpponent_ShouldFreeOpponent()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck1 = Guid.NewGuid();
            Guid deck2 = Guid.NewGuid();
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();

            matchmaker.Enqueue("c1", user1, deck1);
            matchmaker.Enqueue("c2", user2, deck2);

            // c1 leaves
            matchmaker.LeaveOrDisconnect("c1");

            // c2's room should be cleaned up
            string? c2Room = matchmaker.GetRoomId("c2");
            Assert.Null(c2Room);
        }

        [Fact]
        public void MultipleEnqueues_ShouldMatchInOrder()
        {
            Matchmaker matchmaker = new Matchmaker();

            matchmaker.Enqueue("c1", Guid.NewGuid(), Guid.NewGuid());
            matchmaker.Enqueue("c2", Guid.NewGuid(), Guid.NewGuid());
            // c1 and c2 should be matched

            (bool matched3, string? room3, string? otherConn3, _, _) = matchmaker.Enqueue("c3", Guid.NewGuid(), Guid.NewGuid());
            Assert.False(matched3);

            (bool matched4, string? room4, string? otherConn4, _, _) = matchmaker.Enqueue("c4", Guid.NewGuid(), Guid.NewGuid());
            Assert.True(matched4);
            Assert.Equal("c3", otherConn4);
        }

        [Fact]
        public void LeaveOrDisconnect_WithNonExistentConnection_ShouldNotThrow()
        {
            Matchmaker matchmaker = new Matchmaker();

            Exception ex = Record.Exception(() => matchmaker.LeaveOrDisconnect("nonexistent"));
            Assert.Null(ex);
        }

        [Fact]
        public void Enqueue_SamePlayerTwice_ShouldStillWork()
        {
            Matchmaker matchmaker = new Matchmaker();
            Guid deck = Guid.NewGuid();
            Guid user = Guid.NewGuid();

            matchmaker.Enqueue("c1", user, deck);
            // Leave and rejoin
            matchmaker.LeaveOrDisconnect("c1");
            (bool matched, _, _, _, _) = matchmaker.Enqueue("c1", user, deck);

            Assert.False(matched); // Still waiting
        }
    }
}
