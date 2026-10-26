namespace Template.Tools.KeypadSimulator.Behaviors;

public static class PcKeyboard
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly DependencyProperty PressCommandProperty = DependencyProperty.RegisterAttached(
        "PressCommand",
        typeof(ICommand),
        typeof(PcKeyboard),
        new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetPressCommand(DependencyObject obj) => (ICommand?)obj.GetValue(PressCommandProperty);

    public static void SetPressCommand(DependencyObject obj, ICommand? value) => obj.SetValue(PressCommandProperty, value);

    public static readonly DependencyProperty ReleaseCommandProperty = DependencyProperty.RegisterAttached(
        "ReleaseCommand",
        typeof(ICommand),
        typeof(PcKeyboard),
        new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetReleaseCommand(DependencyObject obj) => (ICommand?)obj.GetValue(ReleaseCommandProperty);

    public static void SetReleaseCommand(DependencyObject obj, ICommand? value) => obj.SetValue(ReleaseCommandProperty, value);

    private static readonly DependencyProperty AttachedProperty = DependencyProperty.RegisterAttached(
        "Attached",
        typeof(bool),
        typeof(PcKeyboard),
        new PropertyMetadata(false));

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((d is not UIElement element) || (bool)element.GetValue(AttachedProperty))
        {
            return;
        }

        element.SetValue(AttachedProperty, true);
        element.PreviewKeyDown += OnPreviewKeyDown;
        element.PreviewKeyUp += OnPreviewKeyUp;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e) =>
        e.Handled = Execute(GetPressCommand((DependencyObject)sender), e);

    private static void OnPreviewKeyUp(object sender, KeyEventArgs e) =>
        e.Handled = Execute(GetReleaseCommand((DependencyObject)sender), e);

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static bool Execute(ICommand? command, KeyEventArgs e)
    {
        if ((command is null) || ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows)) != 0))
        {
            return false;
        }

        var input = Map(e.Key, e.IsRepeat);
        if ((input is null) || !command.CanExecute(input))
        {
            return false;
        }

        command.Execute(input);
        return true;
    }

    private static PcKeyInput? Map(Key key, bool repeat) => key switch
    {
        Key.D0 => new PcKeyInput(KeypadKeyId.Num0, false, repeat),
        Key.D1 => new PcKeyInput(KeypadKeyId.Num1, false, repeat),
        Key.D2 => new PcKeyInput(KeypadKeyId.Num2, false, repeat),
        Key.D3 => new PcKeyInput(KeypadKeyId.Num3, false, repeat),
        Key.D4 => new PcKeyInput(KeypadKeyId.Num4, false, repeat),
        Key.D5 => new PcKeyInput(KeypadKeyId.Num5, false, repeat),
        Key.D6 => new PcKeyInput(KeypadKeyId.Num6, false, repeat),
        Key.D7 => new PcKeyInput(KeypadKeyId.Num7, false, repeat),
        Key.D8 => new PcKeyInput(KeypadKeyId.Num8, false, repeat),
        Key.D9 => new PcKeyInput(KeypadKeyId.Num9, false, repeat),
        Key.NumPad0 => new PcKeyInput(KeypadKeyId.Num0, true, repeat),
        Key.NumPad1 => new PcKeyInput(KeypadKeyId.Num1, true, repeat),
        Key.NumPad2 => new PcKeyInput(KeypadKeyId.Num2, true, repeat),
        Key.NumPad3 => new PcKeyInput(KeypadKeyId.Num3, true, repeat),
        Key.NumPad4 => new PcKeyInput(KeypadKeyId.Num4, true, repeat),
        Key.NumPad5 => new PcKeyInput(KeypadKeyId.Num5, true, repeat),
        Key.NumPad6 => new PcKeyInput(KeypadKeyId.Num6, true, repeat),
        Key.NumPad7 => new PcKeyInput(KeypadKeyId.Num7, true, repeat),
        Key.NumPad8 => new PcKeyInput(KeypadKeyId.Num8, true, repeat),
        Key.NumPad9 => new PcKeyInput(KeypadKeyId.Num9, true, repeat),
        Key.OemMinus => new PcKeyInput(KeypadKeyId.Minus, false, repeat),
        Key.Subtract => new PcKeyInput(KeypadKeyId.Minus, true, repeat),
        Key.OemPeriod => new PcKeyInput(KeypadKeyId.Period, false, repeat),
        Key.Decimal => new PcKeyInput(KeypadKeyId.Period, true, repeat),
        Key.Up => new PcKeyInput(KeypadKeyId.Up, false, repeat),
        Key.Down => new PcKeyInput(KeypadKeyId.Down, false, repeat),
        Key.Left => new PcKeyInput(KeypadKeyId.Left, false, repeat),
        Key.Right => new PcKeyInput(KeypadKeyId.Right, false, repeat),
        Key.Enter => new PcKeyInput(KeypadKeyId.Enter, false, repeat),
        Key.Escape => new PcKeyInput(KeypadKeyId.Escape, false, repeat),
        Key.Back or Key.Delete => new PcKeyInput(KeypadKeyId.Del, false, repeat),
        Key.F1 => new PcKeyInput(KeypadKeyId.F1, false, repeat),
        Key.F2 => new PcKeyInput(KeypadKeyId.F2, false, repeat),
        Key.F3 => new PcKeyInput(KeypadKeyId.F3, false, repeat),
        Key.F4 => new PcKeyInput(KeypadKeyId.F4, false, repeat),
        Key.Space => new PcKeyInput(KeypadKeyId.Space, false, repeat),
        _ => null
    };
}
