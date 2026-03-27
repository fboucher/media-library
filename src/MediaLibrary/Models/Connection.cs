namespace MediaLibrary.Models;

public class Connection
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
}
