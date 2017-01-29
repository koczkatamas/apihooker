using System;

namespace ApiHooker
{
    public class ProcessManager
    {
        protected ProcessManager() { }

        public ProcessInformation ProcessInformation { get; protected set; }
        public System.Diagnostics.Process Process { get; protected set; }
        public RemoteMemoryManager MemoryManager { get; protected set; }

        public string InjectedDllPath { get; protected set; }
        public uint InjectedThreadId { get; protected set; }
        public IntPtr InjectedThreadHandle { get; protected set; }

        protected ProcessManager(ProcessInformation procInfo)
        {
            ProcessInformation = procInfo;

            Process = System.Diagnostics.Process.GetProcessById(ProcessInformation.dwProcessId);
            MemoryManager = new RemoteMemoryManager(ProcessInformation.hProcess);
        }

        public static ProcessManager LaunchSuspended(string exePath, string arguments = "", string workingDirectory = null)
        {
            var commandLine = exePath + (String.IsNullOrEmpty(arguments) ? "" : " " + arguments);
            workingDirectory = workingDirectory ?? Environment.CurrentDirectory;

            ProcessInformation procInfo;
            var flags = ProcessCreationFlags.CreateSuspended | ProcessCreationFlags.CreateNewConsole;
            if (!WinApi.CreateProcess(exePath, commandLine, null, null, false, flags, IntPtr.Zero, workingDirectory, new StartupInfo { wShowWindow = 1 }, out procInfo))
                throw new Exception("Could not create process!");

            return new ProcessManager(procInfo);
        }

        public void Inject(string injectionDllPath)
        {
            InjectedDllPath = injectionDllPath;

            var hKernel32 = WinApi.LoadLibrary("kernel32.dll");
            var hLoadLibrary = WinApi.GetProcAddress(hKernel32, "LoadLibraryA");

            uint threadId;
            InjectedThreadHandle = WinApi.CreateRemoteThread(ProcessInformation.hProcess, IntPtr.Zero, 0, hLoadLibrary, MemoryManager.Copy(InjectedDllPath), 0, out threadId);
            InjectedThreadId = threadId;
            WinApi.WaitForSingleObject(InjectedThreadHandle, (uint)WaitForSingleObjectTimeout.Infinite);

            uint injectionDllBaseAddr;
            if (!WinApi.GetExitCodeThread(InjectedThreadHandle, out injectionDllBaseAddr))
                throw new Exception("Could not get Injection DLL base address!");
        }

        public void Inject()
        {
            if (ProcessHelper.Is64Bit(Process.Handle))
                throw new Exception("Injecting into 64-bit processes is not supported yet!");

            Inject(AppDomain.CurrentDomain.BaseDirectory + "ApiHookerInject_x86.dll");
        }

        public void ResumeMainThread()
        {
            WinApi.ResumeThread(ProcessInformation.hThread);
        }
    }
}