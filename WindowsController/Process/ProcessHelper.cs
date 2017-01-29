using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

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

        public static List<ProcessModule> GetProcessModules(int processId)
        {
            var moduleList = new List<ProcessModule>();

            WinApi.SetLastError(0);
            var hModuleSnap = WinApi.CreateToolhelp32Snapshot(SnapshotFlags.Module, (UInt32)processId);
            var lastError = WinApi.GetLastError();
            try
            {
                if ((int)hModuleSnap == WinApi.InvalidHandleValue) return null;

                var me32 = new ModuleEntry32();
                me32.dwSize = (uint)Marshal.SizeOf(me32);
                if (!WinApi.Module32First(hModuleSnap, ref me32))
                    return null;

                do
                {
                    moduleList.Add(new ProcessModule
                    {
                        Name = me32.szModule,
                        Path = me32.szExePath,
                        BaseAddr = (ulong)me32.hModule.ToInt64(),
                        Size = me32.modBaseSize
                    });
                }
                while (WinApi.Module32Next(hModuleSnap, ref me32));
            }
            finally
            {
                WinApi.CloseHandle(hModuleSnap);
            }

            return moduleList;
        }
    }
}