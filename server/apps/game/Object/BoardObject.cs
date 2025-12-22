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

        /// <summary>
        /// Vérifie si au moins une carte sur le plateau peut attaquer.
        /// </summary>
        /// <returns>True si au moins une carte peut attaquer</returns>
        public bool HasAttackableCards()
        {
            // Pour l'instant, une carte peut attaquer si elle existe sur le plateau
            return (_location_1 != null && !_location_1.HasState(CardState.ENGAGE)) ||
                   (_location_2 != null && !_location_2.HasState(CardState.ENGAGE)) ||
                   (_location_3 != null && !_location_3.HasState(CardState.ENGAGE)) ||
                   (_location_4 != null && !_location_4.HasState(CardState.ENGAGE)) ||
                   (_location_5 != null && !_location_5.HasState(CardState.ENGAGE));
        }

        /// <summary>
        /// Vérifie si au moins une carte sur le plateau peut défendre.
        /// </summary>
        /// <returns>True si au moins une carte peut défendre</returns>
        public bool HasDefendableCards()
        {
            // Pour l'instant, une carte peut défendre si elle existe sur le plateau
            return (_location_1 != null && !_location_1.HasState(CardState.ATTACK_ENGAGE)) ||
                   (_location_2 != null && !_location_2.HasState(CardState.ATTACK_ENGAGE)) ||
                   (_location_3 != null && !_location_3.HasState(CardState.ATTACK_ENGAGE)) ||
                   (_location_4 != null && !_location_4.HasState(CardState.ATTACK_ENGAGE)) ||
                   (_location_5 != null && !_location_5.HasState(CardState.ATTACK_ENGAGE));
        }

        /// <summary>
        /// Retourne le nombre de cartes sur le plateau.
        /// </summary>
        public int GetCardCount()
        {
            int count = 0;
            if (_location_1 != null) count++;
            if (_location_2 != null) count++;
            if (_location_3 != null) count++;
            if (_location_4 != null) count++;
            if (_location_5 != null) count++;
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
                    return (_location_1 == null) ? true : false;
                case 1:
                    return (_location_2 == null) ? true : false;
                case 2:
                    return (_location_3 == null) ? true : false;
                case 3:
                    return (_location_4 == null) ? true : false;
                case 4:
                    return (_location_5 == null) ? true : false;
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
            }
        }

        private void ResetCardEngageState(Card card) {
            card.RemoveState(CardState.ENGAGE);
            card.RemoveState(CardState.ATTACK_ENGAGE);
            card.RemoveState(CardState.DEFENSE_ENGAGE);
        }

        public void ResetBoardEngageState() {
            if (_location_1 != null) {
                ResetCardEngageState(_location_1);
            }
            if (_location_2 != null) {
                ResetCardEngageState(_location_2);
            }
            if (_location_3 != null) {
                ResetCardEngageState(_location_3);
            }
            if (_location_4 != null) {
                ResetCardEngageState(_location_4);
            }
            if (_location_5 != null) {
                ResetCardEngageState(_location_5);
            }
        }
    }
}