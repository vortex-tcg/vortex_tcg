public class Role
{
    [Key]
    public int Id { get; set; }

    public string Label { get; set; }

    public ICollection<User> Users { get; set; }
}
