using System.Collections.ObjectModel;
using System.Windows;

namespace WpfUIDice;

public class DiceCollection : DependencyObject
{
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(ObservableCollection<DieView>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<DieView>)));

    public ObservableCollection<DieView> Dice
    {
        get => (ObservableCollection<DieView>)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }

    public static readonly DependencyProperty LogProperty = DependencyProperty.Register(
        nameof(Log),
        typeof(ObservableCollection<string>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<string>)));

    public ObservableCollection<string> Log
    {
        get { return (ObservableCollection<string>)GetValue(LogProperty); }
        set { SetValue(LogProperty, value); }
    }
}