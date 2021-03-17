using System;
using System.Collections.Generic;

enum ValueType
{
    NIL,
    NUM, BOOL, STR,
    OBJ,
    ANY
}

enum ObjectType
{
    FUNCTION, NATIVE, CLASS
}

class Value
{
    internal readonly ValueType type;

    private Value()
    {
        type = ValueType.NIL;
    }

    internal Value(ValueType vType)
    {
        type = vType;
    }

    internal static Value Default(ValueType val)
    {
        switch (val)
        {
            case ValueType.BOOL: return BoolValue.FALSE;
            case ValueType.NIL: return NIL;
            case ValueType.NUM: return NumValue.ZERO;
            case ValueType.STR: return StrValue.EMPTY;
        }
        return null;
    }

    internal static bool Equal(Value a, Value b)
    {
        if(a.type != b.type)
        {
            return false;
        }

        switch(a.type)
        {
            case ValueType.BOOL: return (a as BoolValue).value == (b as BoolValue).value;
            case ValueType.NIL: return true;
            case ValueType.NUM: return (a as NumValue).value == (b as NumValue).value;
            case ValueType.STR: return (a as StrValue).value == (b as StrValue).value;
            default: return false;
        }
    }

    internal bool IsNotNum() => !(this is NumValue);
    internal bool IsNotBool() => !(this is BoolValue);
    internal bool IsNotStr() => !(this is StrValue);

    internal bool IsFalse() => type == ValueType.NIL || (this is BoolValue && !(this as BoolValue).value);

    internal static string TypeStr(ValueType type)
    {
        switch(type)
        {
            case ValueType.BOOL: return "bool";
            case ValueType.NIL: return "nil";
            case ValueType.NUM: return "num";
            case ValueType.STR: return "str";
            default: return "ERROR";
        }
    }

    internal readonly static Value NIL = new Value();
    private const string NIL_STRING = "Nil";

    internal virtual string ToPrint() => NIL_STRING;
    public override string ToString() => NIL_STRING;
}

class NumValue : Value
{
    private static Dictionary<int, NumValue> interned = new Dictionary<int, NumValue>();
    internal readonly static NumValue ZERO = NewNum(0);
    internal readonly double value;

    internal NumValue() : base(ValueType.NUM) { }

    private NumValue(double val) : base(ValueType.NUM)
    {
        value = val;
    }

    internal static NumValue NewNum(double val)
    {
        if(Math.Abs(val % 1) > double.Epsilon * 100)
        {
            return new NumValue(val);
        }

        int intVal = (int)Math.Round(val);
        if (!interned.TryGetValue(intVal, out NumValue num))
        {
            num = new NumValue(val);
            interned.Add(intVal, num);
        }
        return num;
    }

    public static NumValue operator -(NumValue num) => new NumValue(-num.value);
    public static NumValue operator +(NumValue a, NumValue b) => new NumValue(a.value + b.value);
    public static NumValue operator -(NumValue a, NumValue b) => new NumValue(a.value - b.value);
    public static NumValue operator *(NumValue a, NumValue b) => new NumValue(a.value * b.value);
    public static NumValue operator /(NumValue a, NumValue b) => new NumValue(a.value / b.value);
    public static NumValue operator %(NumValue a, NumValue b) => new NumValue(a.value % b.value);
    public static bool operator <(NumValue a, NumValue b) => a.value < b.value;
    public static bool operator >(NumValue a, NumValue b) => a.value > b.value;

    internal override string ToPrint() => value.ToString();
    public override string ToString() => value.ToString();
}

class BoolValue : Value
{
    internal readonly static BoolValue TRUE = new BoolValue(true);
    internal readonly static BoolValue FALSE = new BoolValue(false);

    internal readonly bool value;

    internal BoolValue() : base(ValueType.BOOL) { }

    private BoolValue(bool val) : base(ValueType.BOOL)
    {
        value = val;
    }

    internal static BoolValue Get(bool value)
    {
        if (value)
        {
            return TRUE;
        }
        return FALSE;
    }

    internal BoolValue And(BoolValue b) => Get(value && b.value);
    internal BoolValue Or(BoolValue b) => Get(value || b.value);
    internal BoolValue Not() => Get(!value);

    internal override string ToPrint() => value.ToString();
    public override string ToString() => value.ToString();
}

class StrValue : Value
{
    private static Dictionary<string, StrValue> interned = new Dictionary<string, StrValue>();
    internal readonly static StrValue EMPTY = NewString(string.Empty);
    internal readonly string value;

    private StrValue(string val) : base(ValueType.STR)
    {
        value = val;
    }

