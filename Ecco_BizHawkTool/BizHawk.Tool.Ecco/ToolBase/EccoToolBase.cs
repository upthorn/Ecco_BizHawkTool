using BizHawk.Client.Common;
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
        protected int _mapDumpState = 0;
        protected bool _autofireEnabled = false;
		protected bool _showNumbers = false;
		protected string MapDumpFolder => _form.mapDumpFolder.Text;
        public EccoToolBase(CustomMainForm f, GameRegion r)
        {
            _form = f;
        }
        public void SetMapDumping(bool on)
        {
            _mapDumpingEnabled = on;
        }
        public void SetAutofire(bool on)
        {
            _autofireEnabled = on;
        }
		public void SetShowNumbers(bool on)
		{
			_showNumbers = on;
		}
        public abstract void PreFrameCallback();
		public abstract void CheckLag();
        public abstract void PostFrameCallback();
    }
}
