using System;

namespace StdLib
{
    static class General
    {
        internal static NumValue Clock() => NumValue.NewNum(VM.stopwatch.Elapsed.TotalSeconds);
        internal static StrValue Input() => StrValue.NewString(Console.ReadLine());
        internal static NumValue Number(Value val)
        {
            switch (val.type)
            {
                case ValueType.NUM: return val as NumValue;
                case ValueType.BOOL: return NumValue.NewNum((val as BoolValue).value ? 1 : 0);
                case ValueType.STR:
                    try
                    {
                        return NumValue.NewNum(double.Parse(((val) as StrValue).value));
                    }
                    catch (FormatException)
                    {
                        break;
                    }
            }
            throw new Exception("Value does not represent a numeric value.");
        }

        /*internal static Value Print(Value[] vals)
        {
            Console.Write(vals[0]);
            return Value.NIL;
        }*/

        internal static Value Printf(Value val1, Value val2) { Console.Write((val1 as StrValue).value, val2); return Value.NIL; }
        internal static Value Println() { Console.WriteLine(); return Value.NIL; }
        internal static Value Println(Value val) { Console.WriteLine(val); return Value.NIL; }
    }
}
