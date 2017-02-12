using System;
using System.Runtime.InteropServices;
using System.Text;
using ApiHooker.Utils;

namespace ApiHooker
{
    public class ProcessManager
    {
        protected ProcessManager() { }

        public ProcessInformation ProcessInformation { get; protected set; }
        public System.Diagnostics.Process Process { get; protected set; }
        public RemoteMemoryManager MemoryManager { get; protected set; }

        public string InjectedDllPath { get; protected set; }
        public uint InjectedDllBaseAddr { get; protected set; }

        protected ProcessManager(ProcessInformation procInfo)
        {
            ProcessInformation = procInfo;

            Process = System.Diagnostics.Process.GetProcessById(ProcessInformation.dwProcessId);
            MemoryManager = new RemoteMemoryManager(ProcessInformation.hProcess);
        }

        public static ProcessManager LaunchSuspended(string exePath, string arguments = "", string workingDirectory = null)
        {
            exePath = FileUtils.GetFullPath(exePath);
            var commandLine = exePath + (String.IsNullOrEmpty(arguments) ? "" : " " + arguments);
            workingDirectory = workingDirectory ?? Environment.CurrentDirectory;

            ProcessInformation procInfo;
            var flags = ProcessCreationFlags.CreateSuspended | ProcessCreationFlags.CreateNewConsole;
            if (!WinApi.CreateProcess(exePath, commandLine, null, null, false, flags, IntPtr.Zero, workingDirectory, new StartupInfo { wShowWindow = 1 }, out procInfo))
                throw new Exception("Could not create process!");

            return new ProcessManager(procInfo);
        }

        public uint CallRemoteFunction(long funcAddr, IntPtr data, bool wait = true)
        {
            uint threadId;
            var newThreadHandle = WinApi.CreateRemoteThread(ProcessInformation.hProcess, IntPtr.Zero, 0, new IntPtr(funcAddr), data, 0, out threadId);

            if (!wait)
                return 0;

            WinApi.WaitForSingleObject(newThreadHandle, (uint)WaitForSingleObjectTimeout.Infinite);

            uint returnValue;
            if (!WinApi.GetExitCodeThread(newThreadHandle, out returnValue))
                throw new Exception("Could not get return value after calling remote function!");
            return returnValue;
        }

        static long GetProcAddr(string libraryPath, string funcName, bool absolute = false)
        {
            var baseAddr = WinApi.LoadLibrary(libraryPath);
            var absAddr = WinApi.GetProcAddress(baseAddr, funcName).ToInt64();
            return absolute ? absAddr : absAddr - baseAddr.ToInt64();
        }

        public void InjectDll(string injectionDllPath)
        {
            InjectedDllPath = injectionDllPath;
            var hLoadLibrary = GetProcAddr("kernel32.dll", "LoadLibraryA", true);
            InjectedDllBaseAddr = CallRemoteFunction(hLoadLibrary, MemoryManager.Copy(injectionDllPath));
        }

        [StructLayout(LayoutKind.Sequential)]
        struct InitParams
        {
            public UInt32 tcpPort;
        }

        public void InjectHookerLib(int tcpPort)
        {
            if (ProcessHelper.Is64Bit(Process.Handle))
                throw new Exception("Injecting into 64-bit processes is not supported yet!");

            var injDllPath = AppDomain.CurrentDomain.BaseDirectory + "ApiHookerInject_x86.dll";
            InjectDll(injDllPath);
            var initAddr = GetProcAddr(injDllPath, "Init");
            CallRemoteFunction(InjectedDllBaseAddr + initAddr, MemoryManager.Copy(new InitParams { tcpPort = (uint)tcpPort }), false);
        }

        public void ResumeMainThread()
        {
            WinApi.ResumeThread(ProcessInformation.hThread);
        }
    }
}