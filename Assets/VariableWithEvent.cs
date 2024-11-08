using System;

public class VariableWithEvent<T>
{
    private T _value;

    // Event that fires when the value changes
    public event Action<T> OnValueChanged;

    // Property to get and set the value, firing the event on change
    public T Value
    {
        get => _value;
        set
        {
            // Check if the new value is different from the current one
            if (!Equals(_value, value))
            {
                _value = value;
                // Fire the event
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    // Constructor to initialize the variable
    public VariableWithEvent(T initialValue = default)
    {
        _value = initialValue;
    }
}

