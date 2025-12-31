// =============================================
// FICHIER: Object/RoomObject.cs
// =============================================
// RÔLE PRINCIPAL:
// Représente l'état complet d'une partie de jeu entre 2 joueurs.
// Contient tous les éléments de jeu pour chaque joueur: deck, main, board, champion, cimetière.
//
// RESPONSABILITÉS:
// 1. Stocker l'état de jeu pour les 2 joueurs
// 2. Initialiser les decks et champions à partir des IDs de base de données
// 3. Fournir un accès centralisé à tous les composants de jeu
// 4. Gérer les phases et les tours de jeu
//
// ARCHITECTURE:
// - Chaque joueur (user_1 et user_2) possède:
//   * Un Deck (pioche de cartes)
//   * Une Hand (main de cartes)
//   * Un Board (terrain de jeu avec 5 emplacements)
//   * Un Champion (héros avec HP, gold, etc.)
//   * Un Graveyard (cimetière des cartes défaussées)
//
// PHASES DE JEU:
// - Draw: Pioche automatique d'une carte
// - Placement: Le joueur pose des cartes (attente manuelle)
// - Attack: Le joueur attaque (auto-skip si aucune action possible)
// - Defense: L'adversaire défend (auto-skip si aucune action possible)
// - EndTurn: Fin du tour, passage au joueur suivant
//
// INITIALISATION:
// Les méthodes setUser1() et setUser2() doivent être appelées pour charger
// les decks depuis la base de données et configurer les champions.
// =============================================
using VortexTCG.Game.DTO;
using VortexTCG.Game.Interface;
using VortexTCG.DataAccess.Models;
using System.Timers;

namespace VortexTCG.Game.Object
{

    public class Room
    {
        private readonly IRoomActionEventListener _event;

        #region Identifiants des joueurs

            private Guid _user_1;
            private Guid _user_2;

        #endregion

        #region État du tour et des phases

            private Guid _activePlayerId;
            private GamePhase _currentPhase;
            private int _turnNumber;
            private bool _gameStarted;
            private readonly System.Timers.Timer _timer;
            public event Action OnTimeUp;

        #endregion

        #region Etat de l'attaque et de la défense

            private readonly AttackHandler _attackHandler = new AttackHandler();

        #endregion

        #region État de jeu - Joueur 1

            private readonly Deck _deck_user_1;
            private readonly Hand _hand_user_1;
            private readonly Champion _champion_user_1;
            private readonly Graveyard _graveyard_user_1;
            private readonly Board _board_user_1;

        #endregion

        #region État de jeu - Joueur 2

            private readonly Deck _deck_user_2;
            private readonly Hand _hand_user_2;
            private readonly Champion _champion_user_2;
            private readonly Graveyard _graveyard_user_2;
            private readonly Board _board_user_2;

        #endregion

        #region Constructeur

            public Room(IRoomActionEventListener listener)
            {
                _deck_user_1 = new Deck();
                _deck_user_2 = new Deck();

                _hand_user_1 = new Hand();
                _hand_user_2 = new Hand();

                _champion_user_1 = new Champion();
                _champion_user_2 = new Champion();

                _graveyard_user_1 = new Graveyard();
                _graveyard_user_2 = new Graveyard();

                _board_user_1 = new Board();
                _board_user_2 = new Board();
            
                _timer = new System.Timers.Timer(60000); 
                _timer.Elapsed += HandleTimerElapsed;

                _event = listener;
            }

        #endregion

        #region Méthodes d'initialisation

