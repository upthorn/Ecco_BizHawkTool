using BizHawk.Emulation.Common.WorkingTypes;

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
        protected wushort ReadU16AndAdvance(ref uint addr)
        {
            wushort retval = Mem.ReadU16(addr);
            addr += 2;
            return retval;
        }
        protected wshort ReadS16AndAdvance(ref uint addr)
        {
            wshort retval = Mem.ReadS16(addr);
            addr += 2;
            return retval;
        }
        protected wsbyte ReadSByteAndAdvance(ref uint addr)
        {
            return Mem.ReadS8(addr++);
        }
        protected wbyte ReadByteAndAdvance(ref uint addr)
        {
            return Mem.ReadU8(addr++);
        }
    }
}