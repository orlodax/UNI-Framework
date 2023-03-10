namespace UNI.API.Client;

/// <summary>
/// Represents the appsettings section "UNIClientConfiguration" for apps using the UNIClient
/// </summary>
public class UNIClientConfiguration
{
    public string[] ServerUrls { get; set; } = Array.Empty<string>();
    public string ApiVersion { get; set; } = "v2";
    public bool IsTokenAutoRefreshable { get; set; } = false;
}
