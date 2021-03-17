namespace StdLib
{
    static class String
    {
        internal static Value Compare(Value[] vals) => NumValue.NewNum((vals[0] as StrValue).value.CompareTo((vals[1] as StrValue).value));
        internal static Value Contains(Value[] vals) => BoolValue.Get((vals[0] as StrValue).value.Contains((vals[1] as StrValue).value));
        internal static Value EndsWith(Value[] vals) => BoolValue.Get((vals[0] as StrValue).value.EndsWith((vals[1] as StrValue).value));
        internal static Value IndexOf(Value[] vals) => NumValue.NewNum((vals[0] as StrValue).value.IndexOf((vals[1] as StrValue).value));
        internal static Value Length(Value[] vals) => NumValue.NewNum((vals[0] as StrValue).value.Length);
        internal static Value Replace(Value[] vals) => StrValue.NewString((vals[0] as StrValue).value.Replace((vals[1] as StrValue).value, (vals[2] as StrValue).value));
        internal static Value StartsWith(Value[] vals) => BoolValue.Get((vals[0] as StrValue).value.StartsWith((vals[1] as StrValue).value));
        internal static Value Substring(Value[] vals) => StrValue.NewString((vals[0] as StrValue).value.Substring((int)(vals[1] as NumValue).value, (int)(vals[2] as NumValue).value));
        internal static Value ToLower(Value[] vals) => StrValue.NewString((vals[0] as StrValue).value.ToLower());
        internal static Value ToUpper(Value[] vals) => StrValue.NewString((vals[0] as StrValue).value.ToUpper());
        internal static Value Trim(Value[] vals) => StrValue.NewString((vals[0] as StrValue).value.Trim());
    }
}
