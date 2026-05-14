using System.Reflection;
using game.Infrastructure;
using game.Infrastructure.Manager;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Helpers;

public static class AppServiceHelpers
{
    public static void AddMatchToRoom(MatchAggregate match)
    {
        FieldInfo field = typeof(RoomManager)
            .GetField("_matches", BindingFlags.NonPublic | BindingFlags.Instance)!;
        List<MatchAggregate> list = (List<MatchAggregate>)field.GetValue(RoomManager.Instance)!;
        lock (list) { list.Add(match); }
    }

    public static void ClearRoom()
    {
        FieldInfo field = typeof(RoomManager)
            .GetField("_matches", BindingFlags.NonPublic | BindingFlags.Instance)!;
        List<MatchAggregate> list = (List<MatchAggregate>)field.GetValue(RoomManager.Instance)!;
        lock (list) { list.Clear(); }
    }

    public static void ClearMatchmakerQueue()
    {
        object matchmaker = RoomManager.Instance.Matchmaker;
        Type type = matchmaker.GetType();

        FieldInfo? queueField = type.GetField("_queue", BindingFlags.NonPublic | BindingFlags.Instance);
        if (queueField?.GetValue(matchmaker) is System.Collections.IDictionary queue)
            queue.Clear();

        FieldInfo? eventsField = type.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventsField?.GetValue(matchmaker) is System.Collections.IList events)
            events.Clear();
    }

    public static Mock<IClientProxy> ConfigureCallManager()
    {
        Mock<IHubContext<GameHubClean>> mockHub = new Mock<IHubContext<GameHubClean>>();
        Mock<IHubClients> mockClients = new Mock<IHubClients>();
        Mock<IClientProxy> mockProxy = new Mock<IClientProxy>();

        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockProxy.Object);
        mockProxy
            .Setup(p => p.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CallManager.Configure(mockHub.Object);
        return mockProxy;
    }
}
