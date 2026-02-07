using System;

[System.Serializable]
public class LoginData
{
    public string email;
    public string password;

    public LoginData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}

[System.Serializable]
public class LoginResponseWrapper 
{ 
    public LoginResponseData data; 
}

[System.Serializable]
public class LoginResponseData 
{ 
    public string token; 
}

[System.Serializable]
public class TokenRoot 
{ 
    public string token; 
}

[System.Serializable]
public class AccessTokenRoot 
{ 
    public string accessToken; 
}
