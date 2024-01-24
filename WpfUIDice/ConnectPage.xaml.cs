using System.Windows;
using System.Windows.Controls;

namespace WpfUIDice;

public partial class ConnectPage : Page
{
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(DiceCollection),
        typeof(ConnectPage),
        new PropertyMetadata(default(DiceCollection)));

    public DiceCollection Dice
    {
        get => (DiceCollection)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }
    
    public ConnectPage()
    {
        Dice = (DiceCollection)Application.Current.FindResource("DiceCollection")!;
        InitializeComponent();
    }
}