            /// <summary>
            /// Configure le joueur 1 avec son deck et son champion.
            /// Charge les cartes depuis la base de données et initialise le champion.
            /// </summary>
            /// <param name="user">ID utilisateur (base de données) du joueur 1</param>
            /// <param name="deck">ID du deck sélectionné par le joueur 1</param>
            /// <remarks>
            /// APPELÉ PAR: RoomService.SetPlayerDeck() quand les 2 joueurs sont prêts.
            /// EFFETS:
            /// - Enregistre l'ID utilisateur
            /// - Charge les cartes du deck depuis la BDD (via DeckFactory)
            /// - Configure le champion avec ses stats de base (30 HP, 1 gold, etc.)
            /// </remarks>
            public Task setUser1(Guid user, Guid deck)
            {
                _user_1 = user;
                _deck_user_1.initDeck(deck);
                _champion_user_1.initChampion(deck);

                return Task.CompletedTask;
            }

            /// <summary>
            /// Configure le joueur 2 avec son deck et son champion.
            /// Charge les cartes depuis la base de données et initialise le champion.
            /// </summary>
            /// <param name="user">ID utilisateur (base de données) du joueur 2</param>
            /// <param name="deck">ID du deck sélectionné par le joueur 2</param>
            /// <remarks>
            /// APPELÉ PAR: RoomService.SetPlayerDeck() quand les 2 joueurs sont prêts.
            /// EFFETS:
            /// - Enregistre l'ID utilisateur
            /// - Charge les cartes du deck depuis la BDD (via DeckFactory)
            /// - Configure le champion avec ses stats de base (30 HP, 1 gold, etc.)
            /// </remarks>
            public Task setUser2(Guid user, Guid deck)
            {
                _user_2 = user;
                _deck_user_2.initDeck(deck);
                _champion_user_2.initChampion(deck);

                return Task.CompletedTask;
            }

        #endregion

        #region Méthodes utiles

            private bool checkIfPlayer1(Guid userId)
            => userId == _user_1;

            private Board getPlayerBoard(bool isPlayer1)
            => isPlayer1 ? _board_user_1 : _board_user_2;

            private Hand getPlayerHand(bool isPlayer1)
            => isPlayer1 ? _hand_user_1 : _hand_user_2;

            private Graveyard getPlayerGraveyard(bool isPlayer1)
            => isPlayer1 ? _graveyard_user_1 : _graveyard_user_2;

            private Deck getPlayerDeck(bool isPlayer1)
            => isPlayer1 ? _deck_user_1 : _deck_user_2;

            private Champion getPlayerChamp(bool isPlayer1)
            => isPlayer1 ? _champion_user_1 : _champion_user_2;

            private Guid getPlayerId(bool isPlayer1)
            => isPlayer1 ? _user_1 : _user_2;

        #endregion

        #region Actions de pioche

            /// <summary>
            /// Fait piocher des cartes à un joueur.
            /// </summary>
            /// <param name="playerId">ID du joueur qui pioche.</param>
            /// <param name="cardCount">Nombre de cartes à piocher.</param>
            /// <returns>Résultat pour le joueur et l'adversaire, ou null si joueur invalide.</returns>
            public void DrawCards(Guid playerId, int cardCount)
            {
                if (playerId != _user_1 && playerId != _user_2) return;
                if (cardCount <= 0) return;

                bool isPlayer1 = checkIfPlayer1(playerId);
                Deck deck = getPlayerDeck(isPlayer1);
                Hand hand = getPlayerHand(isPlayer1);
                Graveyard graveyard = getPlayerGraveyard(isPlayer1);

                Champion champion = getPlayerChamp(isPlayer1);

                var drawnCards = new List<DrawnCardDTO>();         
                var sentToGraveyard = new List<DrawnCardDTO>();             
                int fatigueCount = 0;
                int baseFatigue = champion.GetFatigue();

                for (int i = 0; i < cardCount; i++)
                {
                    Card? card = deck.DrawCard();
                    if (card != null)
                    {
                        DrawnCardDTO dto = new DrawnCardDTO
                        {
                            GameCardId = card.GetGameCardId(),
                            Name = card.GetName(),
                            Hp = card.GetHp(),
                            Attack = card.GetAttack(),
                            Cost = card.GetCost(),
                            Description = card.GetDescription(),
                            CardType = card.GetCardType()
                        };
                        if (!hand.AddCard(card))
                        {
                            graveyard.AddCard(card);
                            sentToGraveyard.Add(dto);
                            continue;
                        }
                        drawnCards.Add(dto);
                    }
                    else
                    {
                        fatigueCount++;
                        champion.ApplyFatigueDamage();
                    }
                }

                DrawCardsResultDTO data = new DrawCardsResultDTO
                {
                    PlayerResult = new DrawResultForPlayerDTO
                    {
                        PlayerId = isPlayer1 ? _user_1 : _user_2,
                        DrawnCards = drawnCards,
                        FatigueCount = fatigueCount,
                        BaseFatigue = baseFatigue,
                        SentToGraveyard = sentToGraveyard, 
                    },
                    OpponentResult = new DrawResultForOpponentDTO
                    {
                        PlayerId = isPlayer1 ? _user_2 : _user_1,
                        CardsDrawnCount = drawnCards.Count,
                        FatigueCount = fatigueCount,
                        BaseFatigue = baseFatigue,
                        CardsBurnedCount = sentToGraveyard.Count,
                    }
                };
                _event.sendDrawCardsData(data);
            }

