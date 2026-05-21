using game.Domaine.Match.Agregate;
using game.Domaine.Match.Interface;
using game.Infrastructure.DTO;
using Microsoft.Extensions.Logging;

namespace game.Application.Factory;

using Mapping;
using Domaine.Match.Entity;
using Domaine.Match.ValueObject;
using Infrastructure.Interface;

public sealed class CreateMatchFactory
{
    private readonly IDeckApiClient _deckApiClient;
    private readonly ILogger<CreateMatchFactory> _logger;

    public CreateMatchFactory(IDeckApiClient deckApiClient, ILogger<CreateMatchFactory> logger)
    {
        _deckApiClient = deckApiClient;
        _logger = logger;
    }

    public async Task<Match> CreateMatchAsync(
        (UserId userId, DeckId deckId) p1,
        (UserId userId, DeckId deckId) p2,
        CancellationToken ct = default)
    {
        int globalGameCardId = 1;

        _logger.LogDebug("[FACTORY] Génération joueur 1 — userId={UserId} deckId={DeckId}", p1.userId, p1.deckId);
        (Player player1, int nextId1) = await GeneratePlayerAsync(p1, globalGameCardId, ct);
        _logger.LogDebug("[FACTORY] Joueur 1 généré — {CardCount} cartes", player1.Deck.Count);

        _logger.LogDebug("[FACTORY] Génération joueur 2 — userId={UserId} deckId={DeckId}", p2.userId, p2.deckId);
        (Player player2, int nextId2) = await GeneratePlayerAsync(p2, nextId1, ct);
        _logger.LogDebug("[FACTORY] Joueur 2 généré — {CardCount} cartes", player2.Deck.Count);

        IPhase initialPhase = new StandByPhase();

        Match match = new Match(player1, player2, initialPhase);

        Random rng = Random.Shared;
        player1.Deck.Shuffle(rng);
        player2.Deck.Shuffle(rng);

        DrawOpeningHand(player1, 6);
        DrawOpeningHand(player2, 5);
        _logger.LogDebug("[FACTORY] Mains initiales distribuées — p1={P1Hand} cartes | p2={P2Hand} cartes", player1.Hand.Count, player2.Hand.Count);

        match.Start();
        _logger.LogInformation("[FACTORY] Match démarré — matchId={MatchId} p1={P1} p2={P2}", match.MatchId, p1.userId, p2.userId);
        return match;
    }

    private async Task<(Player player, int nextGlobalGameCardId)> GeneratePlayerAsync(
        (UserId userId, DeckId deckId) p,
        int startGlobalGameCardId,
        CancellationToken ct)
    {
        _logger.LogDebug("[FACTORY] Appel API deck — deckId={DeckId}", p.deckId);
        ApiDeckDataDto apiDeck = await _deckApiClient.GetDeckDataAsync(p.deckId, ct);
        _logger.LogDebug("[FACTORY] API deck OK — {CardCount} cartes reçues pour deckId={DeckId}", apiDeck.Cards.Count, p.deckId);
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
