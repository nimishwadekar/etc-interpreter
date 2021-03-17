namespace StdLib
{
    static class Math
    {
        internal static Value Pow(Value[] vals) => NumValue.NewNum(System.Math.Pow((vals[0] as NumValue).value, (vals[1] as NumValue).value));
        internal static Value Sqrt(Value[] vals) => NumValue.NewNum(System.Math.Sqrt((vals[0] as NumValue).value));
        internal static Value Cbrt(Value[] vals) => NumValue.NewNum(System.Math.Pow((vals[0] as NumValue).value, 1.0/3));
        internal static Value Abs(Value[] vals) => NumValue.NewNum(System.Math.Abs((vals[0] as NumValue).value));
        internal static Value Round(Value[] vals) => NumValue.NewNum(System.Math.Round((vals[0] as NumValue).value));
        internal static Value Ceil(Value[] vals) => NumValue.NewNum(System.Math.Ceiling((vals[0] as NumValue).value));
        internal static Value Floor(Value val) => NumValue.NewNum(System.Math.Floor((val as NumValue).value));
        internal static Value Exp(Value[] vals) => NumValue.NewNum(System.Math.Exp((vals[0] as NumValue).value));
        internal static Value Min(Value[] vals)// => NumValue.NewNum(System.Math.Min((vals[0] as NumValue).value, (vals[1] as NumValue).value));
        {
            NumValue min = vals[0] as NumValue;
            foreach(Value val in vals)
            {
                if(min > (val as NumValue))
                {
                    min = val as NumValue;
                }
            }
            return min;
        }
        internal static Value Max(Value[] vals) => NumValue.NewNum(System.Math.Max((vals[0] as NumValue).value, (vals[1] as NumValue).value));
        internal static Value Trunc(Value[] vals) => NumValue.NewNum(System.Math.Truncate((vals[0] as NumValue).value));

        //Trigonometry
        internal static Value Sin(Value[] vals) => NumValue.NewNum(System.Math.Sin((vals[0] as NumValue).value * System.Math.PI / 180));
        internal static Value Cos(Value[] vals) => NumValue.NewNum(System.Math.Cos((vals[0] as NumValue).value * System.Math.PI / 180));
        internal static Value Tan(Value[] vals) => NumValue.NewNum(System.Math.Tan((vals[0] as NumValue).value * System.Math.PI / 180));
        internal static Value Cosec(Value[] vals) => NumValue.NewNum(1 / System.Math.Sin((vals[0] as NumValue).value * System.Math.PI / 180));
        internal static Value Sec(Value[] vals) => NumValue.NewNum(1 / System.Math.Cos((vals[0] as NumValue).value * System.Math.PI / 180));
        internal static Value Cot(Value[] vals) => NumValue.NewNum(1 / System.Math.Tan((vals[0] as NumValue).value * System.Math.PI / 180));
        internal static Value Arcsin(Value[] vals) => NumValue.NewNum(System.Math.Asin((vals[0] as NumValue).value) * 180 / System.Math.PI);
        internal static Value Arccos(Value[] vals) => NumValue.NewNum(System.Math.Acos((vals[0] as NumValue).value) * 180 / System.Math.PI);
        internal static Value Arctan(Value[] vals) => NumValue.NewNum(System.Math.Atan((vals[0] as NumValue).value) * 180 / System.Math.PI);
        internal static Value Arccosec(Value[] vals) => NumValue.NewNum(System.Math.Asin(1 / (vals[0] as NumValue).value) * 180 / System.Math.PI);
        internal static Value Arcsec(Value[] vals) => NumValue.NewNum(System.Math.Acos(1 / (vals[0] as NumValue).value) * 180 / System.Math.PI);
        internal static Value Arccot(Value[] vals) => NumValue.NewNum(System.Math.Atan(1 / (vals[0] as NumValue).value) * 180 / System.Math.PI);
    }
}
