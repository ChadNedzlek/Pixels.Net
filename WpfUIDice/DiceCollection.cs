using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using VaettirNet.PixelsDice.Net;

namespace WpfUIDice;

public class DiceCollection : DependencyObject
{
    public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
        nameof(Dice),
        typeof(ObservableCollection<DieView>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<DieView>), OnDicePropertyChanged));

    public static readonly DependencyProperty LogProperty = DependencyProperty.Register(
        nameof(Log),
        typeof(ObservableCollection<string>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<string>)));

    public static readonly DependencyProperty ProxyUrlsProperty = DependencyProperty.Register(
        nameof(ProxyUrls),
        typeof(ObservableCollection<ProxyUrl>),
        typeof(DiceCollection),
        new PropertyMetadata(default(ObservableCollection<ProxyUrl>), OnProxyUrlsPropertyChanged));

    public DiceCollection()
    {
        Dice = [];
        ProxyUrls = [];
        Log = [];
        Sender.ResponseReceived += ProxyResponseReceived;
    }

    private void ProxyResponseReceived(string url, bool success, string error)
    {
        if (Dispatcher.CheckAccess())
        {
            UpdateStatus();
        }
        else
        {
            Dispatcher.Invoke(UpdateStatus);
        }

        void UpdateStatus()
        {
            var target = ProxyUrls.FirstOrDefault(u => u.Url == url);
            if (target == null)
                return;
            if (success)
            {
                target.IsError = false;
                target.LastErrorMessage = null;
            }
            else
            {
                target.IsError = true;
                target.LastErrorMessage = error;
            }
        }
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

    private static void OnDicePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var that = (DiceCollection)d;
        if (e.OldValue is ObservableCollection<DieView> oldValue)
            oldValue.CollectionChanged -= that.DiceCollectionChanged;
        if (e.NewValue is ObservableCollection<DieView> newValue)
            newValue.CollectionChanged += that.DiceCollectionChanged;
    }

    private void DiceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (DieView die in e.OldItems.EmptyCast<DieView>())
        {
            Sender.RemoveDie(die.Die);
        }
        
        foreach (DieView die in e.NewItems.EmptyCast<DieView>())
        {
            Sender.AddDie(die.Die);
        }
    }

    private static void OnProxyUrlsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var that = (DiceCollection)d;
        if (e.OldValue is ObservableCollection<ProxyUrl> oldValue)
            oldValue.CollectionChanged -= that.UrlCollectionChanged;
        if (e.NewValue is ObservableCollection<ProxyUrl> newValue)
            newValue.CollectionChanged += that.UrlCollectionChanged;
    }

    private readonly List<(ProxyUrl url, NotifyCollectionChangedEventHandler collectionChanged, EventHandler headerChanged)> _urlListeners = [];
    private void UrlCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ProxyUrl url in e.NewItems)
            {
                Sender.AddUrl(url.Url, url.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Name) && !string.IsNullOrWhiteSpace(h.Value)).ToImmutableDictionary(h => h.Name, h => h.Value));
                NotifyCollectionChangedEventHandler collectionChanged = (s, e2) => UrlHeaderCollectionChanged(url, s, e2);
                EventHandler headerChanged = (s, e2) => HeaderChanged(url, s, e2);
                _urlListeners.Add((url, collectionChanged, headerChanged));
                url.Headers.CollectionChanged += collectionChanged;
            }
        }
        
        if (e.OldItems != null)
        {
            foreach (ProxyUrl url in e.OldItems)
            {
                Sender.RemoveUrl(url.Url);
                var listener = _urlListeners.First(u => u.url == url);
                url.Headers.CollectionChanged -= listener.collectionChanged;
            }
        }
    }

    private static readonly DependencyPropertyDescriptor HeaderNameDescriptor = DependencyPropertyDescriptor.FromProperty(ProxyHeader.NameProperty, typeof(ProxyHeader));
    private static readonly DependencyPropertyDescriptor HeaderValueDescriptor = DependencyPropertyDescriptor.FromProperty(ProxyHeader.ValueProperty, typeof(ProxyHeader));
    private void UrlHeaderCollectionChanged(ProxyUrl url, object sender, NotifyCollectionChangedEventArgs e)
    {
        var listener = _urlListeners.First(u => u.url == url);
        if (e.NewItems != null)
        {
            foreach (ProxyHeader header in e.NewItems)
            {
                HeaderNameDescriptor.AddValueChanged(header, listener.headerChanged);
                HeaderValueDescriptor.AddValueChanged(header, listener.headerChanged);
            }
        }
        
        if (e.OldItems != null)
        {
            foreach (ProxyHeader header in e.OldItems)
            {
                HeaderNameDescriptor.RemoveValueChanged(header, listener.headerChanged);
                HeaderValueDescriptor.RemoveValueChanged(header, listener.headerChanged);
            }
        }
    }

    private void HeaderChanged(ProxyUrl url, object sender, EventArgs e)
    {
        Sender.UpdateUrl(url.Url, url.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Name) && !string.IsNullOrWhiteSpace(h.Value)).ToImmutableDictionary(h => h.Name, h => h.Value));
    }

    public readonly DieSender Sender = new();
}

public static class HelperExtensions
{
    public static IEnumerable<T> EmptyCast<T>(this IEnumerable list)
    {
        if (list == null)
            return Array.Empty<T>();

        return list.Cast<T>();
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
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
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
    
    public static readonly DependencyProperty IsErrorProperty = DependencyProperty.Register(
        nameof(IsError),
        typeof(bool),
        typeof(ProxyUrl),
        new PropertyMetadata(default(bool)));

    public bool IsError
    {
        get => (bool)GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    public static readonly DependencyProperty LastErrorMessageProperty = DependencyProperty.Register(
        nameof(LastErrorMessage),
        typeof(string),
        typeof(ProxyUrl),
        new PropertyMetadata(default(string)));

    public string LastErrorMessage
    {
        get => (string)GetValue(LastErrorMessageProperty);
        set => SetValue(LastErrorMessageProperty, value);
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