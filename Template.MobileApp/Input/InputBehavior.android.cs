namespace Template.MobileApp.Input;

using Android.Content;
using Android.Views;

using AndroidX.Fragment.App;

using CommunityToolkit.Maui.Views;

public sealed partial class InputBehavior
{
    private static readonly ConditionalWeakTable<Window, DialogKeyListener> KeyListeners = [];

    private static partial void AttachPlatformKeyListener(Popup popup)
    {
        if ((popup.Handler?.PlatformView is not View view) || (ActivityResolver.CurrentActivity is not FragmentActivity activity))
        {
            return;
        }

        // MAUI はモーダルのページを DialogFragment のダイアログに表示する
        var root = view.RootView;
        foreach (var fragment in activity.SupportFragmentManager.Fragments)
        {
            if ((fragment is DialogFragment { Dialog: { Window: { } window } dialog }) && window.DecorView.Equals(root))
            {
                dialog.SetOnKeyListener(KeyListeners.GetValue(window, static x => new DialogKeyListener(x)));
                return;
            }
        }
    }

    private sealed class DialogKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
    {
        private readonly KeyInputDriver driver = new();

        private readonly Window window;

        public DialogKeyListener(Window window)
        {
            this.window = window;
        }

        public bool OnKey(IDialogInterface? dialog, Keycode keyCode, KeyEvent? e) =>
            (e is not null) && driver.Process(e, window.CurrentFocus);
    }
}
