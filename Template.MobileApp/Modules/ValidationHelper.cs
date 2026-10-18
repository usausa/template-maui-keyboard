namespace Template.MobileApp.Modules;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Smart.Mvvm.Resolver;

internal static class ValidationHelper
{
    [ThreadStatic]
    private static List<ValidationResult>? validationResults;

    public static void Validate(object target, string name, ErrorInfo errors)
    {
        var accessor = AccessorProvider.FindAccessor(target.GetType());
        if (accessor is null)
        {
            throw new InvalidOperationException($"Accessor is not supported. type=[{target.GetType()}]");
        }

        var value = accessor.GetValue(target, name);
        var context = new ValidationContext(target, ResolveProvider.Default, null)
        {
            MemberName = name
        };
        validationResults ??= [];

        var valid = value is EntryController input
            ? Validator.TryValidateValue(input.Text ?? string.Empty, context, validationResults, target.GetType().GetProperty(name)!.GetCustomAttributes<ValidationAttribute>())
            : Validator.TryValidateProperty(value, context, validationResults);
        if (!valid)
        {
            errors.AddError(name, validationResults[0].ErrorMessage!);
        }

        validationResults.Clear();
    }

    public static async ValueTask<bool> ValidateInputsAsync(object target, ErrorInfo errors, IDialog dialog, string[] names)
    {
        foreach (var name in names)
        {
            errors.ClearErrors(name);
            Validate(target, name, errors);
        }

        var error = Array.Find(names, errors.Contains);
        if (error is not null)
        {
            await dialog.InformationAsync(errors[error]!);
            (AccessorProvider.FindAccessor(target.GetType())?.GetValue(target, error) as EntryController)?.Focus();
        }

        return error is null;
    }
}
