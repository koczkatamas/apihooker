using System;
using System.ComponentModel;

namespace ApiHooker
{
    public static class ProcessHelper
    {
        public static bool Is64Bit(IntPtr processHandle)
        {
            if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "x86")
                return false;

            bool isWow64;
            if (!WinApi.IsWow64Process(processHandle, out isWow64))
                throw new Win32Exception();
            return !isWow64;
        }
    }
}