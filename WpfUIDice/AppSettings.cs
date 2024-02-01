using System.Collections.Generic;

namespace WpfUIDice;

public class AppSettings
{
    public List<string> SavedDiceIds { get; set; } = [];
    public List<WebHookSettings> WebHoolUrls { get; set; } = [];
}

public class WebHookSettings
{
    public string Url { get; set; }
    public Dictionary<string, string> Headers { get; set; }
}