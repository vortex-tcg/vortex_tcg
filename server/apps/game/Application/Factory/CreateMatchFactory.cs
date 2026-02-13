using game.Domaine.Match.Agregate;
using game.Infrastructure.DTO;

namespace game.Application.Factory;

using Mapping;
using Domaine.Match.Entity;
using Domaine.Match.ValueObject;
using Infrastructure.Interface;

public sealed class CreateMatchFactory
{
    private readonly IDeckApiClient _deckApiClient;

    public CreateMatchFactory(IDeckApiClient deckApiClient)
    {
        _deckApiClient = deckApiClient;
    }

    public async Task<Match> CreateMatchAsync(
        (UserId userId, DeckId deckId) p1,
        (UserId userId, DeckId deckId) p2,
        CancellationToken ct = default)
    {
        int globalGameCardId = 1;

        (Player player1, int nextId1) = await GeneratePlayerAsync(p1, globalGameCardId, ct);
        (Player player2, int nextId2) = await GeneratePlayerAsync(p2, nextId1, ct);

        Match match = new Match(player1, player2);

        Random rng = Random.Shared;
        player1.Deck.Shuffle(rng);
        player2.Deck.Shuffle(rng);

        DrawOpeningHand(player1, 6);
        DrawOpeningHand(player2, 5);

        match.Start();
        return match;
    }

    private async Task<(Player player, int nextGlobalGameCardId)> GeneratePlayerAsync(
        (UserId userId, DeckId deckId) p,
        int startGlobalGameCardId,
        CancellationToken ct)
    {
        ApiDeckDataDto apiDeck = await _deckApiClient.GetDeckDataAsync(p.deckId, ct);
        DeckData deckData = DeckDataMapper.Map(p.deckId, apiDeck);

        int nextId = AssignGlobalGameCardIds(deckData.Cards, startGlobalGameCardId);

        Player player = new Player(
            p.userId,
            p.deckId,
            new PlayerDeck(deckData.Cards),
            deckData.Champion
        );

        return (player, nextId);
    }

    private static int AssignGlobalGameCardIds(List<GameCardDto> cards, int startId)
    {
        int id = startId;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].GameCardId = (GameCardId)id;
            id++;
        }
        return id;
    }


    private static void DrawOpeningHand(Player p, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameCardDto? drawn = p.Deck.DrawOne();
            if (drawn == null) break;
            p.Hand.Add(drawn);
        }
    }
}
