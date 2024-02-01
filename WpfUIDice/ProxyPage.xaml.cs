using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VaettirNet.PixelsDice.Net;
using DataGrid = Wpf.Ui.Controls.DataGrid;

namespace WpfUIDice;

public partial class ProxyPage : Page
{
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(DiceCollection),
        typeof(ProxyPage),
        new PropertyMetadata(default(DiceCollection)));

    public DiceCollection Dice
    {
        get => (DiceCollection)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }
    
    public ProxyPage()
    {
        Dice = (DiceCollection)Application.Current.FindResource("DiceCollection")!;
        InitializeComponent();
    }

    private void AddUrl(object sender, RoutedEventArgs e)
    {
        string url = NewUrl.Text;
        if (string.IsNullOrWhiteSpace(url))
            return;
        Dice.ProxyUrls.Add(new ProxyUrl{Url = NewUrl.Text});
        NewUrl.Clear();
    }

    private void SendTestMessage(object sender, RoutedEventArgs e)
    {
        Dice.Sender.SendTestRoll();
    }
}