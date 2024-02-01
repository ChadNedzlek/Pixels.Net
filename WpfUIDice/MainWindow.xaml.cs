using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VaettirNet.PixelsDice.Net;
using Wpf.Ui;
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

    private CancellationTokenSource _scanStop;
    private IContentDialogService _dialogs = new ContentDialogService();

    public MainWindow()
    {
        Dice = (DiceCollection)Application.Current.FindResource("DiceCollection")!;
        InitializeComponent();
        _dialogs.SetContentPresenter(RootContentDialogPresenter);
        NavigationView.Loaded += (_, _) =>
            NavigationView.Navigate(((NavigationViewItem)NavigationView.MenuItems[0]).TargetPageType);

        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        List<string> savedDice = App.Current.AppSettings.SavedDiceIds;
        if (savedDice?.Count > 0)
        {
            var dialogClosed = new CancellationTokenSource();
            var scanCompleted = new CancellationTokenSource();
            
            async Task WaitDialog()
            {
                await Task.Delay(TimeSpan.FromSeconds(10), scanCompleted.Token);
                if (scanCompleted.IsCancellationRequested)
                    return;
                await _dialogs.ShowSimpleDialogAsync(new()
                    {
                        Title = "Reconnecting",
                        CloseButtonText = "Stop",
                        Content = "Reconnecting saved dice..."
                    },
                    scanCompleted.Token
                );
                dialogClosed.Cancel();
            }

            async Task ConnectDice()
            {
                await foreach (var die in App.Current.PixelsManager.ReattachAsync(savedDice, TimeSpan.FromMinutes(1), cancellationToken: dialogClosed.Token))
                {
                    await die.ConnectAsync();
                    Dice.Log.Add($"Found die {die.LedCount}");
                    var dieView = new DieView(die, true);
                    Dice.Dice.Add(dieView);
                }
                scanCompleted.Cancel();
            }

            try
            {
                await Task.WhenAll(WaitDialog(), ConnectDice());
            }
            catch (OperationCanceledException) when (dialogClosed.IsCancellationRequested || scanCompleted.IsCancellationRequested)
            {
                // This is good
            }
        }
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
        _scanStop = new CancellationTokenSource();
        Dice.Log.Add("Scan started.");
        IsScanning = true;
        CancellationToken stopToken = _scanStop.Token;
        try
        {
            await foreach (PixelsDie die in App.Current.PixelsManager.ScanAsync(true, false, cancellationToken: stopToken))
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