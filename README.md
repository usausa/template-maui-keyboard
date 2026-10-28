# Template project for MAUI with physical keys

English | [日本語](README.ja.md)

For devices with physical keys, a MAUI template project for Android apps operated only with physical keys (digits, arrows, Enter, Esc, F1 to F4, and so on).

<img width="40%" src="Document/Images/HT.jpg" />

## ✨ Features

- Button shortcuts with number keys and F keys
- Complete focus movement with the Up/Down/Left/Right keys
- Input fields for hardware keyboards
- Focus control on input validation
- Lists operated by keys
- Dialogs operated only by keys

## 🧩 Classes

| Class | Location | Role |
|---|---|---|
| `KeyInputDriver` / `KeyMap` | Platforms/Android | The input part that passes physical key events to InputManager as logical key events |
| `InputManager` | Input | Management of screens and popups, and dispatching of events |
| `InputBehavior` | Input | Key handling for MainPage and popups |
| `Shortcut.Key` | Input | Marker for shortcuts |
| `ShortcutBehavior` | Input | Command execution by a shortcut |
| `CollectionViewShortcutBehavior` | Input | Handling of CollectionView by physical keys |
| `ControlKeyHandling` | Input | Handling of Left/Right for Slider and Stepper, Esc in the date and time dialogs, and so on |
| `KeyFocus` | Input | Marker for focus control by physical keys |
| `Focus` | Behaviors | Marker for focus control |
| `EntryController` + `EntryBind` | Messaging / Behaviors | Messenger and behavior that handle the text, enabled state, and focus of an input field from the VM |
| `EntryOption` | Behaviors | Settings such as no soft keyboard while a hardware keyboard is present, select all, Enter, and input restrictions |
| `HardKeyboard` | Platforms/Android | Detection of a hardware keyboard (including a keypad) |
| `ElementHelper` | Helpers | Focus movement |
| `NavigationFocusPlugin` | Extender | Default focus of a screen, and focus control when navigating back |
| `PopupFocusPlugin` / `PopupInputPlugin` | Extender | Default focus of a popup, and handling after it closes |
| `IDialog` (MauiComponents) | MauiProgram | Dialog settings for physical keys (cancel with Esc, and so on) |
| `ValidateInputsAsync` | Modules (AppViewModelBase) | Input field validation, and focus after an error |

## ⌨️ Key flow

```
MainActivity.DispatchKeyEvent / key event hook
  └─ KeyInputDriver ……… Conversion of physical key events to logical key events
       └─ InputManager ……… Dispatching of events to the topmost handler (screen or popup)
            └─ InputBehavior
                 ├─ 1. Decision on running the event while busy
                 ├─ 2. Shortcut handling
                 ├─ 3. Up/Down/Left/Right → focus movement (in XAML order)
                 ├─ 4. Shortcut.Key match → same action as a touch
                 └─ 5. Esc → go back, cancel the popup
```

## 🛠️ Usage

### Number keys and F keys

An element with `Shortcut.Key` performs the same action as a touch when that key is pressed.  
Button is tapped, CheckBox and Switch are toggled, RadioButton is selected, and other elements get the focus.

```xml
<Button Style="{StaticResource MenuButton}"
        Command="{Binding ForwardCommand}"
        CommandParameter="{markup:ViewId KeyEntry}"
        input:Shortcut.Key="Num1"
        Text="1.Entry" />
```

### Focus movement

Up/Down/Left/Right move the focus to the previous or next element in the order written in XAML.  
The range is limited to the screen, and by default the focus stops at the ends.

| Attached property | Set on | Behavior |
|---|---|---|
| `Focus.Default` | Element | Sets the focus when the screen opens (the first element if none) |
| `Focus.SuppressDefaultFocus` | Screen, popup | Does not set the default focus |
| `KeyFocus.Skip` | Element | Skipped by Up/Down/Left/Right movement |
| `KeyFocus.Wrap` | Screen, popup | Wraps from one end to the opposite end |
| `KeyFocus.Scope` | Container | Range of movement |

```xml
<ContentView ...
             input:KeyFocus.Wrap="True">
```

### Lists

