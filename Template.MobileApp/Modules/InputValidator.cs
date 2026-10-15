namespace Template.MobileApp.Modules;

// 入力の規則(条件・エラーのメッセージ・エラーの後の処理)。入力欄の規則は、エラーの後にその入力欄へフォーカスを移す
public sealed class ValidationRule
{
    public Func<bool> Validation { get; }

    public string ErrorMessage { get; }

    public Action? PostAction { get; }

    private ValidationRule(Func<bool> validation, string errorMessage, Action? postAction)
    {
        Validation = validation;
        ErrorMessage = errorMessage;
        PostAction = postAction;
    }

    public static ValidationRule Make(Func<bool> validation, string errorMessage, Action? postAction = null) =>
        new(validation, errorMessage, postAction);

    public static ValidationRule Required(EntryController input, string errorMessage) =>
        new(() => !String.IsNullOrEmpty(input.Text), errorMessage, input.Focus);

    public static ValidationRule Length(EntryController input, int length, string errorMessage) =>
        new(() => (input.Text?.Length ?? 0) == length, errorMessage, input.Focus);

    public static ValidationRule Make(EntryController input, Func<string?, bool> validation, string errorMessage) =>
        new(() => validation(input.Text), errorMessage, input.Focus);
}

// 規則を順に確かめ、最初に外れた規則のメッセージを出してから、その規則の後の処理をする
public sealed class InputValidator
{
    private readonly IDialog dialog;

    private readonly ValidationRule[] rules;

    public InputValidator(IDialog dialog, IEnumerable<ValidationRule> rules)
    {
        this.dialog = dialog;
        this.rules = rules.ToArray();
    }

    public async ValueTask<bool> ValidateAsync()
    {
        foreach (var rule in rules)
        {
            if (!rule.Validation())
            {
                await dialog.InformationAsync(rule.ErrorMessage);
                rule.PostAction?.Invoke();
                return false;
            }
        }

        return true;
    }
}
