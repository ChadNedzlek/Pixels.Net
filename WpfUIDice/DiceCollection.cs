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

    public static readonly DependencyProperty LogProperty = DependencyProperty.Register(
        nameof(Log),
        typeof(ObservableCollection<string>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<string>)));

    public static readonly DependencyProperty ProxyUrlsProperty = DependencyProperty.Register(
        nameof(ProxyUrls),
        typeof(ObservableCollection<ProxyUrl>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<ProxyUrl>)));

    public DiceCollection()
    {
        Dice = [];
        ProxyUrls = [];
        Log = [];
    }

    public ObservableCollection<DieView> Dice
    {
        get => (ObservableCollection<DieView>)GetValue(DiceProperty);
        set => SetValue(DiceProperty, value);
    }

    public ObservableCollection<string> Log
    {
        get => (ObservableCollection<string>)GetValue(LogProperty);
        set => SetValue(LogProperty, value);
    }

    public ObservableCollection<ProxyUrl> ProxyUrls
    {
        get => (ObservableCollection<ProxyUrl>)GetValue(ProxyUrlsProperty);
        set => SetValue(ProxyUrlsProperty, value);
    }
}

public class ProxyUrl : DependencyObject
{
    public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
        nameof(Url),
        typeof(string),
        typeof(ProxyUrl),
        new PropertyMetadata(default(string)));

    public string Url
    {
        get { return (string)GetValue(UrlProperty); }
        set { SetValue(UrlProperty, value); }
    }
    
    public static readonly DependencyProperty HeadersProperty = DependencyProperty.Register(
        nameof(Headers),
        typeof(ObservableCollection<ProxyHeader>),
        typeof(ProxyUrl),
        new PropertyMetadata(default(ObservableCollection<ProxyHeader>)));

    public ProxyUrl()
    {
        Headers = [];
    }

    public ObservableCollection<ProxyHeader> Headers
    {
        get => (ObservableCollection<ProxyHeader>)GetValue(HeadersProperty);
        set => SetValue(HeadersProperty, value);
    }
}

public class ProxyHeader : DependencyObject
{
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
        nameof(Name),
        typeof(string),
        typeof(ProxyHeader),
        new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof(ProxyHeader),
        new PropertyMetadata(default(string)));

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}