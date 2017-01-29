namespace ApiHooker
{
    public class ProcessModule
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ulong BaseAddr { get; set; }
        public ulong Size { get; set; }
        public ulong EndAddr => BaseAddr + Size;

        public override string ToString()
        {
            return $"{Name}: 0x{BaseAddr:x8} - 0x{EndAddr:x8}";
        }
    }
}