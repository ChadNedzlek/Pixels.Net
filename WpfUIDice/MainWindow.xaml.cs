using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VaettirNet.PixelsDice.Net;
using Wpf.Ui.Controls;

namespace WpfUIDice;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(DiceCollection),
        typeof(MainWindow),
        new PropertyMetadata(default(DiceCollection)));

    public static readonly DependencyProperty IsScanningProperty = DependencyProperty.Register(
        nameof(IsScanning),
        typeof(bool),
        typeof(MainWindow),
        new PropertyMetadata(default(bool)));

    private PixelsManager _manager;

    private CancellationTokenSource _scanStop;

    public MainWindow()
    {
        Dice = (DiceCollection)Application.Current.FindResource("DiceCollection")!;
        Task.Run(async () => _manager = await PixelsManager.CreateAsync());
        InitializeComponent();
        NavigationView.Loaded += (_, _) =>
            NavigationView.Navigate(((NavigationViewItem)NavigationView.MenuItems[0]).TargetPageType);
    }

    public DiceCollection Dice
    {
        get => (DiceCollection)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }

    public bool IsScanning
    {
        get => (bool)GetValue(IsScanningProperty);
        set => SetValue(IsScanningProperty, value);
    }

    private void StopScan(object sender, RoutedEventArgs e)
    {
        IsScanning = false;
        _scanStop?.Cancel();
        _scanStop = null;
    }

    private async void StartScan(object sender, RoutedEventArgs e)
    {
        if (_manager == null)
            return;
        _scanStop = new CancellationTokenSource();
        Dice.Log.Add("Scan started.");
        IsScanning = true;
        CancellationToken stopToken = _scanStop.Token;
        try
        {
            await foreach (PixelsDie die in _manager.ScanAsync(true, false, cancellationToken: stopToken))
            {
                if (Dice.Dice.Any(d => d.Die.PixelId == die.PixelId))
                {
                    await die.DisposeAsync();
                    continue;
                }

                Dice.Log.Add($"Found die {die.LedCount}");
                Dice.Dice.Add(new DieView(die, false));
            }
        }
        catch (OperationCanceledException) when (stopToken.IsCancellationRequested)
        {
            Dice.Log.Add("Scan stopped.");
            // Ignore
        }
    }
}