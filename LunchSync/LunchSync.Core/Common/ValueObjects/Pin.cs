using LunchSync.Core.Exceptions;

namespace LunchSync.Core.Common.ValueObjects;
public class Pin
{
    public string Value { get; private set; }

    private Pin(string value)
    {
        Value = value;
    }

    public static Pin Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 6 || !value.All(char.IsDigit))
        {
            throw new BusinessRuleViolationException("PIN must be exactly 6 digits.");
        }
        return new Pin(value);
    }

    public static Pin Generate()
    {
        // add your logic here!
        return new Pin("000000");
    }

    public override string ToString() => Value;
}
