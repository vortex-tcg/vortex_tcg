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

    public static void SetToken(string token, bool persist = true)
    {
        if (I == null) return;
        I.SetToken(token, persist);
    }

    public static bool IsExpired(int leewaySeconds = 0)
    {
        return I != null && I.IsExpired(leewaySeconds);
    }
}
