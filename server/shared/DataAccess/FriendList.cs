public class FriendsList
{
    [Key]
    public int Id { get; set; }

    public string Label { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}
