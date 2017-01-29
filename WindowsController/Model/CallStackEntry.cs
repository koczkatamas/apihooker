using System;

namespace ApiHooker.Model
{
    public class CallStackEntry
    {
        public ulong Address { get; set; }
        public ProcessModule Module { get; set; }
    }
}