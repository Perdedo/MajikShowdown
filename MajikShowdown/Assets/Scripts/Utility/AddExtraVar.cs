using System;

[AttributeUsage(AttributeTargets.Field)]
public class AddExtraVar : Attribute
{
    public string DisplayName;

    public AddExtraVar(string displayName)
    {
        DisplayName = displayName;
    }
}