        #endregion

        #region Actions jouer une carte

            public PlayCardResponseDto PlayCard(Guid userId, int cardId, int location) {
                if (userId != _activePlayerId) return null;
                if (_currentPhase != GamePhase.PLACEMENT) return null;
                
                bool isPlayer1 = checkIfPlayer1(userId);
                Hand activeHand = isPlayer1 ? _hand_user_1 : _hand_user_2;

                if (!activeHand.TryGetCard(cardId, out Card? playedCard)) return null;

                switch(playedCard.GetCardType()) {
                    case CardType.GUARD:
                        return PlayGuardCard(isPlayer1, location, playedCard, activeHand);
                    case CardType.SPELL:
                    case CardType.EQUIPMENT:
                    default:
                        return null;
                }

            }

            private PlayCardResponseDto PlayGuardCard(bool isPlayer1, int location, Card card, Hand activeHand) {
                Board activeBoard = getPlayerBoard(isPlayer1);
                Champion activeChamp = getPlayerChamp(isPlayer1);

                if (location < 0 || location > 5) return null;
                else if (!activeBoard.IsAvailable(location)) return null;
                else if (!activeChamp.TryPaiedCard(card.GetCost())) return null;

                activeHand.DeleteFromId(card.GetGameCardId());
                card.AddState(CardState.ENGAGE);
                activeBoard.PosCard(card, location);
                activeChamp.PayCard(card.GetCost());

                return new PlayCardResponseDto {
                    PlayerResult = new PlayCardPlayerResultDto {
                        PlayerId = isPlayer1 ? _user_1 : _user_2,
                        PlayedCard = card.FormatGameCardDto(),
                        Champion = activeChamp.FormatPlayCardChampionDto(),
                        location = location,
                        canPlayed = true
                    },
                    OpponentResult = new PlayCardOpponentResultDto {
                        PlayerId = isPlayer1 ? _user_2 : _user_1,
                        PlayedCard = card.FormatGameCardDto(),
                        Champion = activeChamp.FormatPlayCardChampionDto(),
                        location = location
                    }
                };
            }

        #endregion

        #region Gestion de l'attaque

            private AttackResponseDto UnEngageAttackCard(Board playerBoard, int pos, bool isPlayer1) {
                playerBoard.UnEngageAttackCard(pos);
                _attackHandler.RemoveAttack(playerBoard.GetCardFromSlot(pos));
                return _attackHandler.FormatAttackResponseDto(getPlayerId(isPlayer1), getPlayerId(!isPlayer1));
            }

            private AttackResponseDto EngageAttackCard(Board playerBoard, int pos, bool isPlayer1) {
                playerBoard.EngageAttackCard(pos);
                _attackHandler.AddAttack(playerBoard.GetCardFromSlot(pos));
                return _attackHandler.FormatAttackResponseDto(getPlayerId(isPlayer1), getPlayerId(!isPlayer1));
            }

