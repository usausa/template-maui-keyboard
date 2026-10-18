namespace Template.Tools.KeypadSimulator.Behaviors;

using System.Windows.Controls.Primitives;

public static class KeyPress
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
        "Key",
        typeof(KeypadKey),
        typeof(KeyPress),
        new PropertyMetadata(null, OnKeyChanged));

    public static KeypadKey? GetKey(DependencyObject obj) => (KeypadKey?)obj.GetValue(KeyProperty);

    public static void SetKey(DependencyObject obj, KeypadKey? value) => obj.SetValue(KeyProperty, value);

    public static readonly DependencyProperty PressCommandProperty = DependencyProperty.RegisterAttached(
        "PressCommand",
        typeof(ICommand),
        typeof(KeyPress));

    public static ICommand? GetPressCommand(DependencyObject obj) => (ICommand?)obj.GetValue(PressCommandProperty);

    public static void SetPressCommand(DependencyObject obj, ICommand? value) => obj.SetValue(PressCommandProperty, value);

    public static readonly DependencyProperty ReleaseCommandProperty = DependencyProperty.RegisterAttached(
        "ReleaseCommand",
        typeof(ICommand),
        typeof(KeyPress));

    public static ICommand? GetReleaseCommand(DependencyObject obj) => (ICommand?)obj.GetValue(ReleaseCommandProperty);

    public static void SetReleaseCommand(DependencyObject obj, ICommand? value) => obj.SetValue(ReleaseCommandProperty, value);

    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State",
        typeof(PressState),
        typeof(KeyPress));

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((d is not ButtonBase button) || (button.GetValue(StateProperty) is not null))
        {
            return;
        }

        button.SetValue(StateProperty, new PressState());
        button.PreviewMouseLeftButtonDown += OnMouseDown;
        button.PreviewMouseLeftButtonUp += OnMouseUp;
        button.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseUpCompleted), true);
        button.LostMouseCapture += OnLostMouseCapture;
        button.Click += OnClick;
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var button = (ButtonBase)sender;
        var state = (PressState)button.GetValue(StateProperty);
        if (!state.Pressed)
        {
            state.Pressed = true;
            Execute(GetPressCommand(button), GetKey(button));
        }
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var button = (ButtonBase)sender;
        var state = (PressState)button.GetValue(StateProperty);
        state.SuppressClick = true;
        Release(button, state);
    }

    private static void OnMouseUpCompleted(object sender, MouseButtonEventArgs e)
    {
        var state = (PressState)((ButtonBase)sender).GetValue(StateProperty);
        state.SuppressClick = false;
    }

    private static void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        var button = (ButtonBase)sender;
        Release(button, (PressState)button.GetValue(StateProperty));
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        var button = (ButtonBase)sender;
        var state = (PressState)button.GetValue(StateProperty);
        if (state.SuppressClick || state.Pressed)
        {
            return;
        }

        var key = GetKey(button);
        Execute(GetPressCommand(button), key);
        Execute(GetReleaseCommand(button), key);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static void Release(ButtonBase button, PressState state)
    {
        if (state.Pressed)
        {
            state.Pressed = false;
            Execute(GetReleaseCommand(button), GetKey(button));
        }
    }

    private static void Execute(ICommand? command, KeypadKey? key)
    {
        if ((command is not null) && (key is not null) && command.CanExecute(key))
        {
            command.Execute(key);
        }
    }

    private sealed class PressState
    {
        public bool Pressed { get; set; }

        public bool SuppressClick { get; set; }
    }
}
