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
        ApiDeckDataDto apiDeck1 = await _deckApiClient.GetDeckDataAsync(p1.deckId, ct);
        ApiDeckDataDto apiDeck2 = await _deckApiClient.GetDeckDataAsync(p2.deckId, ct);
        DeckData deckData1 = DeckDataMapper.Map(p1.deckId, apiDeck1);
        DeckData deckData2 = DeckDataMapper.Map(p2.deckId, apiDeck2);
        int globalGameCardId = 1;
        AssignGlobalGameCardIds(deckData1.Cards, ref globalGameCardId);
        AssignGlobalGameCardIds(deckData2.Cards, ref globalGameCardId);
        Player player1 = new Player(
            p1.userId,
            p1.deckId,
            new PlayerDeck(deckData1.Cards),
            deckData1.Champion
        );

        Player player2 = new Player(
            p2.userId,
            p2.deckId,
            new PlayerDeck(deckData2.Cards),
            deckData2.Champion
        );
        Match match = new Match(player1, player2);
        Random rng = Random.Shared;
        player1.Deck.Shuffle(rng);
        player2.Deck.Shuffle(rng);

        DrawOpeningHand(player1, 6);
        DrawOpeningHand(player2, 5);

        match.Start();

        return match;
    }

    private static void AssignGlobalGameCardIds(List<GameCardDto> cards, ref int globalId)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].GameCardId = (GameCardId)globalId;
            globalId++;
        }
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
