// =============================================
// FICHIER: Services/PhaseTimerService.cs
// =============================================
// RÔLE PRINCIPAL:
// Gère les timers de phase pour chaque salon de jeu.
// Force le changement de phase après 1 minute d'inactivité.
//
// RESPONSABILITÉS:
// 1. Démarrer un timer de 60 secondes à chaque changement de phase
// 2. Annuler le timer précédent lors d'un nouveau changement
// 3. Forcer le changement de phase via SignalR quand le timer expire
//
// ARCHITECTURE:
// - Utilise ConcurrentDictionary pour les timers par salon
// - Utilise IHubContext pour communiquer avec les clients SignalR
// - Service Singleton pour persister les timers entre les requêtes
// =============================================

using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using game.Hubs;
using VortexTCG.Game.DTO;

namespace game.Services;

/// <summary>
/// Service gérant les timers de phase pour forcer le changement après 1 minute.
/// </summary>
public class PhaseTimerService
{
    /// <summary>Durée du timer en millisecondes (60 secondes)</summary>
    private const int PHASE_TIMEOUT_MS = 60_000;

    /// <summary>Context SignalR pour envoyer des messages aux clients</summary>
    private readonly IHubContext<GameHub> _hubContext;

    /// <summary>Service de gestion des salons</summary>
    private readonly RoomService _roomService;

    /// <summary>Timers actifs par code de salon</summary>
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _timers = new();

    public PhaseTimerService(IHubContext<GameHub> hubContext, RoomService roomService)
    {
        _hubContext = hubContext;
        _roomService = roomService;
    }

    /// <summary>
    /// Démarre ou redémarre le timer pour un salon.
    /// Appelé à chaque changement de phase.
    /// </summary>
    /// <param name="roomCode">Code du salon</param>
    public void StartOrResetTimer(string roomCode)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        // Annuler le timer précédent s'il existe
        StopTimer(roomCode);

        // Créer un nouveau CancellationTokenSource
        CancellationTokenSource cts = new CancellationTokenSource();
        _timers[roomCode] = cts;

        // Démarrer le timer en arrière-plan
        _ = RunTimerAsync(roomCode, cts.Token);
    }

    /// <summary>
    /// Arrête le timer pour un salon.
    /// Appelé quand la partie se termine ou le salon est fermé.
    /// </summary>
    /// <param name="roomCode">Code du salon</param>
    public void StopTimer(string roomCode)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        if (_timers.TryRemove(roomCode, out CancellationTokenSource? cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    /// <summary>
    /// Exécute le timer et force le changement de phase à expiration.
    /// </summary>
    private async Task RunTimerAsync(string roomCode, CancellationToken cancellationToken)
    {
        try
        {
            // Attendre 60 secondes
            await Task.Delay(PHASE_TIMEOUT_MS, cancellationToken);

            // Si on arrive ici, le timer a expiré (pas été annulé)
            await ForcePhaseChangeAsync(roomCode);
        }
        catch (TaskCanceledException)
        {
            // Le timer a été annulé, c'est normal (le joueur a changé de phase manuellement)
        }
        catch (ObjectDisposedException)
        {
            // Le CancellationTokenSource a été disposé, c'est normal
        }
    }

    /// <summary>
    /// Force le changement de phase et notifie les clients.
    /// </summary>
    private async Task ForcePhaseChangeAsync(string roomCode)
    {
        // Forcer le changement de phase côté serveur
        PhaseChangeResultDTO? result = _roomService.ForceChangePhase(roomCode);

        if (result != null)
        {
            // Notifier tous les joueurs du salon
            await _hubContext.Clients.Group(roomCode).SendAsync("PhaseChanged", result);

            // Redémarrer le timer pour la nouvelle phase
            StartOrResetTimer(roomCode);
        }
    }

    /// <summary>
    /// Récupère le temps restant pour un salon (en secondes).
    /// Utile pour l'affichage côté client.
    /// </summary>
    /// <param name="roomCode">Code du salon</param>
    /// <returns>Temps restant en secondes ou null si pas de timer actif</returns>
    public int? GetRemainingTime(string roomCode)
    {
        return _timers.ContainsKey(roomCode.Trim().ToUpperInvariant()) ? 60 : null;
    }
}
