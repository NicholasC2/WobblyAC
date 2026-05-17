using System;
using System.Collections.Generic;

public class Variable
{
    public string name;
    private Func<object> getter;
    private Action<object> setter;
    public Type type;

    public Variable(string name, Func<object> getter, Action<object> setter, Type type)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.type = type;
    }

    public object GetValue()
    {
        return getter?.Invoke();
    }

    public void SetValue(object value)
    {
        setter?.Invoke(value);
    }
}

public static class VariableRegistry
{
    public static List<Variable> Variables = new List<Variable>();

    public static void Register(Variable variable)
    {
        Variables.Add(variable);
    }
}