            public AttackResponseDto HandleAttackEvent(Guid userId, int cardId) {
                if (userId != _activePlayerId) return null;

                bool isPlayer1 = checkIfPlayer1(userId);
                Board playerBoard = getPlayerBoard(isPlayer1);

                if (!playerBoard.TryGetCardPos(cardId, out int pos)) return null;
                Console.WriteLine("la position est égal à " + pos);
                CardSlotState slotState = playerBoard.canAttackSpot(pos);

                if (slotState == CardSlotState.ATTACK_ENGAGE) return UnEngageAttackCard(playerBoard, pos, isPlayer1);
                else if (slotState == CardSlotState.CAN_ATTACK) return EngageAttackCard(playerBoard, pos, isPlayer1);
                else {
                    Console.WriteLine("Mais c'était sûr enfaite !!!!");    
                    return null;
                }
            }

        #endregion

        #region Gestion de la defense

            private DefenseResponseDto UnEngageDefenseCard(Board board, int pos, bool isPlayer1) {
                board.UnEngageDefenseCard(pos);
                _attackHandler.RemoveDefense(board.GetCardFromSlot(pos));
                return _attackHandler.FormatDefenseResponseDto(getPlayerId(isPlayer1), getPlayerId(!isPlayer1));
            }

            private DefenseResponseDto EngageDefenseCard(Board playerBoard, Board opponentBoard, int pos, int opponentPos, bool isPlayer1) {
                playerBoard.EngageDefenseCard(pos);
                _attackHandler.AddDefense(playerBoard.GetCardFromSlot(pos), opponentBoard.GetCardFromSlot(opponentPos));
                return _attackHandler.FormatDefenseResponseDto(getPlayerId(isPlayer1), getPlayerId(!isPlayer1));
            }

public DefenseResponseDto HandleDefenseEvent(Guid userId, int cardId, int opponentCardId)
{
    Console.WriteLine($"[HandleDefenseEvent] user={userId} cardId={cardId} opponentCardId={opponentCardId} " +
                      $"active={_activePlayerId} phase={_currentPhase} turn={_turnNumber} started={_gameStarted}");

    if (!_gameStarted)
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: game not started");
        return null;
    }

    if (userId != _activePlayerId)
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: not active player");
        return null;
    }