Up/Down select a row, and from the first or last row the focus moves out of the list.  
The `KeyCode` key passes the selected row to `Command`.  
`SelectByNumberKey` selects rows 1 to 9 with keys 1 to 9.

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

### Input fields

Enter runs the command of `EntryController`, and the `Handled` result decides the move to the next field.  
While a hardware keyboard is present, the soft keyboard is not shown even when an input field has the focus (the `InputEntry` style).  
To show it, call `ShowSoftInputAsync()`.

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

### Input validation

Add DataAnnotations attributes to `EntryController` properties, and validate them in the order of the names with `ValidateInputsAsync`.  
Errors are stored in the VM's `Errors`.  
The VM needs `[GenerateAccessor]`.

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

### Dialogs and popups

- `IDialog` (MauiComponents): Configured so that the default button gets the focus and Esc cancels. After closing, the focus returns to the element that had it before opening
- Popups: The default focus is placed on the element with `Focus.Default`. Esc cancels and closes the popup
- While busy, key events are discarded

### Controls

`ConfigureControlKeyHandling()` in MauiProgram makes the following settings.

- Slider: Left/Right move it in 5% steps. At an end, the focus moves to the adjacent control
- Stepper: Left/Right decrease and increase the value. At an end, the focus moves to the adjacent control
- DatePicker, TimePicker: Enter opens the dialog, and Esc cancels it

## 📱 Sample screens

| Screen | KeyMenu | Contents |
|---|---|---|
| Entry | 1 | Three input fields and a multi-line input field, Enter for the action, F2 to validate, F3 to enable/disable Input1, F4 to set a value in Input3 |
| List | 2 | List (Up/Down, number keys, Enter) |
| Dialog | 3 | Dialogs, a numeric popup, Indicator, Loading |
| Control | 4 | CheckBox, Switch, RadioButton |
| Mixed | 5 | For checking various controls placed on one screen |

## 🔍 How to verify

Without a physical keyboard, send keys from a PC with the Keypad Simulator or adb to verify.

### Keypad Simulator

A WPF tool in `tools/Template.Tools.KeypadSimulator`.  
It sends the keys of a keypad with adb to a device connected for USB debugging.

<img width="40%" src="Document/Images/KeypadSimulator.png" />

- Hardware in Mode sends keys as a virtual USB keyboard (the device sees a physical keyboard). Input sends them with `input keyevent`
- Connect starts it, and Disconnect or closing the tool removes the virtual keyboard
- The settings can be changed in `appsettings.json`

### adb

With adb keyevent, the soft keyboard may appear and compose digits.

```
adb shell input keyevent KEYCODE_1
adb shell input keyevent KEYCODE_DPAD_DOWN
adb shell input keyevent KEYCODE_F1
adb shell input keyevent KEYCODE_ESCAPE
```

## ⚠️ Focus() showing the soft keyboard: problem and workaround

### MAUI implementation

- Calling `Focus()` on an Entry or Editor calls two methods in order on Android
  - 1: `InputView.MapFocus`. MAUI Controls maps it to the head of the Focus command
  - 2: `EntryHandler.MapFocus`. The handler's own, where the focus actually moves
- The first one has processing that shows the soft keyboard, and it shows whenever the view has focus, without looking at the value of `ShowSoftInputOnFocus`
  - The processing goes in the order `InputView.MapFocus` → `InputViewExtensions.ShowKeyboardIfFocused` → `ViewExtensions.ShowSoftInput` → `InputMethodManager.ShowSoftInput(view, 0)`
  - All `ShowKeyboardIfFocused` looks at is `IsFocused`
- The second one, `TextViewExtensions.Focus`, does look at `ShowSoftInputOnFocus`, and does not show the soft keyboard when it is false
- It is hard to disable this by overriding
  - The processing is in internal static methods of MAUI and cannot be overridden
  - Even with a class derived from `EntryHandler`, Focus goes through the static `EntryHandler.CommandMapper`, so avoiding it means replacing the whole handler and rebuilding Focus
  - The Android `EditText` side has no virtual method that stops the `InputMethodManager` call either

### Problem

- Setting `ShowSoftInputOnFocus` to false with `EntryOption.DisableShowSoftInputOnFocus` still shows the soft keyboard, because `ShowKeyboardIfFocused` does not look at it
- The Focus command has to be wrapped from the outside to keep `InputView.MapFocus` from being called

