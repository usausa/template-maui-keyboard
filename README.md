# template-maui-keyboard

物理キー(数字・矢印・Enter・Esc・F1〜F4 など)だけで操作する Android アプリの MAUI テンプレート。Keyence の BT-A500 のような物理キー付きの端末向け。

## ⌨️ キーの流れ

```
MainActivity.DispatchKeyEvent / ポップアップのダイアログの OnKeyListener
  └─ KeyInputDriver ……… KeyMap で端末のキーを ShortcutKey に変える。処理しないキーは Android へ
       └─ InputManager ……… いちばん上のハンドラー(画面か、開いているポップアップ)へ配る
            └─ InputBehavior
                 ├─ ① 処理中(BusyState)の間は捨てる
                 ├─ ② フォーカスのある要素の IShortcutBehavior(CollectionViewShortcutBehavior など)
                 ├─ ③ 上下左右 → フォーカスの移動(XAML に書いた順)
                 ├─ ④ Shortcut.Key が一致する要素 → タッチと同じ操作
                 └─ ⑤ Esc → 画面は戻る、ポップアップはキャンセルで閉じる
```

## 🔢 キーの対応表

| 端末のキー | ShortcutKey | 動く時 | 入力欄にフォーカスがある時 |
|---|---|---|---|
| ↑ ↓ | Up / Down | 押した時(押し続けると繰り返す) | アプリ(複数行の入力欄では行の移動が先) |
| ← → | Left / Right | 押した時(押し続けると繰り返す) | 入力欄が先(カーソルの移動)。端ではアプリ |
| Enter・テンキーの Enter・DPAD の中央 | Enter | 離した時 | 入力欄 |
| Esc | Escape | 離した時 | アプリ |
| Backspace・Clear | Clear | 離した時 | 入力欄 |
| -・テンキーの - | Minus | 離した時 | 入力欄 |
| .・テンキーの . | Period | 離した時 | 入力欄 |
| 0〜9・テンキーの 0〜9 | Num0〜Num9 | 離した時 | 入力欄 |
| F1〜F4 | Function1〜Function4 | 離した時 | アプリ |

左右は入力欄の外でもフォーカスのあるコントロールが先に処理し(Slider・Stepper の値など)、処理しない時にアプリがフォーカスを移す。表に無いキーとアプリで処理しなかったキーは Android へ渡る。対応表は `Platforms/Android/KeyMap.cs`。

## 🧩 クラス

| クラス | 場所 | 役割 |
|---|---|---|
| `KeyInputDriver` / `KeyMap` | Platforms/Android | 端末のキーを ShortcutKey に変えて InputManager へ渡す / キーの対応表 |
| `InputManager` | Input | 画面とポップアップのハンドラーを積み、いちばん上へキーを配る |
| `InputBehavior` | Input | 画面(MainPage)とポップアップのキーの処理 |
| `Shortcut.Key` | Input | 要素にキーを割り当てる |
| `ShortcutBehavior` | Input | フォーカスのある要素でキーを押した時に、コマンドを実行する |
| `CollectionViewShortcutBehavior` | Input | 一覧の行を上下・数字キーで選び、キーで決める |
| `ControlKeyHandling` | Input | Slider・Stepper の左右、日付・時刻のダイアログの Esc、`Focus.DefaultHighlight` |
| `Focus` | Behaviors | 既定のフォーカス・飛ばす要素・折り返し・移る範囲・Android の既定のフォーカスの色 |
| `EntryController` + `EntryBind` | Messaging / Behaviors | VM から入力欄の文字・有効・フォーカスを扱い、Enter で次の欄へ移す |
| `EntryOption` | Behaviors | ハードキーボードがある間はソフトキーボードを出さない・全選択・Enter・入力の制限 |
| `ElementHelper` | Helpers | フォーカスの移動と置き方 |
| `NavigationFocusPlugin` | Extender | 画面の既定のフォーカスと、戻った時のフォーカスの戻し |
| `PopupFocusPlugin` / `PopupInputPlugin` | Extender | ポップアップの既定のフォーカスと閉じた後の戻し / ポップアップにキーの処理を付ける |
| `KeypadDialog` | Components | `IDialog`。既定のボタンにフォーカス、Esc・クリアでキャンセル、Esc で消えない Indicator |
| `ValidateInputsAsync` | Modules(AppViewModelBase) | 入力欄の検証と、エラーの後のフォーカス |

## 🛠️ 使い方

### 数字キー・F キー

`Shortcut.Key` を付けた要素は、そのキーでタッチと同じ操作をする(Button は押す、CheckBox・Switch は切り替える、RadioButton は選ぶ、ほかの要素はフォーカスを置く)。F1〜F4 は画面の下の機能ボタン(`ShellProperty.Function1Text` など)。

```xml
<Button Style="{StaticResource MenuButton}"
        Command="{Binding ForwardCommand}"
        CommandParameter="{markup:ViewId KeyEntry}"
        input:Shortcut.Key="Num1"
        Text="1.Entry" />
```

### フォーカスの移動

上下左右で XAML に書いた順に前後へ移る。範囲は画面の中だけで、端では止まる。

