using BizHawk.Client.ApiHawk;
using BizHawk.Client.EmuHawk;

namespace BizHawk.Tool.Ecco
{
    abstract partial class EccoToolBase
    {
        protected IGui Gui;
        protected IMem Mem;
        protected IJoypad Joy;
        protected IEmu Emu;
        protected IMemorySaveState MemSS;
        protected bool _mapDumpingEnabled = false;
        protected CustomMainForm _form;
        public EccoToolBase(IMem m, IGui g, IJoypad j, IEmu e, IMemorySaveState s)
        {
            Gui = g;
            Mem = m;
            Joy = j;
            Emu = e;
            MemSS = s;
        }
        public void EnableMapDumping(bool enabled)
        {
            _mapDumpingEnabled = enabled;
        }
        public abstract void PreFrameCallback();
        public abstract void PostFrameCallback();
    }
}
