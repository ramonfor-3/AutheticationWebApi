namespace AuthenticationWebApi.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Location> Locations { get; set; }
}