| 添付プロパティ | 付ける所 | 動き |
|---|---|---|
| `Focus.Default` | 要素 | 画面を開いた時にフォーカスを置く(無ければ最初の要素) |
| `Focus.SuppressDefaultFocus` | 画面・ポップアップ | 既定のフォーカスを置かない |
| `Focus.Skip` | 要素 | 上下左右の移動で飛ばす |
| `Focus.Wrap` | 画面・ポップアップ | 端から反対の端へ折り返す |
| `Focus.Scope` | 入れ物 | 移る範囲(MainPage の画面の入れ物に付けてある) |

```xml
<ContentView ...
             behaviors:Focus.Wrap="True">
```

### 一覧

上下で行を選び、先頭・最後の行では一覧の外へ移る。`KeyCode` のキーで選んだ行を `Command` に渡す。`SelectByNumberKey` は 1〜9 キーで 1〜9 行目を選ぶ。

```xml
<CollectionView ItemsSource="{Binding Items}"
                SelectionMode="Single">
    <CollectionView.Behaviors>
        <input:CollectionViewShortcutBehavior Command="{Binding SelectCommand}"
                                              KeyCode="Enter"
                                              SelectByNumberKey="True" />
    </CollectionView.Behaviors>
</CollectionView>
```

### 入力欄

Enter で `EntryController` のコマンドを呼び、`Handled` でなければ次の欄へ移る。`Focus()` でその欄へフォーカスを置く。ハードキーボードがある間は、入力欄にフォーカスがあってもソフトキーボードを出さない(`InputEntry` のスタイル)。

```xml
<Entry Style="{StaticResource InputEntry}"
       behaviors:EntryBind.Controller="{Binding Input1}" />
```

```csharp
Input1 = new EntryController(MakeDelegateCommand<EntryCompleteEvent>(Input1Complete));

private void Input1Complete(EntryCompleteEvent ice)
{
    ice.Handled = String.IsNullOrEmpty(Input1.Text);
}
```

### 入力の検証

`EntryController` のプロパティに DataAnnotations の属性を付け、`ValidateInputsAsync` で名前の順に確かめる。最初のエラーのメッセージを Information で出し、その入力欄へフォーカスを移す。エラーは VM の `Errors` に入る。VM には `[GenerateAccessor]` が要る。

```csharp
[GenerateAccessor]
public sealed partial class KeyEntryViewModel : AppViewModelBase
{
    [Required(ErrorMessage = "Input1 is required")]
    public EntryController Input1 { get; }

    [Required(ErrorMessage = "Input3 must be 3 digits")]
    [RegularExpression("^[0-9]{3}$", ErrorMessage = "Input3 must be 3 digits")]
    public EntryController Input3 { get; }

    protected override async Task OnNotifyFunction2()
    {
        if (await ValidateInputsAsync(dialog, nameof(Input1), nameof(Input3)))
        {
            await dialog.InformationAsync("OK");
        }
    }
}
```

### ダイアログとポップアップ

- `IDialog`(`KeypadDialog`): 既定のボタン(Confirm は Cancel)にフォーカスを置き、Esc・クリアでキャンセルする(Prompt のクリアは文字を消す)。閉じた後は開く前の要素にフォーカスが戻る(タッチで閉じた時は戻らない)
- ポップアップ: 既定のフォーカスは `Focus.Default` の要素。Esc でキャンセルして閉じる(結果は無し)。閉じた後のフォーカスはダイアログと同じ
- 処理中(BusyState)の間はキーを捨てる。`Indicator` は Esc で消えない

### コントロール

MauiProgram の `ConfigureControlKeyHandling()` で次のように扱う。

- Slider: 左右で 5% ずつ動かす。端で押すと隣のコントロールへ移る
- Stepper: 全体で 1 つの止まり先にし、左で減らし、右で増やす。端で押すと隣へ移る
- DatePicker・TimePicker: Enter でダイアログを開き、Esc 1 回でキャンセルする
- 左右を押し続けた時は、入力欄やキーを処理していたコントロールの端で止まる

フォーカスのある間の色は Styles.xaml のスタイルで付ける。白地のコントロールは `SelectFocusColor`(`InputEntry`・`InputEditor`・`InputPicker`・`InputDatePicker`・`InputTimePicker`・`ToggleCheckBox`・`ToggleSwitch`・`ToggleRadioButton`・`InputSlider`・`InputStepper`)、色付きのボタンは各スタイルの Focused の色。

## 📱 見本の画面

| 画面 | KeyMenu | 内容 |
|---|---|---|
| Entry | 1 | 入力欄 3 つと複数行の入力欄、Enter 送り、F2 で検証、F3 で Input1 の有効・無効、F4 で Input3 に値 |
| List | 2 | 一覧(上下・数字キー・Enter) |
| Misc | 3 | ダイアログ 5 つ・数値のポップアップ・Indicator・Loading |
| Control | 4 | CheckBox・Switch・RadioButton(数字キー・Enter) |
| Mixed | 5 | 各種コントロールを 1 画面に置いた確かめ用 |

KeyMenu で戻る(BACK・Esc・F1)を押すと、確かめてからアプリを裏に回す。

## 🔍 確かめ方

USB か Bluetooth のキーボードを端末につなぐ。adb でも送れる(仮想のキーボードとして届くので、ソフトキーボードが出て数字を組み立てることがある)。

```
adb shell input keyevent KEYCODE_1
adb shell input keyevent KEYCODE_DPAD_DOWN
adb shell input keyevent KEYCODE_F1
adb shell input keyevent KEYCODE_ESCAPE
adb shell input keyevent KEYCODE_DEL
adb shell input keyevent --longpress KEYCODE_DPAD_RIGHT
```
