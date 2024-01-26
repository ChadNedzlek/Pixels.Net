using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfUIDice;

public partial class LogPage : Page
{
    public static readonly RoutedCommand ConnectCommand = new();
    public static readonly RoutedCommand SaveCommand = new();
    public static readonly RoutedCommand DeleteCommand = new();
    
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(DiceCollection),
        typeof(LogPage),
        new PropertyMetadata(default(DiceCollection)));

    public DiceCollection Dice
    {
        get => (DiceCollection)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }
    
    public LogPage()
    {
        Dice = (DiceCollection)Application.Current.FindResource("DiceCollection")!;
        InitializeComponent();
    }
}