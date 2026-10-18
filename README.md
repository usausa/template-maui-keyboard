# Template project for MAUI with physical keys

English | [日本語](README.ja.md)

Template project for MAUI apps operated only with physical keys (digits, arrows, Enter, Esc, F1 to F4, and so on).  
For devices with physical keys.

<img width="40%" src="Document/Images/HT.jpg" />

## ✨ Features

- Button shortcuts with number keys and F keys
- Complete focus movement with Up/Down/Left/Right
- Lists operated by keys
- Input fields for hardware keyboards
- Focus on the first field with a validation error
- Dialogs, popups, and controls operated only by keys
- Keys discarded while busy
- Keypad Simulator (sends physical keys with adb)

## 🧩 Classes

| Class | Location | Role |
|---|---|---|
| `KeyInputDriver` / `KeyMap` | Platforms/Android | Converts device keys to ShortcutKey and passes them to InputManager / Key map |
| `InputManager` | Input | Stacks the handlers of screens and popups, and dispatches keys to the topmost one |
| `InputBehavior` | Input | Key handling for the screen (MainPage) and popups |
| `Shortcut.Key` | Input | Assigns a key to an element |
| `ShortcutBehavior` | Input | Executes a command when a key is pressed on the focused element |
| `CollectionViewShortcutBehavior` | Input | Selects a list row with Up/Down and number keys, and confirms it with a key |
| `ControlKeyHandling` | Input | Left/Right for Slider and Stepper, Esc in date and time dialogs, `Focus.DefaultHighlight` |
| `Focus` | Behaviors | Default focus, skipped elements, wrap-around, movement range, Android's default focus color |
| `EntryController` + `EntryBind` | Messaging / Behaviors | Handles the text, enabled state, and focus of an input field from the VM, and moves to the next field on Enter |
| `EntryOption` | Behaviors | No soft keyboard while a hardware keyboard is present, select all, Enter, input restrictions |
| `HardKeyboard` | Platforms/Android | Whether a hardware keyboard (including a keypad) is present. `EntryOption` and Prompt use it to decide whether to suppress the soft keyboard |
| `ElementHelper` | Helpers | Moving and placing focus |
| `NavigationFocusPlugin` | Extender | Default focus of a screen, and focus restoration when navigating back |
| `PopupFocusPlugin` / `PopupInputPlugin` | Extender | Default focus of a popup and focus restoration after it closes / Adds key handling to popups |
| `KeypadDialog` | Components | `IDialog`. Focus on the default button, cancel with Esc or DEL, an Indicator that Esc does not dismiss |
| `ValidateInputsAsync` | Modules (AppViewModelBase) | Input field validation, and focus after an error |

## ⌨️ Key flow

```
MainActivity.DispatchKeyEvent / OnKeyListener of the popup dialog
  └─ KeyInputDriver ……… Converts device keys to ShortcutKey with KeyMap. Unhandled keys go to Android
       └─ InputManager ……… Dispatches to the topmost handler (the screen or an open popup)
            └─ InputBehavior
                 ├─ 1. Discards keys while busy (BusyState). KeyInputDriver also discards keys for input fields and controls
                 ├─ 2. IShortcutBehavior of the focused element (CollectionViewShortcutBehavior, etc.)
                 ├─ 3. Up/Down/Left/Right → focus movement (in the order written in XAML)
                 ├─ 4. Element with a matching Shortcut.Key → same action as a touch
                 └─ 5. Esc → a screen goes back, a popup is canceled and closed
```

## 🛠️ Usage

### Number keys and F keys

An element with `Shortcut.Key` performs the same action as a touch when that key is pressed (presses a Button, toggles a CheckBox or Switch, selects a RadioButton, and puts focus on other elements).  
F1 to F4 are the function buttons at the bottom of the screen (`ShellProperty.Function1Text`, etc.).

```xml
<Button Style="{StaticResource MenuButton}"
        Command="{Binding ForwardCommand}"
        CommandParameter="{markup:ViewId KeyEntry}"
        input:Shortcut.Key="Num1"
        Text="1.Entry" />
```

### Focus movement

Up/Down/Left/Right move the focus to the previous or next element in the order written in XAML.  
The range is limited to the screen, and the focus stops at the ends.

| Attached property | Set on | Behavior |
|---|---|---|
| `Focus.Default` | Element | Gets the focus when the screen opens (the first element if none) |
| `Focus.SuppressDefaultFocus` | Screen, popup | Does not set the default focus |
| `Focus.Skip` | Element | Skipped by Up/Down/Left/Right movement |
| `Focus.Wrap` | Screen, popup | Wraps from one end to the opposite end |
| `Focus.Scope` | Container | Range of movement (set on the screen container in MainPage) |

