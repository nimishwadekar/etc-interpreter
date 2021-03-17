using System;

class Debugger
{
    internal void DisassembleChunk(Chunk chunk, string name)
    {
        Console.WriteLine("=== {0} ===", name);
        for (int offset = 0; offset < chunk.Count;)
        {
            offset = DisassembleInstruction(chunk, offset);
        }
    }

    internal int DisassembleInstruction(Chunk chunk, int offset)
    {
        Console.Write("{0,4:D4}  ", offset);
        if (offset > 0 && chunk.lines[offset] == chunk.lines[offset - 1])
        {
            Console.Write("   |  ");
        }
        else
        {
            Console.Write("{0,4}  ", chunk.lines[offset]);
        }

        OpCode instruction = (OpCode)chunk[offset];
        switch (instruction)
        {
            case OpCode.CONSTANT:
                return ConstantInstruction("CONSTANT", chunk, offset);

            case OpCode.DEFAULT:
                return SimpleInstruction("DEFAULT", offset);

            case OpCode.NIL:
                return SimpleInstruction("NIL", offset);

            case OpCode.TRUE:
                return SimpleInstruction("TRUE", offset);

            case OpCode.FALSE:
                return SimpleInstruction("FALSE", offset);

            case OpCode.NEGATE:
                return SimpleInstruction("NEGATE",  offset);

            case OpCode.NOT:
                return SimpleInstruction("NOT", offset);

            case OpCode.ADD:
                return SimpleInstruction("ADD", offset);

            case OpCode.SUBTRACT:
                return SimpleInstruction("SUBTRACT", offset);

            case OpCode.MULTIPLY:
                return SimpleInstruction("MULTIPLY", offset);

            case OpCode.DIVIDE:
                return SimpleInstruction("DIVIDE", offset);

            case OpCode.MODULO:
                return SimpleInstruction("MODULO", offset);

            case OpCode.EQUAL:
                return SimpleInstruction("EQUAL", offset);

            case OpCode.LESS:
                return SimpleInstruction("LESS", offset);

            case OpCode.MORE:
                return SimpleInstruction("MORE", offset);

            case OpCode.PRINT:
                return SimpleInstruction("PRINT", offset);

            case OpCode.POP:
                return SimpleInstruction("POP", offset);

            case OpCode.PUSH:
                return SimpleInstruction("PUSH", offset);

            case OpCode.STORE:
                return SimpleInstruction("STORE", offset);

            case OpCode.DEF_GLOB:
                return ConstantInstruction("DEF_GLOB", chunk, offset);

            case OpCode.GET_GLOB:
                return ConstantInstruction("GET_GLOB", chunk, offset);

            case OpCode.SET_GLOB:
                return ConstantInstruction("SET_GLOB", chunk, offset);

            case OpCode.GET_LOC:
                return WordInstruction("GET_LOC", chunk, offset);

            case OpCode.SET_LOC:
                return WordInstruction("SET_LOC", chunk, offset);

            case OpCode.CHK_TYPE:
                return SimpleInstruction("CHK_TYPE", offset) + 1; //Not showing the operand (value type).

            case OpCode.JUMP:
                return WordInstruction("JUMP", chunk, offset);

            case OpCode.JUMP_IF_TRUE:
                return WordInstruction("JUMP_IF_TRUE", chunk, offset);

            case OpCode.JUMP_IF_FALSE:
                return WordInstruction("JUMP_IF_FALSE", chunk, offset);

            case OpCode.LOOP:
                return WordInstruction("LOOP", chunk, offset);

            case OpCode.DEF_FUN:
                return ConstantInstruction("DEF_FUN", chunk, offset);

            case OpCode.CALL:
                return WordInstruction("CALL", chunk, offset);

            case OpCode.LIB:
                return ConstantInstruction("LIB", chunk, offset);

            case OpCode.RETURN:
                return SimpleInstruction("RETURN", offset);

            default:
                Console.WriteLine("Unknown opcode {0}", (ushort)instruction);
                return offset + 1;
        }
    }

    private int SimpleInstruction(string name, int offset)
    {
        Console.WriteLine(name);
        return offset + 1;
    }

    private int ConstantInstruction(string name, Chunk chunk, int offset)
    {
        ushort constantIndex = chunk[offset + 1];
        Console.WriteLine("{0,-16} {1,4}   '{2}'", name, constantIndex, chunk.values[constantIndex]);
        return offset + 2;
    }

    private int WordInstruction(string name, Chunk chunk, int offset)
    {
        ushort slot = chunk[offset + 1];
        Console.WriteLine("{0,-16} {1,4}", name, slot);
        return offset + 2;
    }
}