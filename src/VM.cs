using System;
using System.Collections.Generic;
using StdLib;
using System.Diagnostics;

class VM
{
    internal enum InterpretResult
    {
        OK, COMPILE_ERROR, RUNTIME_ERROR
    }

    

    Chunk chunk;
    int ip;
    readonly Stack<Value> stack;
    Stack<Environment> environments;
    Environment current, global;
    Value temp;

    readonly Stack<CallFrame> frames;
    int scriptEnd;

    internal static Stopwatch stopwatch;

    NativeDefintions nativeDefintions;

    internal VM()
    {
        ip = 0;
        chunk = new Chunk();
        stack = new Stack<Value>();
        environments = new Stack<Environment>();
        environments.Push(new Environment());
        current = environments.Top();
        global = environments.Top();
        frames = new Stack<CallFrame>();
        stopwatch = new Stopwatch();
        nativeDefintions = new NativeDefintions(this);
        stopwatch.Start();
    }

    internal InterpretResult Interpret(in string source)
    {
        nativeDefintions.MakeNative("general");
        Compiler compiler = new Compiler(source);
        if(!compiler.Compile(chunk))
        {
            return InterpretResult.COMPILE_ERROR;
        }

        InterpretResult result = Run();
        stopwatch.Stop();
        return result;
    }

    