    internal static StrValue NewString(string val)
    {
        if(!interned.TryGetValue(val, out StrValue str))
        {
            str = new StrValue(val);
            interned.Add(val, str);
        }
        return str;
    }

    public static StrValue operator +(StrValue a, StrValue b) => NewString(a.value + b.value);

    internal override string ToPrint() => value;
    public override string ToString() => value;
}

abstract class Object : Value
{
    internal ObjectType objType;
    internal Object(ObjectType type) : base(ValueType.OBJ)
    {
        objType = type;
    }
}

class Function : Object
{
    private static Dictionary<StrValue, List<Function>> funcPool = new Dictionary<StrValue, List<Function>>();
    internal enum Type
    {
        SCRIPT, FUNCTION
    }

    internal int arity;
    internal List<ValueType> parameters;
    internal ValueType returnType;
    internal int location;
    internal StrValue name;
    internal Type functionType;

    private Function(List<ValueType> parameters, ValueType returnType, StrValue name, Type type) : base(ObjectType.FUNCTION)
    {
        arity = parameters.Count;
        this.parameters = parameters;
        this.returnType = returnType;
        this.name = name;
        functionType = type;
    }

    internal static Function newFunction(List<ValueType> parameters, ValueType returnType, StrValue name, Type type)
    {
        Function function;
        if(funcPool.TryGetValue(name, out List<Function> overloads))
        {
            foreach(Function func in overloads)
            {
                if(func.parameters.ListEquals(parameters))
                {
                    return null;
                }
            }
            function = new Function(parameters, returnType, name, type);
            overloads.Add(function);
            return function;
        }
        function = new Function(parameters, returnType, name, type);
        funcPool.Add(name, new List<Function>() { function });
        return function;
    }

    internal List<Function> GetOverloads() => funcPool[name];

    internal override string ToPrint() => string.Format("<fun {0}()>", name);

    public override string ToString() => string.Format("<fun {0} {1}({2})>", returnType.Str(), name, parameters.AsString());
}

class Native : Object
{
    internal Func<Value> zeroOverload;
    internal Dictionary<ValueType, Func<Value, Value>> oneOverloads;
    internal Dictionary<Tuple2, Func<Value, Value, Value>> twoOverloads;
    internal Dictionary<Tuple3, Func<Value, Value, Value, Value>> threeOverloads;

    internal ushort id;
    internal string name;

    internal Native(ushort id, string name) : base(ObjectType.NATIVE)
    {
        this.id = id;
        this.name = name;
        oneOverloads = new Dictionary<ValueType, Func<Value, Value>>();
        twoOverloads = new Dictionary<Tuple2, Func<Value, Value, Value>>();
        threeOverloads = new Dictionary<Tuple3, Func<Value, Value, Value, Value>>();
    }

    internal void AddOverload(Func<Value> function) => zeroOverload = function;
    internal void AddOverload(Func<Value, Value> function, ValueType param) => oneOverloads.Add(param, function);
    internal void AddOverload(Func<Value, Value, Value> function, ValueType param1, ValueType param2) => twoOverloads.Add(Tuple2.New(param1, param2), function);
    internal void AddOverload(Func<Value, Value, Value, Value> function, ValueType param1, ValueType param2, ValueType param3) => threeOverloads.Add(Tuple3.New(param1, param2, param3), function);

    internal Func<Value> GetOverload() => zeroOverload;

    internal Func<Value, Value> GetOverload(ValueType param)
    {
        if (oneOverloads.TryGetValue(param, out Func<Value, Value> function))
        {
            return function;
        }
        return null;
    }

    internal Func<Value, Value, Value> GetOverload(ValueType param1, ValueType param2)
    {
        if (twoOverloads.TryGetValue(Tuple2.New(param1, param2), out Func<Value, Value, Value> function))
        {
            return function;
        }
        return null;
    }

    internal Func<Value, Value, Value, Value> GetOverload(ValueType param1, ValueType param2, ValueType param3)
    {
        if (threeOverloads.TryGetValue(Tuple3.New(param1, param2, param3), out Func<Value, Value, Value, Value> function))
        {
            return function;
        }
        return null;
    }

    //internal Native SetMax(int max)
    //{
    //    if (maxParams < max) maxParams = max;
    //    return this;
    //}

    internal override string ToPrint() => string.Format("<fun {0}()>", name);
    public override string ToString() => "<native fun>";
}

class Class : Object
{
    internal ushort id;
    internal List<ValueType> memberTypes;

    internal Class(ushort id) : base(ObjectType.CLASS)
    {
        this.id = id;
    }
}