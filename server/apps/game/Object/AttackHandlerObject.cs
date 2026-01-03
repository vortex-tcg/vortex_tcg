using VortexTCG.Game.Object;
using VortexTCG.Game.DTO;

namespace VortexTCG.Game.Object {

    public class DefenseCard {
        public Card card { get; set; }
        public int oppositeCardId { get; set; }
    }

    public class AttackHandler {

        #region Variables de class
        
            private List<Card> _attackCards = new List<Card>();
            private List<DefenseCard> _defenseCards = new List<DefenseCard>();

        #endregion

        #region resetAttackHandler

            public void ResetAttackHandler() {
                _attackCards = new List<Card>();
                _defenseCards = new List<DefenseCard>();
            }

        #endregion

        #region Add and remove Attackers

            public void RemoveAttack(Card card) {
                _attackCards.Remove(card);
            }

            public void AddAttack(Card card) {
                _attackCards.Add(card);
            }

        #endregion

        #region Add and remove defenser

            public void RemoveDefense(Card card) {
                DefenseCard cardToRemove = null;
                foreach(DefenseCard defender in _defenseCards) {
                    if (defender.card == card) {
                        cardToRemove = defender;
                        break;
                    }
                }
                _defenseCards.Remove(cardToRemove);
            }

            public void AddDefense(Card playedCard, Card opponentCard) {
                int oppositeId = opponentCard.GetGameCardId();
                _defenseCards.RemoveAll(defenseCard => defenseCard.oppositeCardId == oppositeId);
                _defenseCards.Add(new DefenseCard{
                    card = playedCard,
                    oppositeCardId = oppositeId
                });
            }

        #endregion

        #region getter

            public List<Card> GetAttacker() => _attackCards;

            public List<DefenseCard> GetDefender() => _defenseCards;

            public DefenseCard GetSpecificDefender(int opponentCardId) => _defenseCards.Single(defender => defender.oppositeCardId == opponentCardId);

        #endregion

        #region format data

            private List<int> FormatListAttackers() {
                List<int> attackersCardsId = new List<int>();

                foreach(Card card in _attackCards) {
                    attackersCardsId.Add(card.GetGameCardId());
                }
                return attackersCardsId;
            }

            private List<DefenseCardDataDto> FormatListDefender() {
                List<DefenseCardDataDto> defenderCards = new List<DefenseCardDataDto>();

                foreach(DefenseCard defenseCard in _defenseCards) {
                    defenderCards.Add(new DefenseCardDataDto{
                            cardId = defenseCard.card.GetGameCardId(),
                            opponentCardId = defenseCard.oppositeCardId
                        }
                    );
                }
                return defenderCards;
            }

            public AttackResponseDto FormatAttackResponseDto(Guid PlayerId, Guid OpponentId) {
                List<int> attackersCardsId = FormatListAttackers();
                return new AttackResponseDto {
                    AttackCardsId = attackersCardsId,
                    PlayerId = PlayerId,
                    OpponentId = OpponentId
                };
            }

            public DefenseResponseDto FormatDefenseResponseDto(Guid PlayerId, Guid OpponentId) {
                List<int> attackersCardsId = FormatListAttackers();
                List<DefenseCardDataDto> defenderCards = FormatListDefender();
                return new DefenseResponseDto {
                    PlayerId = PlayerId,
                    OpponentId = OpponentId,
                    data = new DefenseDataResponseDto {
                        AttackCardsId = attackersCardsId,
                        DefenseCards = defenderCards
                    }
                };
            }

        #endregion
    }

}