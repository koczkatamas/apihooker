using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiHooker.Model;
using ApiHooker.Model.StructSerialization;

namespace ApiHooker
{
    public static class ApiDefinitions
    {
        public static ApiMethod SetConsoleTitleA = new ApiMethod
        {
            DllName = "kernel32.dll",
            MethodName = "SetConsoleTitleA",
            Arguments =
            {
                new FieldDescriptor { Name = "lpConsoleTitle", Type = FieldType.NullTerminatedAnsiString, PointerLevel = 1 },
            }
        };

        public static ApiMethod SetConsoleWindowInfo = new ApiMethod
        {
            DllName = "kernel32.dll",
            MethodName = "SetConsoleWindowInfo",
            Arguments =
            {
                new FieldDescriptor { Name = "hConsoleOutput", Type = FieldType.ByteArray, Length = 4 },
                new FieldDescriptor { Name = "bAbsolute", Type = FieldType.ByteArray, Length = 4 },
                new FieldDescriptor
                {
                    Name = "lpConsoleWindow", Type = FieldType.Struct, PointerLevel = 1, Fields =
                    {
                        new FieldDescriptor { Name = "Left", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Top", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Right", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Bottom", Type = FieldType.ByteArray, Length = 2 },
                    }
                },
            }
        };

        public static FieldDescriptor StructCoord = new FieldDescriptor
        {
            Type = FieldType.Struct,
            Fields =
            {
                new FieldDescriptor { Name = "X", Type = FieldType.ByteArray, Length = 2 },
                new FieldDescriptor { Name = "Y", Type = FieldType.ByteArray, Length = 2 },
            }
        };

        public static ApiMethod SetConsoleScreenBufferSize = new ApiMethod
        {
            DllName = "kernel32.dll",
            MethodName = "SetConsoleScreenBufferSize",
            Arguments =
            {
                new FieldDescriptor { Name = "hConsoleOutput", Type = FieldType.ByteArray, Length = 4 },
                new FieldDescriptor(StructCoord) { Name = "dwSize" }
            }
        };

        public static ApiMethod WriteConsoleOutputA = new ApiMethod
        {
            DllName = "kernel32.dll",
            MethodName = "WriteConsoleOutputA",
            Arguments =
            {
                new FieldDescriptor { Name = "hConsoleOutput", Type = FieldType.ByteArray, Length = 4 },
                new FieldDescriptor
                {
                    Name = "lpBuffer", Type = FieldType.Struct, PointerLevel = 1, Fields =
                    {
                        new FieldDescriptor { Name = "Char", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Attributes", Type = FieldType.ByteArray, Length = 2 },
                    }
                },
                new FieldDescriptor(StructCoord) { Name = "dwBufferSize" },
                new FieldDescriptor(StructCoord) { Name = "dwBufferCoord" },
                new FieldDescriptor
                {
                    Name = "lpWriteRegion", Type = FieldType.Struct, PointerLevel = 1, Fields =
                    {
                        new FieldDescriptor { Name = "Left", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Top", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Right", Type = FieldType.ByteArray, Length = 2 },
                        new FieldDescriptor { Name = "Bottom", Type = FieldType.ByteArray, Length = 2 },
                    }
                },
            }
        };

        public static ApiMethod GetConsoleTitleA = new ApiMethod
        {
            DllName = "kernel32.dll",
            MethodName = "GetConsoleTitleA",
            Arguments =
            {
                new FieldDescriptor { Name = "lpConsoleTitle", Type = FieldType.NullTerminatedAnsiString },
                new FieldDescriptor { Name = "nSize", Type = FieldType.ByteArray, Length = 4 },
            }
        };
    }
}
