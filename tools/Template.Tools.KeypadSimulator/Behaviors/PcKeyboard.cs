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
        Key.D0 => new PcKeyInput(KeypadKeys.Num0, false, repeat),
        Key.D1 => new PcKeyInput(KeypadKeys.Num1, false, repeat),
        Key.D2 => new PcKeyInput(KeypadKeys.Num2, false, repeat),
        Key.D3 => new PcKeyInput(KeypadKeys.Num3, false, repeat),
        Key.D4 => new PcKeyInput(KeypadKeys.Num4, false, repeat),
        Key.D5 => new PcKeyInput(KeypadKeys.Num5, false, repeat),
        Key.D6 => new PcKeyInput(KeypadKeys.Num6, false, repeat),
        Key.D7 => new PcKeyInput(KeypadKeys.Num7, false, repeat),
        Key.D8 => new PcKeyInput(KeypadKeys.Num8, false, repeat),
        Key.D9 => new PcKeyInput(KeypadKeys.Num9, false, repeat),
        Key.NumPad0 => new PcKeyInput(KeypadKeys.Num0, true, repeat),
        Key.NumPad1 => new PcKeyInput(KeypadKeys.Num1, true, repeat),
        Key.NumPad2 => new PcKeyInput(KeypadKeys.Num2, true, repeat),
        Key.NumPad3 => new PcKeyInput(KeypadKeys.Num3, true, repeat),
        Key.NumPad4 => new PcKeyInput(KeypadKeys.Num4, true, repeat),
        Key.NumPad5 => new PcKeyInput(KeypadKeys.Num5, true, repeat),
        Key.NumPad6 => new PcKeyInput(KeypadKeys.Num6, true, repeat),
        Key.NumPad7 => new PcKeyInput(KeypadKeys.Num7, true, repeat),
        Key.NumPad8 => new PcKeyInput(KeypadKeys.Num8, true, repeat),
        Key.NumPad9 => new PcKeyInput(KeypadKeys.Num9, true, repeat),
        Key.OemMinus => new PcKeyInput(KeypadKeys.Minus, false, repeat),
        Key.Subtract => new PcKeyInput(KeypadKeys.Minus, true, repeat),
        Key.OemPeriod => new PcKeyInput(KeypadKeys.Period, false, repeat),
        Key.Decimal => new PcKeyInput(KeypadKeys.Period, true, repeat),
        Key.Up => new PcKeyInput(KeypadKeys.Up, false, repeat),
        Key.Down => new PcKeyInput(KeypadKeys.Down, false, repeat),
        Key.Left => new PcKeyInput(KeypadKeys.Left, false, repeat),
        Key.Right => new PcKeyInput(KeypadKeys.Right, false, repeat),
        Key.Enter => new PcKeyInput(KeypadKeys.Enter, false, repeat),
        Key.Escape => new PcKeyInput(KeypadKeys.Escape, false, repeat),
        Key.Back or Key.Delete => new PcKeyInput(KeypadKeys.Del, false, repeat),
        Key.F1 => new PcKeyInput(KeypadKeys.F1, false, repeat),
        Key.F2 => new PcKeyInput(KeypadKeys.F2, false, repeat),
        Key.F3 => new PcKeyInput(KeypadKeys.F3, false, repeat),
        Key.F4 => new PcKeyInput(KeypadKeys.F4, false, repeat),
        _ => null
    };
}
