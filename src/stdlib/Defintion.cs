namespace StdLib
{
    class NativeDefintions
    {
        internal enum Library
        {
            GENERAL, MATH, STRING,

            nil
        }

        readonly VM vm;

        internal NativeDefintions(VM vm)
        {
            this.vm = vm;
        }

        private void DefineGeneral()
        {
            vm.DefineNative("clock", General.Clock);
            vm.DefineNative("input", General.Input);
            vm.DefineNative("number", General.Number, ValueType.STR);
            //DefineNative("print", new ValueType[] { ValueType.ANY }, General.Print);
            vm.DefineNative("printf", General.Printf, ValueType.STR, ValueType.NUM);
            vm.DefineNative("println", General.Println);
            vm.DefineNative("println", General.Println, ValueType.BOOL);
            vm.DefineNative("println", General.Println, ValueType.NUM);
            vm.DefineNative("println", General.Println, ValueType.STR);
        }

        private void DefineMath()
        {
            /*vm.DefineNative("pow", new ValueType[] { ValueType.NUM, ValueType.NUM }, StdLib.Math.Pow);
            vm.DefineNative("sqrt", new ValueType[] { ValueType.NUM }, StdLib.Math.Sqrt);
            vm.DefineNative("cbrt", new ValueType[] { ValueType.NUM }, StdLib.Math.Cbrt);
            vm.DefineNative("abs", new ValueType[] { ValueType.NUM }, StdLib.Math.Abs);
            vm.DefineNative("round", new ValueType[] { ValueType.NUM }, StdLib.Math.Round);
            vm.DefineNative("ceil", new ValueType[] { ValueType.NUM }, StdLib.Math.Ceil);*/
            vm.DefineNative("floor", Math.Floor, ValueType.NUM);
            /*vm.DefineNative("exp", new ValueType[] { ValueType.NUM }, StdLib.Math.Exp);
            vm.DefineNative("min", new ValueType[] { ValueType.NUM, ValueType.NUM }, StdLib.Math.Min);
            vm.DefineNative("min", new ValueType[] { ValueType.NUM, ValueType.NUM, ValueType.NUM }, StdLib.Math.Min);
            vm.DefineNative("max", new ValueType[] { ValueType.NUM, ValueType.NUM }, StdLib.Math.Max);
            vm.DefineNative("trunc", new ValueType[] { ValueType.NUM }, StdLib.Math.Trunc);

            //Trigonometry
            vm.DefineNative("sin", new ValueType[] { ValueType.NUM }, StdLib.Math.Sin);
            vm.DefineNative("cos", new ValueType[] { ValueType.NUM }, StdLib.Math.Cos);
            vm.DefineNative("tan", new ValueType[] { ValueType.NUM }, StdLib.Math.Tan);
            vm.DefineNative("cosec", new ValueType[] { ValueType.NUM }, StdLib.Math.Cosec);
            vm.DefineNative("sec", new ValueType[] { ValueType.NUM }, StdLib.Math.Sec);
            vm.DefineNative("cot", new ValueType[] { ValueType.NUM }, StdLib.Math.Cot);
            vm.DefineNative("arcsin", new ValueType[] { ValueType.NUM }, StdLib.Math.Arcsin);
            vm.DefineNative("arccos", new ValueType[] { ValueType.NUM }, StdLib.Math.Arccos);
            vm.DefineNative("arctan", new ValueType[] { ValueType.NUM }, StdLib.Math.Arctan);
            vm.DefineNative("arccosec", new ValueType[] { ValueType.NUM }, StdLib.Math.Arccosec);
            vm.DefineNative("arcsec", new ValueType[] { ValueType.NUM }, StdLib.Math.Arcsec);
            vm.DefineNative("arccot", new ValueType[] { ValueType.NUM }, StdLib.Math.Arccot);*/
        }

        private void DefineString()
        {
            /*vm.DefineNative("compare", new ValueType[] { ValueType.STR, ValueType.STR }, StdLib.String.Compare);
            vm.DefineNative("contains", new ValueType[] { ValueType.STR, ValueType.STR }, StdLib.String.Contains);
            vm.DefineNative("endsWith", new ValueType[] { ValueType.STR, ValueType.STR }, StdLib.String.EndsWith);
            vm.DefineNative("indexOf", new ValueType[] { ValueType.STR, ValueType.STR }, StdLib.String.IndexOf);
            vm.DefineNative("length", new ValueType[] { ValueType.STR }, StdLib.String.Length);
            vm.DefineNative("replace", new ValueType[] { ValueType.STR, ValueType.STR, ValueType.STR }, StdLib.String.Replace);
            vm.DefineNative("startsWith", new ValueType[] { ValueType.STR, ValueType.STR }, StdLib.String.StartsWith);
            vm.DefineNative("substring", new ValueType[] { ValueType.STR, ValueType.NUM, ValueType.NUM }, StdLib.String.Substring);
            vm.DefineNative("toLower", new ValueType[] { ValueType.STR }, StdLib.String.ToLower);
            vm.DefineNative("toUpper", new ValueType[] { ValueType.STR }, StdLib.String.ToUpper);
            vm.DefineNative("trim", new ValueType[] { ValueType.STR }, StdLib.String.Trim);*/
        }

        internal VM.InterpretResult MakeNative(string name)
        {
            Library library = Library.nil;
            switch (name[0])
            {
                case 'g': library = CheckLibrary(name, 1, 6, "eneral", Library.GENERAL); break;
                case 'm': library = CheckLibrary(name, 1, 3, "ath", Library.MATH); break;
                case 's': library = CheckLibrary(name, 1, 5, "tring", Library.STRING); break;
            }

            if (library == Library.nil)
            {
                return vm.RuntimeError(string.Format("Library '{0}' does not exist.", name));
            }

            switch (library)
            {
                case Library.GENERAL: DefineGeneral(); break;
                case Library.MATH: DefineMath(); break;
                case Library.STRING: DefineString(); break;
            }
            return VM.InterpretResult.OK;
        }

        private Library CheckLibrary(string libName, int start, int length, string rest, Library lib)
        {
            if (libName.Length == start + length && libName.Substring(start, length) == rest)
            {
                return lib;
            }
            return Library.nil;
        }
    }
}
