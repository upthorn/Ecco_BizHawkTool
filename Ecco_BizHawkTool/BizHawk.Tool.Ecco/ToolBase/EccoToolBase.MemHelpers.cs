namespace BizHawk.Tool.Ecco
{
    abstract partial class EccoToolBase
    {
        protected uint ReadU32AndAdvance(ref uint addr)
        {
            uint retval = Mem.ReadU32(addr);
            addr += 4;
            return retval;
        }
        protected int ReadS32AndAdvance(ref uint addr)
        {
            int retval = Mem.ReadS32(addr);
            addr += 4;
            return retval;
        }
        protected uint ReadPtr(uint addr)
        {
            uint retval = Mem.ReadU24(addr + 1);
            return retval;
        }
        protected uint ReadPtrAndAdvance(ref uint addr)
        {
            uint retval = Mem.ReadU24(addr + 1);
            addr += 4;
            return retval;
        }
        protected ushort ReadU16AndAdvance(ref uint addr)
        {
            ushort retval = (ushort) Mem.ReadU16(addr);
            addr += 2;
            return retval;
        }
        protected short ReadS16AndAdvance(ref uint addr)
        {
            short retval = (short)Mem.ReadS16(addr);
            addr += 2;
            return retval;
        }
        protected sbyte ReadSByteAndAdvance(ref uint addr)
        {
            return (sbyte) Mem.ReadS8(addr++);
        }
        protected byte ReadByteAndAdvance(ref uint addr)
        {
            return (byte) Mem.ReadU8(addr++);
        }
    }
}