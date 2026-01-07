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
    }
}