### Workaround

- Wrap the Focus command of `EntryHandler` and `EditorHandler` with `CommandMapper.ModifyMapping`
  - When the view already has focus and `ShowSoftInputOnFocus` is false, return only the result to `FocusRequest` without calling `InputView.MapFocus`
- The wrapping is done after `UseMauiApp` (`ConfigureCustomBehaviors`)
- `HardKeyboard` detects whether a hardware keyboard is present. Either of the following counts as present
  - `Configuration.Keyboard` is other than `nokeys` (`qwerty` for an alphabetic keyboard), and `HardKeyboardHidden` is not `yes`
  - `InputDevice` has a device treated as a keyboard that has all of the keys 0 to 9 (this is the case for a keypad without letters; `Configuration.Keyboard` stays at `nokeys`)
  - Devices without number keys, such as the volume buttons, do not count
- To show the soft keyboard explicitly, call `ShowSoftInputAsync()`

```csharp
// Behaviors/EntryOption.android.cs (same for Editor)
EntryHandler.CommandMapper.ModifyMapping(nameof(IEntry.Focus), static (handler, view, args, next) =>
{
    if (!SuppressFocusSoftInput(handler.PlatformView, view, args))
    {
        next?.Invoke(handler, view, args);
    }
});

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

### Notes

- With Material3 (`UseMaterial3`), the Entry and Editor handlers change to internal MAUI classes (`EntryHandler2`, `EditorHandler2`), and neither the Focus command wrapper nor the `EntryOption` mappers take effect
  - `UseMaterial3` is off in this template
- SearchBar is out of scope (the AndroidX SearchView shows the soft keyboard by itself)
- With a keypad without letters, typing digits into a text input field makes the IME show the soft keyboard, and F keys do not work during composition
  - Set `Keyboard="Numeric"` on input fields that use only digits
- When updating MAUI, check whether these implementations have changed
  - The side that shows the soft keyboard: `InputView.MapFocus`, `InputViewExtensions.ShowKeyboardIfFocused`, `ViewExtensions.ShowSoftInput`
  - The side the wrapper stops calling: `TextViewExtensions.Focus`

## 📦 Adding physical key support to template-maui

About the files used when adding physical key operation to template-maui.  
Add the files in "Copy" as they are, and replace the files in "Replace" with the files of this template.  
The files in "Merge" also contain features of template-maui, so merge only the parts in the table instead of overwriting them.

### Copy

| File | Notes |
|---|---|
| `Input/` | All |
| `Platforms/Android/KeyInputDriver.cs` / `KeyMap.cs` / `HardKeyboard.cs` | |
| `Extender/PopupInputPlugin.cs` | |

### Replace

| File | Difference from template-maui |
|---|---|
| `Helpers/ElementHelper.cs` / `ElementHelper.android.cs` | Focus movement in the order written in XAML, and focus control for elements other than input fields |
| `Extender/NavigationFocusPlugin.cs` / `PopupFocusPlugin.cs` | Focus control for a restored screen, and restoring the focus after a popup closes |
| `Behaviors/EntryBind.cs` | Enter handling |
| `Behaviors/EntryOption.android.cs` | Handling while a hardware keyboard is present |

### Merge

| File | What to add |
|---|---|
| `Extensions.cs` | Add `SetDefaultFocus` |
| `Platforms/Android/MainActivity.cs` | Add the `KeyInputDriver` field and the `DispatchKeyEvent` override, and `StateHidden` of the window while a hardware keyboard is present (`OnResume`, `OnPause`) |
| `MainPage.xaml` | Add `InputBehavior` to the page behaviors, `KeyFocus.Scope` to the content area, and `Shortcut.Key` (Function1 to Function4) to the F key buttons |
| `MauiProgram.cs` | `HandleEnterKey` and `DisableShowSoftInputOnFocus` set to true, `ConfigureControlKeyHandling()`, `DismissKeys` / `IgnorePromptDismissKeys` / `EnableDialogButtonFocus` / `SuppressPromptSoftInput` of the dialog, and `AddComponentsPopupPlugin<PopupInputPlugin>()` |

## 🔗 Links

[template-maui](https://github.com/usausa/template-maui)