    private InterpretResult Run()
    {
        for (; ; )
        {
            //DEBUG_TRACE_EXECUTION();

            OpCode instruction = (OpCode)ReadWord();
            switch (instruction)
            {
                case OpCode.CONSTANT:
                    {
                        Value val = ReadConstant();
                        stack.Push(val);
                        break;
                    }

                case OpCode.DEFAULT:
                    {
                        stack.Push(Value.Default(frames.Top().function.returnType));
                        break;
                    }

                case OpCode.NIL: stack.Push(Value.NIL); break;

                case OpCode.TRUE: stack.Push(BoolValue.TRUE); break;

                case OpCode.FALSE: stack.Push(BoolValue.FALSE); break;

                case OpCode.NEGATE:
                    {
                        if(stack.Top() as NumValue == null)
                        {
                            return RuntimeError("Operand must be a number.");
                        }
                        stack.Push(-(stack.Pop() as NumValue));
                        break;
                    }

                case OpCode.NOT:
                    {
                        stack.Push(BoolValue.Get(stack.Pop().IsFalse()));
                        break;
                    }

                case OpCode.ADD:
                    {
                        if(stack.Peek(0) as StrValue != null && stack.Peek(1) as StrValue != null)
                        {
                            StrValue bstr = stack.Pop() as StrValue, astr = stack.Pop() as StrValue;
                            stack.Push(astr + bstr);
                            break;
                        }
                        else if(stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be either numbers or strings.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(a + b);
                        break;
                    }

                case OpCode.SUBTRACT:
                    {
                        if (stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be numbers.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(a - b);
                        break;
                    }

                case OpCode.MULTIPLY:
                    {
                        if (stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be numbers.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(a * b);
                        break;
                    }

                case OpCode.DIVIDE:
                    {
                        if (stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be numbers.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(a / b);
                        break;
                    }

                case OpCode.MODULO:
                    {
                        if (stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be numbers.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(a % b);
                        break;
                    }

                case OpCode.EQUAL:
                    {
                        Value b = stack.Pop(), a = stack.Pop();
                        stack.Push(BoolValue.Get(Value.Equal(a, b)));
                        break;
                    }

                case OpCode.LESS:
                    {
                        if (stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be numbers.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(BoolValue.Get(a < b));
                        break;
                    }

                case OpCode.MORE:
                    {
                        if (stack.Peek(0) as NumValue == null || stack.Peek(1) as NumValue == null)
                        {
                            return RuntimeError("Both operands must be numbers.");
                        }
                        NumValue b = stack.Pop() as NumValue, a = stack.Pop() as NumValue;
                        stack.Push(BoolValue.Get(a > b));
                        break;
                    }

                case OpCode.PRINT:
                    {
                        PrintLine(stack.Pop());
                        break;
                    }

                case OpCode.POP:
                    {
                        stack.Pop();
                        break;
                    }

                case OpCode.PUSH:
                    {
                        stack.Push(temp);
                        temp = null;
                        break;
                    }

                case OpCode.STORE:
                    {
                        temp = stack.Pop();
                        break;
                    }

                case OpCode.DEF_GLOB:
                    {
                        //StrValue name = ReadConstant() as StrValue;
                        ushort id = ReadWord();
                        if(current.variables.ContainsKey(id))
                        {
                            if(!(stack.Top() is Function))
                            {
                                return RuntimeError(string.Format("Variable with name '{0}' already exists.", chunk.values[id]));
                            }
                            stack.Pop();
                            break;
                        }
                        current.variables.Add(id, stack.Top());
                        stack.Pop();
                        break;
                    }

                case OpCode.GET_GLOB:
                    {
                        ushort id = ReadWord();
                        //StrValue name = ReadConstant() as StrValue;
                        if (!current.variables.TryGetValue(id, out Value val))
                        {
                            if(!global.variables.TryGetValue(id, out val))
                            {
                                return RuntimeError(string.Format("Undefined variable '{0}'.", chunk.values[id]));
                            }
                        }
                        stack.Push(val);
                        break;
                    }

                case OpCode.SET_GLOB:
                    {
                        //StrValue name = ReadConstant() as StrValue;
                        ushort id = ReadWord();
                        bool local;
                        if (!current.variables.TryGetValue(id, out Value val))
                        {
                            if (!global.variables.TryGetValue(id, out val))
                            {
                                return RuntimeError(string.Format("Undefined variable '{0}'.", chunk.values[id]));
                            }
                            local = false;
                        }
                        else
                        {
                            local = true;
                        }

                        if(val is Object && ((val as Object).objType == ObjectType.FUNCTION || (val as Object).objType == ObjectType.NATIVE))
                        {
                            return RuntimeError("A function identifier cannot be used to store values.");
                        }

                        if(val.type != stack.Top().type)
                        {
                            return RuntimeError(string.Format("Expecting a value of type '{0}'.", Value.TypeStr(val.type)));
                        }

                        if(local)
                        {
                            current.variables[id] = stack.Top();
                        }
                        else
                        {
                            global.variables[id] = stack.Top();
                        }
                        break;
                    }

                case OpCode.GET_LOC:
                    {
                        int index = ReadWord();
                        int offset = 0;
                        if(frames.top != 0)
                        {
                            offset = frames.Top().frameStart + 1;
                        }
                        stack.Push(stack[index + offset]);
                        break;
                    }

                case OpCode.SET_LOC:
                    {
                        int offset = 0;
                        if (frames.top != 0)
                        {
                            offset = frames.Top().frameStart + 1;
                        }
                        ushort index = ReadWord();
                        if(stack[index + offset].type != stack.Top().type)
                        {
                            return RuntimeError(string.Format("Expecting a value of type '{0}'.", Value.TypeStr(stack[index + offset].type)));
                        }
                        stack[index + offset] = stack.Top();
                        break;
                    }

                case OpCode.CHK_TYPE:
                    {
                        ValueType type = (ValueType)ReadWord();
                        if (stack.Top().type != type)
                        {
                            return RuntimeError(string.Format("Expecting a value of type '{0}'.", Value.TypeStr(type)));
                        }
                        break;
                    }

                case OpCode.JUMP:
                    {
                        ushort jump = ReadWord();
                        ip += jump;
                        break;
                    }

                case OpCode.JUMP_IF_TRUE:
                    {
                        ushort jump = ReadWord();
                        if (!stack.Top().IsFalse())
                        {
                            ip += jump;
                        }
                        break;
                    }

                case OpCode.JUMP_IF_FALSE:
                    {
                        ushort jump = ReadWord();
                        if(stack.Top().IsFalse())
                        {
                            ip += jump;
                        }
                        break;
                    }

                case OpCode.LOOP:
                    {
                        ushort jump = ReadWord();
                        ip -= jump;

                        break;
                    }

                case OpCode.DEF_FUN:
                    {
                        Function newFunction = stack.Top() as Function;
                        int location = (int)((ReadConstant() as NumValue).value);
                        newFunction.location = location;
                        break;
                    }

                case OpCode.CALL:
                    {
                        int args = ReadWord();
                        ValueType[] argTypes = new ValueType[args];
                        for(int i = 0; i < args; i++)
                        {
                            argTypes[i] = stack.Peek(args - 1 - i).type;
                        }
                        if(!CallValue(stack.Peek(args), argTypes))
                        {
                            return InterpretResult.RUNTIME_ERROR;
                        }
                        break;
                    }

                case OpCode.LIB:
                    {
                        string lib = (ReadConstant() as StrValue).value;
                        if(nativeDefintions.MakeNative(lib) == InterpretResult.RUNTIME_ERROR)
                        {
                            return InterpretResult.RUNTIME_ERROR;
                        }
                        break;
                    }

                case OpCode.RETURN:
                    {
                        if (frames.top == 0)
                        {
                            return InterpretResult.OK;
                        }

                        Function function = frames.Top().function;
                        ValueType returnType = function.returnType;
                        ip = frames.Top().returnLocation;

                        Value val = stack.Pop();
                        for (int i = stack.top; i > frames.Top().frameStart; i--)
                        {
                            stack.Pop();
                        }
                        frames.Pop();

                        if(returnType == ValueType.NIL && val.type != ValueType.NIL)
                        {
                            return RuntimeError("Cannot return any value except 'nil'.");
                        }
                        else if(val.type != returnType)
                        {
                            return RuntimeError(string.Format("'{0}'can only return a value of type '{1}'.", function, returnType.Str()));
                        }
                        environments.Pop();
                        current = environments.Top();

                        stack.Push(val);
                        break;
                    }
            }
            if(frames.top == 0)
            {
                scriptEnd = stack.top - 1;
            }
        }
    }

    private bool CallValue(Value callee, ValueType[] args)
    {
        if(callee is Object)
        {
            switch((callee as Object).objType)
            {
                case ObjectType.FUNCTION:
                    return Call(callee as Function, args);

                case ObjectType.NATIVE:
                    return NativeCall(callee as Native, args);
            }
        }

        RuntimeError("Invalid call.");
        return false;
    }

    private bool Call(Function function, ValueType[] args)
    {
        List<Function> overloads = function.GetOverloads();
        Function actual = null;
        if(!args.ListEquals(function.parameters))
        {
            foreach(Function func in overloads)
            {
                if(args.ListEquals(func.parameters))
                {
                    actual = func;
                }
            }
            if(actual == null)
            {
                RuntimeError(string.Format("No overload of {0} accepts {1} argument{2}.", function.ToPrint(), args.Length, args.Length == 1 ? "" : "s"));
                return false;
            }
        }
        else
        {
            actual = function;
        }

        environments.Push(new Environment());
        current = environments.Top();
        CallFrame frame = new CallFrame(actual, ip, stack.top - 1 - args.Length, current);
        if(frames.top == 0)
        {
            scriptEnd = stack.top - 1;
        }
        frames.Push(frame);
        ip = actual.location;
        return true;
    }

    private bool NativeCall(Native native, ValueType[] args)
    {
        try
        {
            Value result = null;
            switch (args.Length)
            {
                case 0:
                    {
                        var fun = native.GetOverload();
                        if (fun == null) goto default;
                        result = fun();
                        break;
                    }
                case 1:
                    {
                        var fun = native.GetOverload(args[0]);
                        if (fun == null) goto default;
                        result = fun(stack.Top()); 
                        break;
                    }
                case 2:
                    {
                        var fun = native.GetOverload(args[0], args[1]);
                        if (fun == null) goto default;
                        result = fun(stack.Peek(1), stack.Peek(0));
                        break;
                    }
                case 3:
                    {
                        var fun = native.GetOverload(args[0], args[1], args[2]);
                        if (fun == null) goto default;
                        result = fun(stack.Peek(2), stack.Peek(1), stack.Peek(0));
                        break;
                    }

                default:
                    RuntimeError(string.Format("No such overload of {0} found.", native.ToPrint()));
                    return false;
            }

            stack.top -= args.Length + 1;
            stack.Push(result);
            return true;
        }
        catch (Exception e)
        {
            RuntimeError(e.Message);
            return false;
        }
    }

    internal void DefineNative(string name, Func<Value> function) => GetNative(name).AddOverload(function);
    internal void DefineNative(string name, Func<Value, Value> function, ValueType arg) => GetNative(name).AddOverload(function, arg);
    internal void DefineNative(string name, Func<Value, Value, Value> function, ValueType arg1, ValueType arg2) => GetNative(name).AddOverload(function, arg1, arg2);
    internal void DefineNative(string name, Func<Value, Value, Value, Value> function, ValueType arg1, ValueType arg2, ValueType arg3) => GetNative(name).AddOverload(function, arg1, arg2, arg3);

    private Native GetNative(string name)
    {
        ushort id = (ushort)chunk.AddConstant(StrValue.NewString(name));

        if (global.variables.TryGetValue(id, out Value val)) return val as Native;

        Native nat = new Native(id, name);
        global.variables[id] = nat;
        return nat;
    }

    internal InterpretResult RuntimeError(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.Write("[line {0}] in ", chunk.lines[ip - 1]);
        for(int i = frames.top - 1; i >= 0; i--)
        {
            CallFrame frame = frames[i];
            Function function = frame.function;
            int instruction = frame.returnLocation - 1;
            Console.Error.WriteLine(function.name + "()");
            Console.Error.Write("[line {0}] in ", chunk.lines[instruction]);
        }
        Console.Error.WriteLine("script.");
        return InterpretResult.RUNTIME_ERROR;
    }

    private void Print(object obj)
    {
        if(obj is Function)
        {
            Console.Write((obj as Function));
            return;
        }

        Console.Write(obj);
    }

    private void PrintLine(object obj)
    {
        Print(obj);
        Console.WriteLine();
    }

    private ushort ReadWord()
    {
        return chunk[ip++];
    }

    private uint ReadLongWord()
    {
        return (uint)(chunk[ip++] * (ushort.MaxValue + 1) + chunk[ip++]);
    }

    private Value ReadConstant()
    {
        return chunk.values[ReadWord()];
    }

    private void DEBUG_TRACE_EXECUTION()
    {
        Console.WriteLine();
        for(int i = 0; i < stack.top; i++)
        {
            Console.Write("[ {0} ]", stack.array[i]);
        }
        Console.WriteLine();
        ETC.debugger.DisassembleInstruction(chunk, ip);
    }
}

class CallFrame
{
    internal Function function;
    internal int returnLocation, frameStart;

    internal CallFrame(Function function, int returnLocation, int frameStart, Environment environment)
    {
        this.function = function;
        this.returnLocation = returnLocation;
        this.frameStart = frameStart;
    }
}

class Environment
{
    internal Dictionary<ushort, Value> variables;

    internal Environment()
    {
        variables = new Dictionary<ushort, Value>();
    }
}

class Stack<T>
{
    internal T[] array;
    internal int top;

    internal T this[int i]
    {
        get
        {
            return array[i];
        }
        set
        {
            array[i] = value;
        }
    }

    internal Stack()
    {
        top = 0;
        array = new T[ushort.MaxValue];
    }

    internal void Push(T t)
    {
        array[top] = t;
        top++;
    }

    internal T Pop()
    {
        top--;
        return array[top];
    }

    internal T Top() => array[top - 1];

    internal T Peek(int distance) => array[top - 1 - distance];
}