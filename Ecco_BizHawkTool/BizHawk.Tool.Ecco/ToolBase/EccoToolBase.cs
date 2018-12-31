using BizHawk.Client.ApiHawk;
using BizHawk.Client.EmuHawk;

namespace BizHawk.Tool.Ecco
{
    public enum GameRegion
    {
        J,
        U,
        E,
        JU,
        JE,
        UE,
        JUE
    }
    abstract partial class EccoToolBase
    {
        private CustomMainForm _form;
        protected IGui Gui => _form.Gui;
        protected IMem Mem => _form.Mem;
        protected IJoypad Joy => _form.Joy;
        protected IEmu Emu => _form.Emu;
        protected IMemorySaveState MemSS => _form.MemSS;
        protected bool _mapDumpingEnabled = false;
        public EccoToolBase(CustomMainForm f, GameRegion r)
        {
            _form = f;
        }
        public void SetMapDumping(bool on)
        {
            _mapDumpingEnabled = on;
        }
        public abstract void PreFrameCallback();
        public abstract void PostFrameCallback();
    }
}
