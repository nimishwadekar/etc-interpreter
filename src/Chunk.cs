using System.Collections.Generic;

enum OpCode : ushort
{
    CONSTANT, DEFAULT,
    NIL, TRUE, FALSE,
    NEGATE, NOT,
    ADD, SUBTRACT, MULTIPLY, DIVIDE, MODULO,
    EQUAL, LESS, MORE,

    PRINT,
    POP, PUSH, STORE,
    DEF_GLOB, GET_GLOB, SET_GLOB,
    GET_LOC, SET_LOC,
    CHK_TYPE,
    JUMP, JUMP_IF_TRUE, JUMP_IF_FALSE,
    LOOP,

    DEF_FUN, CALL,

    LIB,
    RETURN, 
}

class Chunk
{
    private List<ushort> code;
    public int Count
    {
        get
        {
            return code.Count;
        }
    }
    internal List<Value> values;
    internal List<int> lines;

    internal ushort this[int i]
    {
        get
        {
            return code[i];
        }
        set
        {
            code[i] = value;
        }
    }

    internal Chunk()
    {
        code = new List<ushort>();
        values = new List<Value>();
        lines = new List<int>();
    }

    internal void Write(ushort instruction, int line)
    {
        code.Add(instruction);
        lines.Add(line);
    }

    internal int AddConstant(Value val)
    {
        switch(val.type)
        {
            case ValueType.NUM:
                {
                    NumValue num;
                    double newVal = (val as NumValue).value;
                    for(int i = 0; i < values.Count; i++)
                    {
                        num = values[i] as NumValue;
                        if(num == null)
                        {
                            continue;
                        }
                        if(newVal == num.value)
                        {
                            return i;
                        }
                    }
                    break;
                }
            case ValueType.STR:
                {
                    StrValue str;
                    string newVal = (val as StrValue).value;
                    for (int i = 0; i < values.Count; i++)
                    {
                        str = values[i] as StrValue;
                        if (str == null)
                        {
                            continue;
                        }
                        if (newVal == str.value)
                        {
                            return i;
                        }
                    }
                    break;
                }
        }
        values.Add(val);
        return values.Count - 1;
    }

    internal Value GetConstant(int index) => values[index];

    internal void Free()
    {
        code.Clear();
        values.Clear();
        lines.Clear();
    }
}