using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VaettirNet.PixelsDice.Net;

namespace BasicDiceMonitor;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(ObservableCollection<DieView>),
        typeof(MainWindow),
        new PropertyMetadata(default(ObservableCollection<DieView>)));

    public static readonly RoutedCommand ConnectDieCommand = new();
    public static readonly RoutedCommand SaveDieCommand = new();
    public static readonly RoutedCommand ForgetDieCommand = new();

    private PixelsManager _manager;
    private Task _scanTask;
    private CancellationTokenSource _stopScan;

    public MainWindow()
    {
        Dice = [];
        InitializeComponent();
        Task.Run(async () => _manager = await PixelsManager.CreateAsync());
    }

    public ObservableCollection<DieView> Dice
    {
        get => (ObservableCollection<DieView>)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }

    private void StartScanning(object sender, RoutedEventArgs e)
    {
        if (_scanTask != null)
            return;

        _stopScan = new CancellationTokenSource();
        _scanTask = ScanForDice(_stopScan.Token);
    }

    private async Task ScanForDice(CancellationToken cancellationToken)
    {
        await foreach (PixelsDie die in _manager.ScanAsync(true, false, cancellationToken: cancellationToken))
        {
            die.RollStateChanged += DieRolled;
            Dice.Add(new DieView(die, die.IsConnected));
        }
    }

    private void DieRolled(PixelsDie die, RollState roll, int value, int face)
    {
        Dispatcher.Invoke(() =>
        {
            DieView view = Dice.FirstOrDefault(d => d.Die == die);
            if (view == null)
                return;

            if (roll == RollState.OnFace)
                view.FinishRolling(value);
            else
                view.StartRolling();
        });
    }

    private async void StopScanning(object sender, RoutedEventArgs e)
    {
        if (_scanTask == null)
            return;

        await _stopScan.CancelAsync();
        try
        {
            await _scanTask;
        }
        catch (OperationCanceledException)
        {
        }

        _scanTask = null;
    }

    private async void ConnectDie(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not DieView die)
            return;

        await die.Die.ConnectAsync();
        die.Connect();
    }

    private void SaveDie(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not DieView die)
            return;
        die.Save();
    }

    private void ForgetDie(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not DieView die)
            return;
        Dice.Remove(die);
        die.Die.Dispose();
    }
}

public class DieView : DependencyObject
{
    private static readonly DependencyPropertyKey FaceValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(FaceValue),
        typeof(int),
        typeof(DieView),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty FaceValueProperty = FaceValuePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey LedCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(LedCount),
        typeof(int),
        typeof(DieView),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty LedCountProperty = LedCountPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ConnectVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ConnectVisibility),
        typeof(Visibility),
        typeof(DieView),
        new PropertyMetadata(default(Visibility)));

    public static readonly DependencyProperty ConnectVisibilityProperty =
        ConnectVisibilityPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey SaveVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SaveVisibility),
        typeof(Visibility),
        typeof(DieView),
        new PropertyMetadata(default(Visibility)));

    public static readonly DependencyProperty SaveVisibilityProperty = SaveVisibilityPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ValueVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ValueVisibility),
        typeof(Visibility),
        typeof(DieView),
        new PropertyMetadata(default(Visibility)));

    public static readonly DependencyProperty ValueVisibilityProperty = ValueVisibilityPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey RollingVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(RollingVisibility),
        typeof(Visibility),
        typeof(DieView),
        new PropertyMetadata(default(Visibility)));

    public static readonly DependencyProperty RollingVisibilityProperty =
        RollingVisibilityPropertyKey.DependencyProperty;

    private bool _isFromSave;

    public DieView(PixelsDie die, bool fromSave)
    {
        Die = die;
        LedCount = die.LedCount;
        _isFromSave = fromSave;
        RollingVisibility = Visibility.Collapsed;
        ValueVisibility = Visibility.Collapsed;
        SaveVisibility = fromSave || !die.IsConnected ? Visibility.Collapsed : Visibility.Visible;
        ConnectVisibility = die.IsConnected ? Visibility.Collapsed : Visibility.Visible;
    }

    public PixelsDie Die { get; }

    public int FaceValue
    {
        get => (int)GetValue(FaceValueProperty);
        private set => SetValue(FaceValuePropertyKey, value);
    }

    public int LedCount
    {
        get => (int)GetValue(LedCountProperty);
        private set => SetValue(LedCountPropertyKey, value);
    }

    public Visibility ConnectVisibility
    {
        get => (Visibility)GetValue(ConnectVisibilityProperty);
        private set => SetValue(ConnectVisibilityPropertyKey, value);
    }

    public Visibility SaveVisibility
    {
        get => (Visibility)GetValue(SaveVisibilityProperty);
        private set => SetValue(SaveVisibilityPropertyKey, value);
    }

    public Visibility ValueVisibility
    {
        get => (Visibility)GetValue(ValueVisibilityProperty);
        private set => SetValue(ValueVisibilityPropertyKey, value);
    }

    public Visibility RollingVisibility
    {
        get => (Visibility)GetValue(RollingVisibilityProperty);
        private set => SetValue(RollingVisibilityPropertyKey, value);
    }

    public void Connect()
    {
        ConnectVisibility = Visibility.Collapsed;
        SaveVisibility = _isFromSave ? Visibility.Collapsed : Visibility.Visible;
    }

    public void Save()
    {
        _isFromSave = true;
        SaveVisibility = Visibility.Collapsed;
    }

    public void StartRolling()
    {
        RollingVisibility = Visibility.Visible;
        ValueVisibility = Visibility.Collapsed;
    }

    public void FinishRolling(int face)
    {
        RollingVisibility = Visibility.Collapsed;
        ValueVisibility = Visibility.Visible;
        FaceValue = face + 1;
    }
}