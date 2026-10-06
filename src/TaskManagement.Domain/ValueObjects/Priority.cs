namespace TaskManagement.Domain.ValueObjects;

public readonly record struct Priority
{
    private static readonly HashSet<int> ValidValues = new() { 0, 1, 2, 3, 4 };

    public int Value { get; }

    public Priority(int value)
    {
        if (!ValidValues.Contains(value))
            throw new ArgumentOutOfRangeException(nameof(value), $"Priority must be between 0 and 4. Received: {value}");
        Value = value;
    }

    public static implicit operator int(Priority priority) => priority.Value;
    public static implicit operator Priority(int value) => new(value);

    public static Priority Highest => new(0);
    public static Priority High => new(1);
    public static Priority Medium => new(2);
    public static Priority Low => new(3);
    public static Priority Lowest => new(4);

    public string ToDisplayString() => Value switch
    {
        0 => "Highest",
        1 => "High",
        2 => "Medium",
        3 => "Low",
        4 => "Lowest",
        _ => Value.ToString()
    };

    public override string ToString() => ToDisplayString();
}