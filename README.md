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
| `ControlKeyHandling` | Input | Left/Right for Slider and Stepper, Esc in date and time dialogs, `KeyFocus.DefaultHighlight` |
| `KeyFocus` | Input | Skipped elements, wrap-around, movement range, Android's default focus color |
| `Focus` | Behaviors | Default focus |
| `EntryController` + `EntryBind` | Messaging / Behaviors | Handles the text, enabled state, and focus of an input field from the VM, and moves to the next field on Enter |
| `EntryOption` | Behaviors | No soft keyboard while a hardware keyboard is present, select all, Enter, input restrictions |
| `HardKeyboard` | Platforms/Android | Whether a hardware keyboard (including a keypad) is present |
| `ElementHelper` | Helpers | Moving and placing focus |
| `NavigationFocusPlugin` | Extender | Default focus of a screen, and focus restoration when navigating back |
| `PopupFocusPlugin` / `PopupInputPlugin` | Extender | Default focus of a popup and focus restoration after it closes / Adds key handling to popups |
| `IDialog` (MauiComponents) | MauiProgram | Dialogs (configured to focus the default button and cancel with Esc or DEL) |
| `ValidateInputsAsync` | Modules (AppViewModelBase) | Input field validation, and focus after an error |

## ⌨️ Key flow

