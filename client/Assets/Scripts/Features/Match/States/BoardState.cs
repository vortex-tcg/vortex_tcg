using System;
using System.Collections.Generic;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene.States
{
    /// <summary>
    /// Représente l'état complet du plateau de jeu pour un match
    /// </summary>
    [Serializable]
    public class BoardState
    {
        /// <summary>
        /// Cartes du joueur local sur le plateau (positions 0-4)
        /// </summary>
        public Dictionary<int, GameCardDto> PlayerBoardCards { get; set; } = new Dictionary<int, GameCardDto>();

        /// <summary>
        /// Cartes de l'adversaire sur le plateau (positions 0-4)
        /// </summary>
        public Dictionary<int, GameCardDto> OpponentBoardCards { get; set; } = new Dictionary<int, GameCardDto>();

        /// <summary>
        /// IDs des cartes actuellement en attaque
        /// </summary>
        public List<int> AttackingCardIds { get; set; } = new List<int>();

        /// <summary>
        /// Affectations de défense: clé = ID attaquant, valeur = ID défenseur
        /// </summary>
        public Dictionary<int, int> DefenseAssignments { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Nombre total de slots disponibles sur le plateau (généralement 5)
        /// </summary>
        public int MaxBoardSlots { get; set; } = 5;

        /// <summary>
        /// Vérifie si le plateau du joueur est plein
        /// </summary>
        public bool IsPlayerBoardFull => PlayerBoardCards.Count >= MaxBoardSlots;

        /// <summary>
        /// Vérifie si le plateau de l'adversaire est plein
        /// </summary>
        public bool IsOpponentBoardFull => OpponentBoardCards.Count >= MaxBoardSlots;

        /// <summary>
        /// Obtient une carte du joueur par sa position sur le plateau
        /// </summary>
        public GameCardDto GetPlayerCardAtPosition(int position)
        {
            return PlayerBoardCards.TryGetValue(position, out GameCardDto card) ? card : null;
        }

        /// <summary>
        /// Obtient une carte de l'adversaire par sa position sur le plateau
        /// </summary>
        public GameCardDto GetOpponentCardAtPosition(int position)
        {
            return OpponentBoardCards.TryGetValue(position, out GameCardDto card) ? card : null;
        }

        /// <summary>
        /// Ajoute une carte du joueur à une position spécifique
        /// </summary>
        public bool AddPlayerCard(int position, GameCardDto card)
        {
            if (position < 0 || position >= MaxBoardSlots) return false;
            if (PlayerBoardCards.ContainsKey(position)) return false;

            PlayerBoardCards[position] = card;
            return true;
        }

        /// <summary>
        /// Ajoute une carte de l'adversaire à une position spécifique
        /// </summary>
        public bool AddOpponentCard(int position, GameCardDto card)
        {
            if (position < 0 || position >= MaxBoardSlots) return false;
            if (OpponentBoardCards.ContainsKey(position)) return false;

            OpponentBoardCards[position] = card;
            return true;
        }

        /// <summary>
        /// Retire une carte du joueur par son ID
        /// </summary>
        public bool RemovePlayerCard(int gameCardId)
        {
            foreach (KeyValuePair<int, GameCardDto> kvp in PlayerBoardCards)
            {
                if (kvp.Value.GameCardId == gameCardId)
                {
                    PlayerBoardCards.Remove(kvp.Key);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retire une carte de l'adversaire par son ID
        /// </summary>
        public bool RemoveOpponentCard(int gameCardId)
        {
            foreach (KeyValuePair<int, GameCardDto> kvp in OpponentBoardCards)
            {
                if (kvp.Value.GameCardId == gameCardId)
                {
                    OpponentBoardCards.Remove(kvp.Key);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Réinitialise complètement l'état du plateau
        /// </summary>
        public void Reset()
        {
            PlayerBoardCards.Clear();
            OpponentBoardCards.Clear();
            AttackingCardIds.Clear();
            DefenseAssignments.Clear();
        }

        /// <summary>
        /// Clone l'état actuel du plateau
        /// </summary>
        public BoardState Clone()
        {
            return new BoardState
            {
                PlayerBoardCards = new Dictionary<int, GameCardDto>(PlayerBoardCards),
                OpponentBoardCards = new Dictionary<int, GameCardDto>(OpponentBoardCards),
                AttackingCardIds = new List<int>(AttackingCardIds),
                DefenseAssignments = new Dictionary<int, int>(DefenseAssignments),
                MaxBoardSlots = MaxBoardSlots
            };
        }
    }
}
