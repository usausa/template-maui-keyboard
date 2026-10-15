namespace Template.MobileApp.Behaviors;

using Android.Views;
using Android.Widget;

using Microsoft.Maui.Handlers;

public static partial class SliderOption
{
    private static readonly SliderKeyListener KeyListener = new();

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        if (options.HandleArrowKey)
        {
            SliderHandler.Mapper.AppendToMapping(nameof(BehaviorOptions.HandleArrowKey), static (handler, _) => handler.PlatformView.SetOnKeyListener(KeyListener));
        }
    }

    // 左右キーの 1 回の量を端までに抑える(MAUI の SeekBar の最大値は int.MaxValue で、端を越える加算があふれて反対の端へ戻る)
    private sealed class SliderKeyListener : Java.Lang.Object, View.IOnKeyListener
    {
        public bool OnKey(View? v, Keycode keyCode, KeyEvent? e)
        {
            if ((v is SeekBar seekBar) && (e?.Action == KeyEventActions.Down) && (keyCode is Keycode.DpadLeft or Keycode.DpadRight))
            {
                var step = (int)Math.Ceiling(((double)seekBar.Max - seekBar.Min) / 20);
                var increase = (keyCode == Keycode.DpadRight) == (seekBar.LayoutDirection == LayoutDirection.Ltr);
                seekBar.KeyProgressIncrement = increase
                    ? Math.Min(step, seekBar.Max - seekBar.Progress)
                    : Math.Min(step, seekBar.Progress - seekBar.Min);
            }

            // 値は SeekBar のキーの処理で変える(MAUI はキーやタッチで変わった値だけを受け取る)
            return false;
        }
    }
}
