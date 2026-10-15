# 物理キーボード端末でのソフトキーボード抑止 実装プラン (Android)

対象: Microsoft.Maui.Controls 10.0.110 / Android

## 現状の仕組み

- `EntryOption.DisableShowSoftInputOnFocus` (添付プロパティ、バインド・実行時切替可)
- `BehaviorOptions.DisableShowSoftInputOnFocus = true` のとき、Entry / Editor の `ShowSoftInputOnFocus` を、ハードキーボード(キーパッドを含む)がある間だけ `false` にする
- ハードキーボードの有無が変わったら `IComponentCallbacks.OnConfigurationChanged` で設定し直す
- スタイル `InputEntry` / `InputEditor` で有効

これで抑止できるもの:

- タップ (`TextView.onTouchEvent`)
- DPAD_CENTER (`TextView.onKeyUp`)
- 未フォーカスの入力欄への `Focus()` (`TextViewExtensions.Focus` が `ShowSoftInputOnFocus` を見る)

`SoftInputExtensions.ShowSoftInputAsync()` などで明示的に出す操作はそのまま使える。

## 残っている表示経路

| # | 経路 | 対応 |
|---|------|------|
| 1 | フォーカス済みの Entry / Editor への `Focus()`。`InputView.MapFocus` → `ShowKeyboardIfFocused` が `ShowSoftInputOnFocus` を見ずに `InputMethodManager.ShowSoftInput(view, 0)` を呼ぶ | 対応1 |
| 2 | `KeypadDialog` の Prompt。`SetSoftInputMode(SoftInput.StateVisible)` と素の `EditText` で、添付プロパティの対象外 | 対応2 |
| 3 | ウィンドウがフォーカスを得た時の IME の自動表示 (`SOFT_INPUT_STATE_UNSPECIFIED` で、大画面か adjustResize のとき) | 対応3 (実機で確認してから) |
| 4 | Material3 (`EntryHandler2` / `EditorHandler2`)。PlatformView が `TextInputLayout` で、今のマッパーが効かない | 対象外 (`UseMaterial3` を有効にする時に対応) |
| 5 | SearchBar (AndroidX `SearchView` が自分で `showSoftInput` を呼ぶ) | 対象外 |

経路1を通るアプリ側の呼び出し:

- `PopupFocusPlugin.OnClosed` (キーで閉じた時に、開く前の入力欄へ戻す。ポップアップは別ウィンドウなので、下の入力欄は `IsFocused` のまま)
- `InputBehavior.InvokeShortcut` (ショートカットの対象がフォーカス済みの入力欄の時)
- `EntryBind` の `EntryController.Focus()`
- `FocusHelper.WithRestoreFocus`
- `NavigationFocusPlugin` のフォーカスの復元 (戻った時点で `IsFocused` が残っている場合)

## 対応1: Focus コマンドのラップ

`EntryOption.android.cs` の `DisableShowSoftInputOnFocus` の分岐に追加する。

```csharp
EntryHandler.CommandMapper.ModifyMapping(nameof(IEntry.Focus), static (handler, view, args, next) =>
{
    if (!SuppressFocusSoftInput(handler.PlatformView, view, args))
    {
        next?.Invoke(handler, view, args);
    }
});
EditorHandler.CommandMapper.ModifyMapping(nameof(IEditor.Focus), static (handler, view, args, next) =>
{
    if (!SuppressFocusSoftInput(handler.PlatformView, view, args))
    {
        next?.Invoke(handler, view, args);
    }
});
```

```csharp
// フォーカス済みの入力欄への Focus() は MAUI が ShowSoftInputOnFocus を見ずにソフトキーボードを出すので、元の処理を通さない
private static bool SuppressFocusSoftInput(TextView editText, IView view, object? args)
{
    if ((args is not FocusRequest request) || !view.IsFocused || editText.ShowSoftInputOnFocus)
    {
        return false;
    }

    request.TrySetResult(true);
    return true;
}
```

- 登録は `UseMauiApp<App>()` より後で行う。`UseMauiApp` の中の `RemapForControls` が `InputView.MapFocus` を Prepend するので、それより後に `ModifyMapping` すれば外側から包める (今の `UseCustomView()` の位置で満たしている)
- 抜けるのは「フォーカス済み かつ `ShowSoftInputOnFocus == false`」の時だけ。それ以外は元の処理 (`InputView.MapFocus` → `EntryHandler.MapFocus`) に任せる
- フォーカス済みなので、元の処理を通さなくてもフォーカスの状態は変わらない
- `ShowSoftInputAsync()` は、フォーカス済みなら Focus コマンドを通らずに直接表示し、未フォーカスなら元の処理でフォーカスしてから表示するので、影響しない

## 対応2: Prompt ダイアログ

- `EntryOption` のハードキーボードの判定 (`hardKeyboard`、`HasHardKeyboard`、`KeyboardCallbacks`) を、Android 用の internal static クラス (例: `Platforms/Android/HardKeyboard.cs`) へ移し、`KeypadDialog` からも参照できるようにする
  - 例: `HardKeyboard.Start()`、`HardKeyboard.SuppressSoftInput` (オプションが有効 かつ ハードキーボードあり)、`HardKeyboard.Changed`
- `KeypadDialog.android.cs` の `PromptAsync` で、`HardKeyboard.SuppressSoftInput` の時は
  - `SetSoftInputMode(SoftInput.StateVisible)` を `SoftInput.StateHidden` にする
  - `editText.ShowSoftInputOnFocus = false` にする

## 対応3: ウィンドウのフォーカス取得時の自動表示 (要実機確認)

- 確認する端末: 大画面 (sw600dp 以上) の端末、ポップアップの中の入力欄、アプリをバックグラウンドから戻した時
- 再現した場合のみ、ハードキーボードがある間はウィンドウの state を `SoftInput.StateHidden` にする
  - `WindowSoftInputModeAdjust="Pan"` は `Window.SetSoftInputMode(SoftInput.AdjustPan)` で state を含めて上書きするので、その後で `AdjustPan | StateHidden` を設定する
  - モーダル (ポップアップ) のダイアログは Activity の `SoftInputMode` を引き継ぐ

## 確認手順

1. エミュレーター (AVD の `hw.keyboard=yes`) で、設定 > システム > 言語と入力 > 物理キーボード の「画面キーボードの使用」を ON にして、ハードキーボードがあってもソフトキーボードが出る端末の状態にする
2. 各操作でソフトキーボードが出ないこと
   - 入力欄のタップ
   - Tab / 矢印 / Enter でのフォーカスの移動
   - 入力欄にフォーカスがある状態で、キーでポップアップを開いてキーで閉じる (対応1 の前は出る)
   - フォーカス済みの入力欄へのショートカットキー、`EntryController.Focus()` (対応1 の前は出る)
   - Prompt ダイアログ (対応2 の前は出る)
3. `ShowSoftInputAsync()` ではソフトキーボードが出ること
4. ハードキーボードの無い端末 (`hw.keyboard=no` の AVD、実機ならキーボードを外した状態) では、タップでソフトキーボードが出ること
5. 出た時は `adb shell dumpsys input_method` の show/hide の履歴で、表示の理由 (明示の `SHOW_SOFT_INPUT` か、自動表示か) を見る
6. ビルドの警告が無いこと

## MAUI の更新時

- `InputView.MapFocus` / `InputViewExtensions.ShowKeyboardIfFocused` / `TextViewExtensions.Focus` の実装が変わっていないかを確認する
