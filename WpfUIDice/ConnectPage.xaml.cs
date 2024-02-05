using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VaettirNet.PixelsDice.Net;

namespace WpfUIDice;

public partial class ConnectPage : Page
{
    public static readonly RoutedCommand ConnectCommand = new();
    public static readonly RoutedCommand SaveCommand = new();
    public static readonly RoutedCommand DeleteCommand = new();

    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(DiceCollection),
        typeof(ConnectPage),
        new PropertyMetadata(default(DiceCollection)));

    public ConnectPage()
    {
        Dice = (DiceCollection)Application.Current.FindResource("DiceCollection")!;
        InitializeComponent();
        Dice.DieRollStateChanged += DieStateChanged;
        Unloaded += (_, _) => Dice.DieRollStateChanged -= DieStateChanged;
    }

    public DiceCollection Dice
    {
        get => (DiceCollection)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }

    private async void SaveDie(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not DieView die) return;

        Dice.Log.Add($"Saved die {die.Die.PixelId} as {die.Die.GetPersistentIdentifier()}");

        App.Current.AppSettings.SavedDiceIds.Add(die.Die.GetPersistentIdentifier());
        await App.Current.SaveSettingsAsync();
        die.Save();
    }

    private async void ConnectDie(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not DieView die) return;
        
        await die.Die.ConnectAsync();

        Dice.Log.Add($"Connected die {die.Die.PixelId} s{die.Die.LedCount} (Firmware: {die.Die.BuildTimestamp})");

        die.Connect();
    }

    private void DieStateChanged(DieView die, RollState state, int value, int index)
    {
        Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case RollState.OnFace:
                        Dice.Log.Add($"Die roll: {value} (die: {die.Die.PixelId})");
                        die.FinishRolling(value);
                        return;
                    default:
                        die.StartRolling();
                        return;
                }
            }
        );
    }

    private async void DisconnectDie(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not DieView die) return;

        await die.Die.DisposeAsync();

        Dice.Dice.Remove(die);
        App.Current.AppSettings.SavedDiceIds.Remove(die.Die.GetPersistentIdentifier());
    }
}