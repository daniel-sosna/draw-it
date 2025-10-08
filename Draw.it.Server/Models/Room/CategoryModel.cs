namespace Draw.it.Server.Models.Room;

public class CategoryModel
{
    public long Id { get; set; } 
    
    public string Name { get; set; } = string.Empty;
    
    public List<string> Words { get; set; } = new List<string>();
}