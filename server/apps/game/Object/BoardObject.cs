// =============================================
// FICHIER: Object/BoardObject.cs
// =============================================
// RÔLE PRINCIPAL:
// Représente le plateau de jeu d'un joueur avec 5 emplacements pour les cartes.
//
// RESPONSABILITÉS:
// 1. Gérer les 5 emplacements de cartes du plateau
// 2. Vérifier si des cartes peuvent attaquer ou défendre
// =============================================
namespace VortexTCG.Game.Object
{

    public enum CardSlotState {
        ENGAGE = 0,
        ATTACK_ENGAGE = 1,
        CAN_ATTACK = 2,
        DEFENSE_ENGAGE = 3,
        CAN_DEFEND = 4,
    };

    /// <summary>
    /// Plateau de jeu d'un joueur avec 5 emplacements pour les cartes.
    /// </summary>
    public class Board
    {
        private Card? _location_1 = null;
        private Card? _location_2 = null;
        private Card? _location_3 = null;
        private Card? _location_4 = null;
        private Card? _location_5 = null;
        private Card? _location_6 = null;

        private List<Card> GetListBoardCards()
        => new List<Card>([_location_1, _location_2, _location_3, _location_4, _location_5, _location_6]);

        /// <summary>
        /// Vérifie si au moins une carte sur le plateau peut attaquer.
        /// </summary>
        /// <returns>True si au moins une carte peut attaquer</returns>
        public bool HasAttackableCards()
        {
            List<Card> cards = GetListBoardCards();
            int count = cards.Count(card => card != null && !card.HasState(CardState.ENGAGE));

            return count > 0;
        }

        /// <summary>
        /// Vérifie si au moins une carte sur le plateau peut défendre.
        /// </summary>
        /// <returns>True si au moins une carte peut défendre</returns>
        public bool HasDefendableCards()
        {
            List<Card> cards = GetListBoardCards();
            int count = cards.Count(card => card != null && !card.HasStates([CardState.ATTACK_ENGAGE, CardState.ENGAGE]));

            return count > 0;
        }

        /// <summary>
        /// Retourne le nombre de cartes sur le plateau.
        /// </summary>
        public int GetCardCount()
        {
            List<Card> cards = GetListBoardCards();
            int count = cards.Count(card => card != null);
            return count;
        }

        /// <summary>
        /// Vérifie si le plateau est vide.
        /// </summary>
        public bool IsEmpty()
        {
            return GetCardCount() == 0;
        }

        public bool IsAvailable(int location) {
            switch(location) {
                case 0:
                    return (_location_1 == null);
                case 1:
                    return (_location_2 == null);
                case 2:
                    return (_location_3 == null);
                case 3:
                    return (_location_4 == null);
                case 4:
                    return (_location_5 == null);
                case 5:
                    return (_location_6 == null);
                default:
                    return false;
            }
        }

        public void PosCard(Card card, int location) {
            switch(location) {
                case 0:
                    _location_1 = card;
                    break;
                case 1:
                    _location_2 = card;
                    break;
                case 2:
                    _location_3 = card;
                    break;
                case 3:
                    _location_4 = card;
                    break;
                case 4:
                    _location_5 = card;
                    break;
                case 5:
                    _location_6 = card;
                    break;
            }
        }

        public bool TryGetCardPos(int cardId, out int pos) {
            if (_location_1.GetGameCardId() == cardId) {
                pos = 0;
                return true;
            } else if (_location_2.GetGameCardId() == cardId) {
                pos = 1;
                return true;
            } else if (_location_3.GetGameCardId() == cardId) {
                pos = 2;
                return true;
            } else if (_location_4.GetGameCardId() == cardId) {
                pos = 3;
                return true;
            } else if (_location_5.GetGameCardId() == cardId) {
                pos = 4;
                return true;
            } else if (_location_6.GetGameCardId() == cardId) {
                pos = 5;
                return true;
            } else {
                pos = -1;
                return false;
            }
        }

        public Card GetCardFromSlot(int slotPos) {
            switch(slotPos) {
                case 0:
                    return _location_1;
                case 1:
                    return _location_2;
                case 2:
                    return _location_3;
                case 3:
                    return _location_4;
                case 4:
                    return _location_5;
                case 5:
                    return _location_6;
                default:
                    return null;
            }
        }

        public CardSlotState canAttackSpot(int spot) {
            switch(spot) {
                case 0:
                    return _location_1.checkCanAttack();
                case 1:
                    return _location_2.checkCanAttack();
                case 2:
                    return _location_3.checkCanAttack();
                case 3:
                    return _location_4.checkCanAttack();
                case 4:
                    return _location_5.checkCanAttack();
                case 5:
                    return _location_6.checkCanAttack();
                default:
                    return CardSlotState.CAN_DEFEND;
            }
        }

        public CardSlotState canDefendSpot(int spot) {
            switch(spot) {
                case 0:
                    return _location_1.checkCanDefend();
                case 1:
                    return _location_2.checkCanDefend();
                case 2:
                    return _location_3.checkCanDefend();
                case 3:
                    return _location_4.checkCanDefend();
                case 4:
                    return _location_5.checkCanDefend();
                case 5:
                    return _location_6.checkCanDefend();
                default:
                    return CardSlotState.CAN_ATTACK;
            }
        }

        public void ResetBoardEngageState() {
            List<Card> cards = GetListBoardCards();

            foreach (Card card in cards) {
                if (card != null){

                    card.RemoveStates([CardState.ENGAGE, CardState.ATTACK_ENGAGE, CardState.DEFENSE_ENGAGE]);
                }
            }
        }

        #region Engage Unengage

            public void EngageAttackCard(int pos) {
                Card card = GetCardFromSlot(pos);

                card.AddState(CardState.ATTACK_ENGAGE);
            }

            public void EngageDefenseCard(int pos) {
                Card card = GetCardFromSlot(pos);

                card.AddState(CardState.DEFENSE_ENGAGE);
            }

            public void UnEngageAttackCard(int pos) {
                Card card = GetCardFromSlot(pos);

                card.RemoveState(CardState.ATTACK_ENGAGE);
            }

            public void UnEngageDefenseCard(int pos) {  
                Card card = GetCardFromSlot(pos);

                card.RemoveState(CardState.DEFENSE_ENGAGE);
            }

        #endregion
    }
}