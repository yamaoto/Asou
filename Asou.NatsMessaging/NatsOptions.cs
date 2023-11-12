namespace Asou.NatsMessaging;

public class NatsOptions
{
    public string[] Servers { get; set; } = Array.Empty<string>();
    public int MaxReconnect { get; set; } = 2;
    public int ReconnectWait { get; set; } = 1000;
}
