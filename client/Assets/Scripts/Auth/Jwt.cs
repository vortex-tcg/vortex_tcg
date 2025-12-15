using UnityEngine;

public static class Jwt
{
    private static JwtStore _inst;

    public static JwtStore I
    {
        get
        {
            if (_inst == null)
            {
                _inst = Resources.Load<JwtStore>("JwtStore"); 
                if (_inst == null)
                {
                    Debug.LogError("JwtStore.asset introuvable dans Resources/. Crée l'asset via Create → Vortex → Auth → JWT Store et nomme-le 'JwtStore'.");
                }
                else
                {
                    _inst.LoadFromPrefs();
                }
            }
            return _inst;
        }
    }

    public static string GetToken() => I != null ? I.Token : null;

    public static string GetUsername() => I != null ? I.Username : null;
    public static string GetRole() => I != null ? I.Role : null;
    public static string GetUserId() => I != null ? I.UserId : null;

    public static void SetSession(string id, string username, string role, string token, bool persist = true)
    {
        if (I == null) return;
        I.SetSession(id, username, role, token, persist);
    }

    public static bool IsExpired(int leewaySeconds = 0)
    {
        return I != null && I.IsExpired(leewaySeconds);
    }
}
