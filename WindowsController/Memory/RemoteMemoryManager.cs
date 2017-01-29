using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ApiHooker
{
    public class RemoteMemoryManager
    {
        public IntPtr HProcess { get; protected set; }
        public int AllocationRegionSize { get; set; }
        public List<Region> Regions { get; protected set; } = new List<Region>();
        public Region CurrentRegion { get; protected set; }

        public class Region
        {
            public IntPtr BaseAddress { get; internal set; }
            public IntPtr NextFree { get; internal set; }
            public IntPtr EndPointer { get; internal set; }
            public int Size => EndPointer.ToInt32() - BaseAddress.ToInt32();
        }

        public RemoteMemoryManager(IntPtr hProcess, int allocationRegionSize = 65536)
        {
            HProcess = hProcess;
            AllocationRegionSize = allocationRegionSize <= 0 ? 65536 : allocationRegionSize;
        }

        protected void AllocateNewRegion()
        {
            var remoteAddr = WinApi.VirtualAllocEx(HProcess, IntPtr.Zero, (uint)AllocationRegionSize, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            if (remoteAddr == IntPtr.Zero)
                throw new Exception("Could not allocate remote memory!");
            CurrentRegion = new Region { BaseAddress = remoteAddr, NextFree = remoteAddr, EndPointer = remoteAddr + (int)AllocationRegionSize };
            Regions.Add(CurrentRegion);
        }

        public IntPtr Allocate(int length)
        {
            while (AllocationRegionSize < length)
                AllocationRegionSize *= 2;

            if (CurrentRegion == null)
                AllocateNewRegion();

            var availBytes = CurrentRegion.EndPointer.ToInt32() - CurrentRegion.NextFree.ToInt32();
            if (availBytes < length)
                AllocateNewRegion();

            var resultAddr = CurrentRegion.NextFree;
            CurrentRegion.NextFree += length;
            return resultAddr;
        }

        public IntPtr Copy(byte[] buffer)
        {
            var remoteAddr = Allocate(buffer.Length);
            uint numBytesWritten;
            WinApi.WriteProcessMemoryBuffer(HProcess, remoteAddr, buffer, buffer.Length, out numBytesWritten);
            if(numBytesWritten != buffer.Length)
                throw new Exception("Could not write enought bytes via WriteProcessMemoryBuffer!");
            return remoteAddr;
        }

        public IntPtr Copy(string data)
        {
            return Copy(Encoding.ASCII.GetBytes(data + "\0"));
        }

        public IntPtr Copy(IntPtr buffer, int length)
        {
            var remoteAddr = Allocate(length);
            uint numBytesWritten;
            WinApi.WriteProcessMemory(HProcess, remoteAddr, buffer, length, out numBytesWritten);
            if (numBytesWritten != length)
                throw new Exception("Could not write enought bytes via WriteProcessMemoryBuffer!");
            return remoteAddr;
        }

        public IntPtr Copy<T>(T obj)
        {
            var size = Marshal.SizeOf(obj);
            var localPtr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(obj, localPtr, false);
                return Copy(localPtr, size);
            }
            finally
            {
                Marshal.FreeHGlobal(localPtr);
            }
        }
    }
}