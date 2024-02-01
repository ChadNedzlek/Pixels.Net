using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using VaettirNet.PixelsDice.Net;

namespace WpfUIDice;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "vaettir.net",
            "gamewithpixels",
            "data.json");
    
    private async Task LoadExisting()
    {
        if (!File.Exists(AppDataPath))
            return;

        try
        {
            await using FileStream stream = File.OpenRead(AppDataPath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream);
            AppSettings = settings;

            DiceCollection coll = DiceCollection;
            foreach (var url in settings.WebHoolUrls)
            {
                ProxyUrl view = new ProxyUrl
                {
                    Url = url.Url,
                };
                foreach (var h in url.Headers)
                {
                    view.Headers.Add(new ProxyHeader { Name = h.Key, Value = h.Value });
                }

                coll.ProxyUrls.Add(view);

                coll.Sender.AddUrl(url.Url, url.Headers.ToImmutableDictionary());
            }
        }
        catch
        {
            // Don't care
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        PixelsManager = await PixelsManager.CreateAsync();
        await LoadExisting();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await SaveSettingsAsync();
        
        base.OnExit(e);
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AppDataPath));
            
            AppSettings settings = AppSettings;
            DiceCollection coll = DiceCollection;
            settings.WebHoolUrls.Clear();
            foreach (var url in coll.ProxyUrls)
            {
                settings.WebHoolUrls.Add(new WebHookSettings
                {
                    Url = url.Url,
                    Headers = url.Headers.ToDictionary(u => u.Name, u => u.Value),
                });
            }

            await using FileStream stream = File.Create(AppDataPath);
            await JsonSerializer.SerializeAsync(stream, settings);
        }
        catch
        {
            // Don't care
        }
    }

    public AppSettings AppSettings
    {
        get => (AppSettings)Resources["AppSettings"];
        set => Resources["AppSettings"] = value;
    }
    
    public DiceCollection DiceCollection
    {
        get => (DiceCollection)Resources["DiceCollection"];
        set => Resources["DiceCollection"] = value;
    }

    public new static App Current => (App)Application.Current;

    public PixelsManager PixelsManager;
    
}