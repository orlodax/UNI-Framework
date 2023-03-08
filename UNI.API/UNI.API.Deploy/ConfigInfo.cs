namespace UNI.API.Deploy;

public class ConfigInfo
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public List<string>? Hosts { get; set; }
    public string? DestinationPath { get; set; }
    public string? Source { get; set; }
    public string? ServiceName { get; set; }
}