```xml
<ContentView ...
             behaviors:Focus.Wrap="True">
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

Enter calls the command of `EntryController` and, unless `Handled`, moves to the next field.  
`Focus()` puts the focus on that field.  
While a hardware keyboard is present, the soft keyboard does not appear even when an input field has focus (the `InputEntry` style.  
It does not appear on `Focus()` to an already focused field either).  
To show it, call `ShowSoftInputAsync()`.

While busy (BusyState), keys for input fields and Enter confirmation (moving to the next field) are not accepted either.  
However, keys that the IME receives first (composing digits in Gboard's Japanese input, and committing them) do not go through the app's key handling, so they cannot be stopped even while busy.

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

Add DataAnnotations attributes to `EntryController` properties, and `ValidateInputsAsync` checks them in the order of the given names.  
It shows the message of the first error with Information and moves the focus to that input field.  
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

- `IDialog` (`KeypadDialog`): Puts the focus on the default button (Cancel for Confirm), and cancels with Esc or DEL (DEL in Prompt deletes characters). After closing, the focus returns to the element that had it before opening (not when closed by touch). Prompt also does not show the soft keyboard while a hardware keyboard is present
- Popups: The default focus is the element with `Focus.Default`. Esc cancels and closes the popup (no result). The focus after closing works the same as for dialogs
- While busy (BusyState), keys are discarded. Esc does not dismiss `Indicator`

### Controls

`ConfigureControlKeyHandling()` in MauiProgram handles them as follows.

- Slider: Left/Right move it in 5% steps. Pressing at an end moves to the adjacent control
- Stepper: The whole Stepper is a single focus stop, Left decreases the value, and Right increases it. Pressing at an end moves to the adjacent control
- DatePicker, TimePicker: Enter opens the dialog, and a single Esc cancels it
- When Left/Right is held down, the movement stops at the end of the input field or of the control that was handling the key

The color while focused is set by the styles in Styles.xaml.  
Controls with a white background use `SelectFocusColor` (`InputEntry`, `InputEditor`, `InputPicker`, `InputDatePicker`, `InputTimePicker`, `ToggleCheckBox`, `ToggleSwitch`, `ToggleRadioButton`, `InputSlider`, `InputStepper`), and colored buttons use the Focused color of each style.

## 🔢 Key map

Implementation example in this template (`Platforms/Android/KeyMap.cs`).

| Device key | ShortcutKey | Trigger | When an input field has focus |
|---|---|---|---|
| ↑ ↓ | Up / Down | Press (repeats while held) | App (in a multi-line input field, moving between lines comes first) |
| ← → | Left / Right | Press (repeats while held) | Input field first (cursor movement). App at the ends |
| Enter, numpad Enter, DPAD center | Enter | Release | Input field |
| Esc | Escape | Release | App |
| DEL (Backspace) | Del | Release | Input field |
| -, numpad - | Minus | Release | Input field |
| ., numpad . | Period | Release | Input field |
| 0 to 9, numpad 0 to 9 | Num0 to Num9 | Release | Input field |
| F1 to F4 | Function1 to Function4 | Release | App |

Even outside input fields, the focused control handles Left/Right first (such as Slider and Stepper values), and the app moves the focus when the control does not handle them.  
Keys not in the table and keys the app does not handle go to Android.

## 📱 Sample screens

| Screen | KeyMenu | Contents |
|---|---|---|
| Entry | 1 | Three input fields and a multi-line input field, Enter to advance, F2 to validate, F3 to enable/disable Input1, F4 to set a value in Input3 |
| List | 2 | List (Up/Down, number keys, Enter) |
| Misc | 3 | Five dialogs, a numeric popup, Indicator, Loading |
| Control | 4 | CheckBox, Switch, RadioButton (number keys, Enter) |
| Mixed | 5 | Various controls on one screen, for verification |

On KeyMenu, pressing Back (BACK, Esc, F1) asks for confirmation and then moves the app to the background.

## 🔍 How to verify

Connect a USB or Bluetooth keyboard to the device.  
Without one, send keys from a PC with the Keypad Simulator or adb.

### Keypad Simulator

A WPF tool in `tools/Template.Tools.KeypadSimulator`.  
It sends the keys of a keypad with adb to a device connected for USB debugging.

<img width="40%" src="Document/Images/KeypadSimulator.png" />

| Item | Contents |
|---|---|
| Device | Devices from `adb devices -l`. adb is searched for in `Simulator:AdbPath` of appsettings.json, `ANDROID_HOME`, `ANDROID_SDK_ROOT`, the default SDK folders, and PATH, in this order |
| Mode | Hardware: Registers a virtual USB keyboard with `uinput`. The device sees a hardware keyboard and repeats a key while it is held. Input: Sends `input keyevent`. The device has no hardware keyboard, so the soft keyboard and the IME may take keys first |
| Keyboard | Keyboard type for Hardware. Alphabetic: The device configuration becomes `qwerty`, and `HardKeyboard` is true. Numeric: The configuration stays `nokeys`, and `HardKeyboard` is false |
| Options | PC keys: While the window has focus, the PC's digits, numpad, arrows, Enter, Esc, Backspace, Delete, F1 to F4, - and . are sent as the same keys. Numpad: Digits, ENT, - and . are sent as numpad keys (the PC's numpad keys always are) |
| 📌 | Keeps the window on top |

- Connect starts sending. Disconnect or closing the tool removes the virtual keyboard
- The device, mode, options, and window position are kept for the next start

### adb

Keys can also be sent with adb (they arrive from a virtual keyboard, so the soft keyboard may appear and compose digits.  
Sending Tab in an input field makes Gboard show the soft keyboard by itself).

```
adb shell input keyevent KEYCODE_1
adb shell input keyevent KEYCODE_DPAD_DOWN
adb shell input keyevent KEYCODE_F1
adb shell input keyevent KEYCODE_ESCAPE
adb shell input keyevent KEYCODE_DEL
adb shell input keyevent --longpress KEYCODE_DPAD_RIGHT
```

## ⚠️ Focus() showing the soft keyboard: problem and workaround

### MAUI implementation

- The Android implementation of MAUI has a call to `InputMethodManager.ShowSoftInput(view, 0)`, which explicitly shows the soft keyboard
- Calling `Focus()` on an already focused Entry or Editor makes this call without checking `ShowSoftInputOnFocus`
- The path is `InputView.MapFocus` → `InputViewExtensions.ShowKeyboardIfFocused` → `ViewExtensions.ShowSoftInput`. `UseMauiApp` prepends `InputView.MapFocus` to the Focus command of Entry and Editor
- It is hard to fix with overrides
  - All three are internal static methods of MAUI and cannot be overridden
  - Even with a class derived from `EntryHandler`, Focus goes through the static `EntryHandler.CommandMapper`. Avoiding it means replacing the whole handler and rebuilding Focus
  - The Android `EditText` side has no virtual method that stops the `InputMethodManager` call either
- Therefore, the Focus command is wrapped from the outside, and the original processing is skipped ("Workaround" below)

### Problem

- Setting `ShowSoftInputOnFocus` to false with `EntryOption.DisableShowSoftInputOnFocus` does not stop this call
- `Focus()` on an already focused input field is common in this template
  - Default focus when a screen opens (`NavigationFocusPlugin`)
  - Focus restoration after a dialog or popup closes (`FocusHelper`, `PopupFocusPlugin`)
  - `EntryController.Focus()`, and shortcuts to an already focused input field
- When the soft keyboard appears on a device with a hardware keyboard, it covers the bottom of the screen, and the IME may take keys first (it composes digits as full-width characters, and F keys do not work during composition)

### Workaround

- `EntryOption` wraps the Focus command of `EntryHandler` and `EditorHandler` with `CommandMapper.ModifyMapping` (when `DisableShowSoftInputOnFocus` is enabled). When the view is already focused and `ShowSoftInputOnFocus` is false, it returns only the result without going through the MAUI processing. The view already has focus, so the state does not change. Otherwise, the MAUI processing runs as is
- The wrapping is done after `UseMauiApp` (`ConfigureCustomBehaviors`) so that it wraps `InputView.MapFocus`, added by MAUI, from the outside
- The Prompt dialog (`KeypadDialog`) sets the window to `SoftInput.StateHidden` and the input field to `ShowSoftInputOnFocus = false` while a hardware keyboard is present
- `HardKeyboard` tells whether a hardware keyboard is present. Connecting and disconnecting are received as configuration changes, and the input field settings are updated
- To show the soft keyboard, call `ShowSoftInputAsync()` (if the field is already focused, it shows the keyboard without going through the Focus command)

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

- With Material3 (`UseMaterial3`), the Entry and Editor handlers change to internal MAUI classes (`EntryHandler2`, `EditorHandler2`), and neither the Focus command wrapper nor the `EntryOption` mappers take effect. This template has `UseMaterial3` commented out
- SearchBar is out of scope (the AndroidX SearchView shows the soft keyboard by itself)
- MAUI's `DisplayPromptAsync` opens with `StateVisible`. For input, use `PromptAsync` of `IDialog`
- When updating MAUI, check whether the implementations of `InputView.MapFocus`, `InputViewExtensions.ShowKeyboardIfFocused`, and `TextViewExtensions.Focus` have changed

## 🔗 Links

[template-maui](https://github.com/usausa/template-maui)