    if (_currentPhase != GamePhase.DEFENSE)
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: wrong phase (need DEFENSE)");
        return null;
    }

    bool isPlayer1 = checkIfPlayer1(userId);
    Board playerBoard = getPlayerBoard(isPlayer1);

    if (!playerBoard.TryGetCardPos(cardId, out int pos))
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: defense card not found on player's board");
        return null;
    }

    CardSlotState defenseCardState = playerBoard.canDefendSpot(pos);
    Console.WriteLine($"[HandleDefenseEvent] defenseCardPos={pos} canDefendSpot={defenseCardState}");

    if (opponentCardId < 0 && defenseCardState != CardSlotState.DEFENSE_ENGAGE)
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: opponentCardId<0 but state != DEFENSE_ENGAGE");
        return null;
    }

    if (opponentCardId >= 0 && defenseCardState != CardSlotState.CAN_DEFEND)
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: opponentCardId>=0 but state != CAN_DEFEND");
        return null;
    }

    if (opponentCardId < 0)
    {
        Console.WriteLine("[HandleDefenseEvent] -> UnEngageDefenseCard");
        return UnEngageDefenseCard(playerBoard, pos, isPlayer1);
    }

    Board opponentBoard = getPlayerBoard(!isPlayer1);

    if (!opponentBoard.TryGetCardPos(opponentCardId, out int opponentPos))
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: opponentCardId not found on opponent board");
        return null;
    }

    CardSlotState attackCardState = opponentBoard.canAttackSpot(opponentPos);
    Console.WriteLine($"[HandleDefenseEvent] opponentPos={opponentPos} attackCardState={attackCardState}");

    if (attackCardState != CardSlotState.ATTACK_ENGAGE)
    {
        Console.WriteLine("[HandleDefenseEvent] REJECT: opponent card is not ATTACK_ENGAGE");
        return null;
    }

    Console.WriteLine("[HandleDefenseEvent] -> EngageDefenseCard");
    return EngageDefenseCard(playerBoard, opponentBoard, pos, opponentPos, isPlayer1);
}


        #endregion

        #region Resolution de bataille
            
            private void HandleCardDeath(bool isPlayer1, Card card) {
                Board board = isPlayer1 ? _board_user_1 : _board_user_2;
                Graveyard graveyard = isPlayer1 ? _graveyard_user_1 : _graveyard_user_2;

                board.clearSpot(card.GetGameCardId());
                graveyard.AddCard(card);
            }

            private BattleDataDto HandleAgainstCardBattle(bool isDefenderPlayer, Card attacker, DefenseCard defender, Champion attackerChamp, Champion defenderChamp)
            {
                int attackerDamageDeal = defender.card.ApplyDamage(attacker);
                int defenderDamageDeal = attacker.ApplyDamage(defender.card);
                bool isDefenderDead = defender.card.IsDead();
                bool isAttackerDead = attacker.IsDead();

                if (isAttackerDead) {
                    HandleCardDeath(!isDefenderPlayer, attacker);
                }
                if (isDefenderDead) {
                    HandleCardDeath(isDefenderPlayer, defender.card);
                }
                return new BattleDataDto{
                    isAgainstChamp = false,
                    againstCard = new BattleAgainstCardDataDto {
                        isAttackerDead = isAttackerDead,
                        isDefenderDead = isDefenderDead,
                        attackerDamageDeal = attackerDamageDeal,
                        defenderDamageDeal = defenderDamageDeal,
                        attackerCard = attacker.FormatGameCardDto(),
                        attackerChamp = attackerChamp.FormatBattleChampionDto(),
                        defenderCard = defender.card.FormatGameCardDto(),
                        defenderChamp = defenderChamp.FormatBattleChampionDto()
                    },
                    againstChamp = null
                };
            }

            private BattleDataDto HandleAgainstChampBattle(Card attacker, Champion attackerChamp, Champion defenderChamp)
            {
                int damageDeal = defenderChamp.ApplyDamage(attacker);
                bool isChampDead = defenderChamp.IsDead();
                return new BattleDataDto{
                    isAgainstChamp = true,
                    againstChamp = new BattlaAgainstChampDataDto {
                        isChampDead = isChampDead,
                        isCardDead = false,
                        attackerDamageDeal = damageDeal,
                        championDamageDeal = 0,
                        attackerCard = attacker.FormatGameCardDto(),
                        attackerChamp = attackerChamp.FormatBattleChampionDto(),
                        defenderChamp = defenderChamp.FormatBattleChampionDto()
                    },
                    againstCard = null
                };
            }

            /// <summary>
            /// Résout le combat entre les deux joueurs.
            /// </summary>
            private void ResolveBattle()
            {
                Guid defenderId = _activePlayerId;
                Guid attackerId = GetOpponentId(defenderId);

                bool defenderIsPlayer1 = checkIfPlayer1(defenderId);

                Console.WriteLine($"[ResolveBattle] turn={_turnNumber} phase={_currentPhase} " +
                                  $"attackerId={attackerId} defenderId={defenderId} defenderIsP1={defenderIsPlayer1}");

                List<Card> attackers = _attackHandler.GetAttacker() ?? new List<Card>();
                List<DefenseCard> defenders = _attackHandler.GetDefender() ?? new List<DefenseCard>();

                Console.WriteLine($"[ResolveBattle] attackers={attackers.Count} defenders={defenders.Count}");

                Champion defenderChamp = getPlayerChamp(defenderIsPlayer1);
                Champion attackerChamp = getPlayerChamp(!defenderIsPlayer1);

                List<BattleDataDto> battleEvent = new List<BattleDataDto>();

                foreach (Card attacker in attackers)
                {
                    if (attacker == null) continue;

                    int attackerCardId = attacker.GetGameCardId();
                    int matchingDefenders = defenders.Count(d => d != null && d.oppositeCardId == attackerCardId);

                    Console.WriteLine(
                        $"[ResolveBattle] attackerCardId={attackerCardId} matchingDefenders={matchingDefenders}");

                    if (matchingDefenders == 1)
                    {
                        DefenseCard defender = _attackHandler.GetSpecificDefender(attackerCardId);
                        if (defender == null || defender.card == null)
                        {
                            Console.WriteLine("[ResolveBattle] WARN: defender mapping missing -> fallback vs champ");
                            battleEvent.Add(HandleAgainstChampBattle(attacker, attackerChamp, defenderChamp));
                        }
                        else
                        {
                            battleEvent.Add(HandleAgainstCardBattle(defenderIsPlayer1, attacker, defender,
                                attackerChamp, defenderChamp));
                        }
                    }
                    else
                    {
                        battleEvent.Add(HandleAgainstChampBattle(attacker, attackerChamp, defenderChamp));
                    }
                }

                BattlesDataDto payload = new BattlesDataDto { battles = battleEvent };

                Console.WriteLine($"[ResolveBattle] sending battles={payload.battles.Count} " +
                                  $"-> attacker={attackerId} defender={defenderId}");
                _event.sendBattleResolveData(payload, attackerId, defenderId);
            }

            #endregion

        #region Gestion des phases et tours

            /// <summary>
            /// Handle when user don't play during 1 minute
            /// </summary>
            public void StartTimer(int seconds)
            {
                _timer.Interval = seconds * 1000;
                _timer.Start();
            }
            public void StopTimer(){
                _timer.Stop();
            }
            private void HandleTimerElapsed(object? sender, ElapsedEventArgs e)
            {
                _timer.Stop();
                OnTimeUp?.Invoke();
            }

            /// <summary>
            /// Démarre la partie. Le joueur 1 commence toujours.
            /// Initialise le tour 1 en phase Draw.
            /// </summary>
            /// <returns>DTO avec l'état initial de la partie</returns>
            public PhaseChangeResultDTO StartGame()
            {
                _gameStarted = true;
                _turnNumber = 1;
                _activePlayerId = _user_1;
                _currentPhase = GamePhase.PLACEMENT;

                DrawCards(_user_1, 6);
                DrawCards(_user_2, 5);

                _champion_user_1.SetBaseGold(1);
                _champion_user_2.SetBaseGold(1);

                StartTimer(60);
                return new PhaseChangeResultDTO
                {
                    CurrentPhase = _currentPhase,
                    ActivePlayerId = _activePlayerId,
                    TurnNumber = _turnNumber,
                    AutoChanged = false,
                    AutoChangeReason = null,
                    CanAct = true
                };
            }
            public PhaseChangeResultDTO GetState()
            {
                return new PhaseChangeResultDTO
                {
                    CurrentPhase = _currentPhase,
                    ActivePlayerId = _activePlayerId,
                    TurnNumber = _turnNumber,
                    AutoChanged = false,
                    AutoChangeReason = null,
                    CanAct = _gameStarted
                };
            }

            /// <summary>
            /// Change de phase pour le joueur actif.
            /// Gère les auto-skips selon les règles:
            /// - Draw: auto-skip après la pioche automatique
            /// - Placement: attente manuelle (le joueur doit appeler ChangePhase)
            /// - Attack: auto-skip si aucune carte sur le board ne peut attaquer
            /// - Defense: auto-skip si l'adversaire n'a pas de cartes pouvant défendre
            /// - EndTurn: passe au joueur suivant et retourne en Draw
            /// </summary>
            /// <param name="playerId">ID du joueur qui demande le changement</param>
            /// <returns>Résultat du changement de phase, ou null si non autorisé</returns>
            public PhaseChangeResultDTO? ChangePhase(Guid playerId)
            {
                // Seul le joueur actif peut changer de phase (sauf Defense où c'est l'adversaire)
                if (!_gameStarted) return null;
                else if (playerId != _activePlayerId) return null;

                return AdvancePhase();
            }

            /// <summary>
            /// Avance à la phase suivante avec gestion des auto-skips.
            /// </summary>
            /// <returns>Résultat du changement de phase</returns>
            private PhaseChangeResultDTO AdvancePhase()
            {
                bool isPlayer1 = checkIfPlayer1(_activePlayerId);
                GamePhase nextPhase = GetNextPhase(_currentPhase);
                bool autoChanged = false;
                string? autoChangeReason;

                if (ShouldAutoSkip(nextPhase, out autoChangeReason, isPlayer1)) {
                    autoChanged = true;
                    nextPhase = GetNextPhase(nextPhase);
                    if (_currentPhase == GamePhase.PLACEMENT) {
                        SwitchActivePlayer();
                    }
                }

                if (_currentPhase == GamePhase.ATTACK)
                {
                    SwitchActivePlayer();
                }

                
                _currentPhase = nextPhase;
               if (
                    _currentPhase == GamePhase.PLACEMENT &&        
                    _activePlayerId == _user_1
                ) {
                        _turnNumber += 1;
                }
                HandleChangePhaseEvent(isPlayer1);

                return new PhaseChangeResultDTO
                {
                    CurrentPhase = _currentPhase,
                    ActivePlayerId = _activePlayerId,
                    TurnNumber = _turnNumber,
                    AutoChanged = autoChanged,
                    AutoChangeReason = autoChangeReason,
                    CanAct = CanPlayerActInPhase()
                };
            }

       private void HandleChangePhaseEvent(bool isPlayer1)
        {
            switch(_currentPhase) {
                case GamePhase.PLACEMENT:
                    Champion activeChamp = getPlayerChamp(isPlayer1);
                    Board activeBoard = getPlayerBoard(isPlayer1);

                    DrawCards(_activePlayerId, 1);
                    _attackHandler.ResetAttackHandler();
                    if (activeChamp.GetBaseGold() < 10) {
                        activeChamp.SetBaseGold(activeChamp.GetBaseGold() + 1);
                    }
                    activeChamp.resetGold();
                    activeBoard.ResetBoardEngageState();
                    StartTimer(60);
                    break;
                case GamePhase.ATTACK:
                    break;
                case GamePhase.DEFENSE:
                    StopTimer();
                    StartTimer(60);
                    break;
                case GamePhase.END_TURN:
                    StopTimer();
                    ResolveBattle();
                    break;
            }
        }

        /// <summary>
        /// Force le changement de phase (utilisé par le timer d'1 minute).
        /// </summary>
        /// <returns>Résultat du changement de phase forcé</returns>
        public PhaseChangeResultDTO ForceChangePhase()
            {
                if (!_gameStarted)
                {
                    return new PhaseChangeResultDTO
                    {
                        CurrentPhase = _currentPhase,
                        ActivePlayerId = _activePlayerId,
                        TurnNumber = _turnNumber,
                        AutoChanged = true,
                        AutoChangeReason = "Timeout - 1 minute écoulée",
                        CanAct = false
                    };
                }

                PhaseChangeResultDTO result = AdvancePhase();
                result.AutoChanged = true;
                result.AutoChangeReason = "Timeout - 1 minute écoulée";
                return result;
            }

            /// <summary>
            /// Vérifie si un joueur peut agir dans une phase donnée.
            /// </summary>
            private bool CanPlayerActInPhase()
            {
                // En phase Defense, l'adversaire peut agir
                // Dans les autres phases, seul le joueur actif peut agir
                return _currentPhase != GamePhase.END_TURN;
            }

            /// <summary>
            /// Retourne la phase suivante dans le cycle.
            /// </summary>
            private static GamePhase GetNextPhase(GamePhase current)
            {
                switch(current) {
                    case GamePhase.PLACEMENT:
                        return GamePhase.ATTACK;
                    case GamePhase.ATTACK:
                        return GamePhase.DEFENSE;
                    case GamePhase.DEFENSE:
                        return GamePhase.END_TURN;
                    case GamePhase.END_TURN:
                        return GamePhase.PLACEMENT;
                    default:
                        return GamePhase.PLACEMENT;
                }
            }

            /// <summary>
            /// Détermine si une phase doit être automatiquement skippée.
            /// </summary>
            /// <param name="phase">Phase à vérifier</param>
            /// <param name="reason">Raison du skip si applicable</param>
            /// <returns>True si la phase doit être skippée</returns>
            private bool ShouldAutoSkip(GamePhase phase, out string? reason, bool isPlayer1)
            {
                reason = null;

                switch (phase)
                {

                    case GamePhase.PLACEMENT:
                        // Le joueur DOIT appeler ChangePhase manuellement
                        // Jamais auto-skip, même s'il n'a pas de cartes jouables
                        return false;

                    case GamePhase.ATTACK:
                        // Auto-skip si aucune carte sur le board ne peut attaquer
                        if (!CanActivePlayerAttack(isPlayer1))
                        {
                            reason = "Aucune carte ne peut attaquer";
                            return true;
                        }
                        return false;

                    case GamePhase.DEFENSE:
                        // Auto-skip si l'adversaire n'a pas de cartes pour défendre
                        // ou si le joueur actif n'a pas attaqué
                        if (!CanOpponentDefend(isPlayer1))
                        {
                            reason = "Aucune carte ne peut défendre";
                            return true;
                        }
                        return false;

                    case GamePhase.END_TURN:
                        // EndTurn est toujours exécuté, jamais skippé
                        return false;

                    default:
                        return false;
                }
            }

            /// <summary>
            /// Vérifie si le joueur actif a des cartes pouvant attaquer.
            /// </summary>
            private bool CanActivePlayerAttack(bool isPlayer1)
            {
                Board activeBoard = getPlayerBoard(isPlayer1);
                return activeBoard.HasAttackableCards();
            }

            /// <summary>
            /// Vérifie si l'adversaire peut défendre.
            /// </summary>
            private bool CanOpponentDefend(bool isPlayer1)
            {
                Board opponentBoard = getPlayerBoard(isPlayer1);
                return opponentBoard.HasDefendableCards();
            }

            /// <summary>
            /// Change le joueur actif.
            /// </summary>
            private void SwitchActivePlayer()
            {
                _activePlayerId = _activePlayerId == _user_1 ? _user_2 : _user_1;
            }

            /// <summary>
            /// Retourne l'ID de l'adversaire d'un joueur.
            /// </summary>
            public Guid GetOpponentId(Guid playerId)
            {
                return playerId == _user_1 ? _user_2 : _user_1;
            }

            /// <summary>
            /// Vérifie si c'est le tour du joueur spécifié.
            /// </summary>
            public bool IsPlayerTurn(Guid playerId)
            {
                return _gameStarted && _activePlayerId == playerId;
            }

            /// <summary>
            /// Vérifie si un joueur peut agir dans la phase actuelle.
            /// </summary>
            public bool CanPlayerAct(Guid playerId)
            {
                if (!_gameStarted) return false;

                // En phase Defense, seul l'adversaire peut agir
                if (_currentPhase == GamePhase.DEFENSE)
                {
                    return playerId == GetOpponentId(_activePlayerId);
                }

                // Dans les autres phases, seul le joueur actif peut agir
                return playerId == _activePlayerId;
            }

        #endregion
    
    }
}