using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace ApiHooker.VisualStudio
{
    public static class VsDebuggerHelper
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        private static object GetDTE(int processId)
        {
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            var names = new List<string>();

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        if (runningObjectMoniker != null)
                        {
                            runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    names.Add(name);

                    if (!string.IsNullOrEmpty(name) && name.StartsWith("!VisualStudio.DTE.") && name.EndsWith(processId.ToString()))
                    {
                        Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return runningObject;
        }

        public static bool AttachToProcess(int processId, string vsTitle = null)
        {
            var devenv = Process.GetProcessesByName("devenv").FirstOrDefault(x => vsTitle == null || x.MainWindowTitle.Contains(vsTitle));
            if (devenv == null) return false;

            dynamic dte = GetDTE(devenv.Id);
            if (dte == null) return false;

            try
            {
                foreach(dynamic p in dte.Debugger.LocalProcesses)
                    if (p.ProcessID == processId)
                    {
                        p.Attach();
                        return true;
                    }

                return false;
            }
            finally
            {
                Marshal.ReleaseComObject(dte);
            }
        }

    }
}
