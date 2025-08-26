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
                    Debug.LogError("JwtStore.asset introuvable.");
                }
                else
                {
                    _inst.LoadFromPrefs();
                }
            }
            return _inst;
        }
    }
}
