using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VaettirNet.PixelsDice.Net;

namespace WpfUIDice;

public class DieView : DependencyObject, INotifyPropertyChanged
{
    public PixelsDie Die { get; }
    private bool _isFromSave;

    public DieView(PixelsDie die, bool fromSave)
    {
        Die = die;
        LedCount = die.LedCount;
        _isFromSave = fromSave;
        IsRolling = false;
        IsSaved = fromSave;
        IsConnected = die.IsConnected;
    }

    public void Connect()
    {
        IsConnected = true;
        IsSaved = _isFromSave;
        OnPropertyChanged(nameof(Die));
    }

    public void Save()
    {
        _isFromSave = true;
        IsSaved = true;
    }

    public void StartRolling()
    {
        IsRolling = true;
    }

    public void FinishRolling(int face)
    {
        IsRolling = false;
        FaceValue = face + 1;
    }

    private static readonly DependencyPropertyKey FaceValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(FaceValue),
        typeof(int),
        typeof(DieView),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty FaceValueProperty = FaceValuePropertyKey.DependencyProperty;

    public int FaceValue
    {
        get => (int)GetValue(FaceValueProperty);
        private set => SetValue(FaceValuePropertyKey, value);
    }
    
    private static readonly DependencyPropertyKey LedCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(LedCount),
        typeof(int),
        typeof(DieView),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty LedCountProperty = LedCountPropertyKey.DependencyProperty;

    public int LedCount
    {
        get => (int)GetValue(LedCountProperty);
        private set => SetValue(LedCountPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsConnectedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsConnected),
        typeof(bool),
        typeof(DieView),
        new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsConnectedProperty = IsConnectedPropertyKey.DependencyProperty;

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        private set => SetValue(IsConnectedPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsRollingPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsRolling),
        typeof(bool),
        typeof(DieView),
        new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsRollingProperty = IsRollingPropertyKey.DependencyProperty;

    public bool IsRolling
    {
        get => (bool)GetValue(IsRollingProperty);
        private set => SetValue(IsRollingPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsSavedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsSaved),
        typeof(bool),
        typeof(DieView),
        new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsSavedProperty = IsSavedPropertyKey.DependencyProperty;

    public bool IsSaved
    {
        get => (bool)GetValue(IsSavedProperty);
        private set => SetValue(IsSavedPropertyKey, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}