```
MainActivity.DispatchKeyEvent / OnKeyListener of the popup dialog
  └─ KeyInputDriver ……… Converts device keys to ShortcutKey with KeyMap. Unhandled keys go to Android
       └─ InputManager ……… Dispatches to the topmost handler (the screen or an open popup)
            └─ InputBehavior
                 ├─ 1. Discards keys while busy (BusyState)
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
| `KeyFocus.Skip` | Element | Skipped by Up/Down/Left/Right movement |
| `KeyFocus.Wrap` | Screen, popup | Wraps from one end to the opposite end |
| `KeyFocus.Scope` | Container | Range of movement (set on the screen container in MainPage) |

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

Enter calls the command of `EntryController` and, unless `Handled`, moves to the next field.  
While a hardware keyboard is present, the soft keyboard does not appear even when an input field has focus (the `InputEntry` style).  
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

- `IDialog` (MauiComponents): Puts the focus on the default button (Cancel for Confirm), and cancels with Esc or DEL. After closing, the focus returns to the element that had it before opening
- Popups: The default focus is the element with `Focus.Default`. Esc cancels and closes the popup
- While busy (BusyState), keys are discarded

### Controls

`ConfigureControlKeyHandling()` in MauiProgram handles them as follows.

- Slider: Left/Right move it in 5% steps. Pressing at an end moves to the adjacent control
- Stepper: Left decreases the value, and Right increases it. Pressing at an end moves to the adjacent control
- DatePicker, TimePicker: Enter opens the dialog, and Esc cancels it

The color while focused is set by each style in Styles.xaml.

## 📱 Sample screens

| Screen | KeyMenu | Contents |
|---|---|---|
| Entry | 1 | Three input fields and a multi-line input field, Enter to advance, F2 to validate, F3 to enable/disable Input1, F4 to set a value in Input3 |
| List | 2 | List (Up/Down, number keys, Enter) |
| Dialog | 3 | Five dialogs, a numeric popup, Indicator, Loading |
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

- Hardware mode sends keys as a virtual USB keyboard (the device sees a hardware keyboard). Input mode sends `input keyevent`
- Connect starts sending. Disconnect or closing the tool removes the virtual keyboard
- The key labels and the codes to send can be changed in `Simulator:Keys` of `appsettings.json`. Keys and fields that are not written use the defaults below
- The numpad codes (`NumpadAndroidKey`, `NumpadLinuxKey`) are sent with Numpad, and for the digits, `-`, and `.` of the PC numpad
- The codes are sent without checking. In Hardware mode, Connect fails if a code is unknown to the device

| Name | Label | AndroidKey (Input) | LinuxKey (Hardware) | Numpad codes |
|---|---|---|---|---|
| F1 to F4 | F1 to F4 | `KEYCODE_F1` to `F4` | `KEY_F1` to `F4` | |
| Escape | ESC | `KEYCODE_ESCAPE` | `KEY_ESC` | |
| Up | ▲ | `KEYCODE_DPAD_UP` | `KEY_UP` | |
| Space | SPC | `KEYCODE_SPACE` | `KEY_SPACE` | |
| Left | ◀ | `KEYCODE_DPAD_LEFT` | `KEY_LEFT` | |
| Center | ● | `KEYCODE_DPAD_CENTER` | `KEY_SELECT` | |
| Right | ▶ | `KEYCODE_DPAD_RIGHT` | `KEY_RIGHT` | |
| Del | DEL | `KEYCODE_DEL` | `KEY_BACKSPACE` | |
| Down | ▼ | `KEYCODE_DPAD_DOWN` | `KEY_DOWN` | |
| Enter | ENT | `KEYCODE_ENTER` | `KEY_ENTER` | `KEYCODE_NUMPAD_ENTER` / `KEY_KPENTER` |
| Num0 to Num9 | 0 to 9 | `KEYCODE_0` to `9` | `KEY_0` to `9` | `KEYCODE_NUMPAD_0` to `9` / `KEY_KP0` to `9` |
| Minus | - | `KEYCODE_MINUS` | `KEY_MINUS` | `KEYCODE_NUMPAD_SUBTRACT` / `KEY_KPMINUS` |
| Period | . | `KEYCODE_PERIOD` | `KEY_DOT` | `KEYCODE_NUMPAD_DOT` / `KEY_KPDOT` |

```json
{
  "Simulator": {
    "Keys": {
      "F1": { "Label": "Back", "AndroidKey": "KEYCODE_BACK", "LinuxKey": "KEY_BACK" }
    }
  }
}
```

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
  - Focus restoration after a popup closes (`PopupFocusPlugin`)
  - `EntryController.Focus()`, and shortcuts to an already focused input field
- When the soft keyboard appears on a device with a hardware keyboard, it covers the bottom of the screen, and the IME may take keys first (it composes digits as full-width characters, and F keys do not work during composition)

### Workaround

- `EntryOption` wraps the Focus command of `EntryHandler` and `EditorHandler` with `CommandMapper.ModifyMapping` (when `DisableShowSoftInputOnFocus` is enabled). When the view is already focused and `ShowSoftInputOnFocus` is false, it returns only the result without going through the MAUI processing. The view already has focus, so the state does not change. Otherwise, the MAUI processing runs as is
- The wrapping is done after `UseMauiApp` (`ConfigureCustomBehaviors`) so that it wraps `InputView.MapFocus`, added by MAUI, from the outside
- The Prompt dialog (`SuppressPromptSoftInput` of `IDialog`) sets the window to `SoftInput.StateHidden` and the input field to `ShowSoftInputOnFocus = false` while a hardware keyboard is present
- On large screens (sw600dp and above), Android shows the soft keyboard when returning to a screen whose input field has focus (for example, coming back from the background). While a hardware keyboard is present, `MainActivity` sets the window to `SoftInput.StateHidden` (when the screen comes to the front, and when the keyboard is connected or disconnected)
- `HardKeyboard` tells whether a hardware keyboard is present. It counts an alphabetic keyboard (`qwerty` in the configuration) and a keyboard device with the number keys 0 to 9 (a keypad without letters, which leaves the configuration at `nokeys`). Devices without number keys, such as the volume buttons, are not counted. Connecting and disconnecting are received as configuration changes and as input device additions and removals, and the input field settings are updated
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
- With a keypad without letters, typing digits into a text input field makes the IME (Japanese Gboard) start composing and show the soft keyboard, and F keys do not work during composition. Set `Keyboard="Numeric"` on fields that take only digits (the digits go in as half-width characters without composition)
- MAUI's `DisplayPromptAsync` opens with `StateVisible`. For input, use `PromptAsync` of `IDialog`
- When updating MAUI, check whether the implementations of `InputView.MapFocus`, `InputViewExtensions.ShowKeyboardIfFocused`, and `TextViewExtensions.Focus` have changed

## 📦 Adding to template-maui

Files to use when adding physical key support to template-maui.  
Copy the files in "Copy", and replace the files in "Replace" with the files of this template.  
The files in "Merge" also contain features of template-maui, so add only the parts in the table instead of overwriting them.

### Copy

| File | Notes |
|---|---|
| `Input/` | All files |
| `Platforms/Android/KeyInputDriver.cs` / `KeyMap.cs` / `HardKeyboard.cs` | |
| `Extender/PopupInputPlugin.cs` | |
| `Modules/Key/` | Sample screens (when needed). Also add the `Key*` values to `ViewId` and an entry to a menu |

### Replace

| File | Difference from template-maui |
|---|---|
| `Helpers/ElementHelper.cs` / `ElementHelper.android.cs` | Focus movement in the order written in XAML (Skip, Wrap, Scope), and placing the focus on controls other than input fields. The public members of template-maui remain |
| `Extender/NavigationFocusPlugin.cs` / `PopupFocusPlugin.cs` | Places the restored focus also on controls other than input fields (`ElementHelper.RequestFocus`), and restores the focus after a popup closes |
| `Behaviors/EntryBind.cs` | Ignores Enter while keys are discarded (`InputManager.IsBusy`) |
| `Behaviors/EntryOption.android.cs` | Suppresses the soft keyboard only while a hardware keyboard is present |

### Merge

| File | What to add |
|---|---|
| `Extensions.cs` | `SetDefaultFocus` of this template (skips `KeyFocus.Skip`, and places the focus with `ElementHelper.RequestFocus`) |
| `Platforms/Android/MainActivity.cs` | The `KeyInputDriver` field and the `DispatchKeyEvent` override, and `StateHidden` of the window while a hardware keyboard is present (`OnResume` / `OnPause`) |
| `MainPage.xaml` | `InputBehavior` in the page behaviors, `KeyFocus.Scope` on the content area, and `Shortcut.Key` (Function1 to Function4) on the F key buttons |
| `MauiProgram.cs` | `HandleEnterKey` and `DisableShowSoftInputOnFocus` set to true, `ConfigureControlKeyHandling()`, `DismissKeys` / `IgnorePromptDismissKeys` / `EnableDialogButtonFocus` / `SuppressPromptSoftInput` of the dialog, and `AddComponentsPopupPlugin<PopupInputPlugin>()` |
| `Modules/Navigation/Modal/InputNumberView.xaml` | `Shortcut.Key` on the keypad buttons |
| `Resources/Styles/Styles.xaml` | The `Focused` state of the controls operated by keys, and `KeyFocus.DefaultHighlight` |
| `Settings.XamlStyler` | `input:*` in the attribute order |

## 🔗 Links

[template-maui](https://github.com/usausa/template-maui)
