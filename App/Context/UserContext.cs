namespace App.Context;

/// <summary>
/// Singleton Object to store the current user's username and authentication Token for API requests
/// </summary>
public class UserContext
{
    protected static UserContext _instance = null;

    public string UserName { get; set; }
    public string Token { get; set; }

    private UserContext()
    {
    }

    public static UserContext GetInstance()
    {
        if (_instance == null)
        {
            _instance = new UserContext();
        }
        
        return _instance;
    }

    public bool IsLoggedIn()
    {
        return !String.IsNullOrEmpty(Token);
    } 
}