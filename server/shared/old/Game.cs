public class Game
{
    [Key]
    public int Id { get; set; }

    public string Status { get; set; }

    public int TurnNumber { get; set; }

    public ICollection<User> Players { get; set; }

    public int? CurrentPlayerId { get; set; }
    public User CurrentPlayer { get; set; }
}
