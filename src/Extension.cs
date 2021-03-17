using System.Collections.Generic;

public static class Extension
{
    internal static string Lexeme(this string source, Token token)
    {
        return source.Substring(token.start, token.length);
    }

    internal static string AsString(this List<ValueType> list)
    {
        string str = "";
        if(list.Count == 0)
        {
            return str;
        }
        foreach(ValueType type in list)
        {
            str += type.Str() + ", ";
        }
        return str.Substring(0, str.Length - 2);
    }

    internal static string AsString(this ValueType[] list)
    {
        string str = "";
        if (list.Length == 0)
        {
            return str;
        }
        foreach (ValueType type in list)
        {
            str += type.Str() + ", ";
        }
        return str.Substring(0, str.Length - 2);
    }

    internal static string Str(this ValueType val)
    {
        switch (val)
        {
            case ValueType.BOOL: return "bool";
            case ValueType.NIL: return "nil";
            case ValueType.NUM: return "num";
            case ValueType.STR: return "str";
        }
        return null;
    }

    internal static bool ListEquals(this List<ValueType> list, List<ValueType> newList)
    {
        if(list.Count != newList.Count)
        {
            return false;
        }

        for(int i = 0; i < list.Count; i++)
        {
            if(list[i] == ValueType.ANY || newList[i] == ValueType.ANY)
            {
                continue;
            }
            if(list[i] != newList[i])
            {
                return false;
            }
        }
        return true;
    }

    internal static bool ListEquals(this ValueType[] list, List<ValueType> newList)
    {
        if (list.Length != newList.Count)
        {
            return false;
        }

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == ValueType.ANY || newList[i] == ValueType.ANY)
            {
                continue;
            }
            if (list[i] != newList[i])
            {
                return false;
            }
        }
        return true;
    }

    internal static bool ListEquals(this ValueType[] list, ValueType[] newList)
    {
        if (list.Length != newList.Length)
        {
            return false;
        }

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == ValueType.ANY || newList[i] == ValueType.ANY)
            {
                continue;
            }
            if (list[i] != newList[i])
            {
                return false;
            }
        }
        return true;
    }
}

struct Tuple2
{
    internal ValueType first;
    internal ValueType second;

    private Tuple2(ValueType a, ValueType b)
    {
        first = a;
        second = b;
    }

    internal static Tuple2 New(ValueType a, ValueType b)
    {
        return new Tuple2(a, b);
    }

    public override int GetHashCode() => (int)first * 10 + (int)second;
    public override bool Equals(object obj)
    {
        if (!(obj is Tuple2)) return false;
        return GetHashCode() == ((Tuple2)obj).GetHashCode();
    }
}

struct Tuple3
{
    internal ValueType first;
    internal ValueType second;
    internal ValueType third;

    private Tuple3(ValueType a, ValueType b, ValueType c)
    {
        first = a;
        second = b;
        third = c;
    }

    internal static Tuple3 New(ValueType a, ValueType b, ValueType c)
    {
        return new Tuple3(a, b, c);
    }

    public override int GetHashCode() => (int)first * 100 + (int)second * 10 + (int)third;
    public override bool Equals(object obj)
    {
        if (!(obj is Tuple3)) return false;
        return GetHashCode() == ((Tuple3)obj).GetHashCode();
    }
}