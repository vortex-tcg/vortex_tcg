using UnityEngine;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Utility responsible for toggling "sleepy" state on all cards during turn one.
    /// Designed to be driven from PhaseService (or other game flow logic).
    /// </summary>
    public static class SleepManager
    {
        /// <summary>
        /// Whether the game is currently in the sleepy first-turn period.
        /// </summary>
        public static bool IsSleeping { get; private set; }

        public static void SleepAll()
        {
            if (IsSleeping) return;
            IsSleeping = true;
            Debug.Log("[SleepManager] Activating sleepy state for all cards");

            CardUI[] cards = Object.FindObjectsOfType<CardUI>(true);
            foreach (CardUI card in cards)
            {
                if (card != null)
                    card.SetSleepy(true);
            }
        }

        public static void WakeAll()
        {
            if (!IsSleeping) return;
            IsSleeping = false;
            Debug.Log("[SleepManager] Clearing sleepy state for all cards");

            CardUI[] cards = Object.FindObjectsOfType<CardUI>(true);
            foreach (CardUI card in cards)
            {
                if (card != null)
                    card.SetSleepy(false);
            }
        }
    }
}