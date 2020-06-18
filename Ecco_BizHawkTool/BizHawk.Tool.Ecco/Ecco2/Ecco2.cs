using System.Drawing;

using BizHawk.Client.EmuHawk;

namespace BizHawk.Tool.Ecco
{
    partial class Ecco2Tool : EccoToolBase
    {
        #region Type Defs and Loaders
        private static class AddrGlobal
        {
            public const uint LevelID = 0xFFA7D0;
            public const uint LevelFrameCount = 0xFFA7C8;
            public const uint FrameCount = 0xFFA520;
            public const uint NonLagFrameCount = 0xFFA524;
            public const uint GameMode = 0xFFA555;
            public const uint RandomSeed = 0xFFE2F8;
            public const uint TextYSpeed = 0xFFF342;
        }
        #endregion

        #region fields
        private int _camX = 0;
        private int _camY = 0;
        private uint _prevF = 0;
        private uint _levelTime = 0;
        private int _levelID = 0;
        #endregion
        #region Methods
        #endregion

        public Ecco2Tool(CustomMainForm f, GameRegion r) : base(f, r)
        {
			Addr3D = new Addr3DProvider(r);
            switch (r)
            {
                case GameRegion.J:
                    _3DTypeProvider = new J3DProvider();
					_2DTypeProvider = new J2DProvider();
					break;
                case GameRegion.U:
                    _3DTypeProvider = new U3DProvider();
                    _2DTypeProvider = new U2DProvider();
                    break;
                case GameRegion.E:
                    _3DTypeProvider = new E3DProvider();
					_2DTypeProvider = new E2DProvider();
					break;
                default:
                    break;
            }
        }
        public override void PreFrameCallback()
        {
            Gui.ClearText();
            if (!Gui.HasGUISurface)
                Gui.DrawNew("emu");
            _camX = Mem.ReadS16(Addr2D.CamX) - _left;
            _camY = Mem.ReadS16(Addr2D.CamY) - _top;
            switch (Mem.ReadU8(AddrGlobal.GameMode))
            {
                // Dialog screen
                case 0x00:
					if (_top != 112)
					{
						_top = _bottom = 112;
						_left = _right = 160;
						ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
					}
					if (_autofireEnabled)
                    {
                        // This doesn't fully work, but gets > 50%
                        if (Mem.ReadS16(AddrGlobal.TextYSpeed) == -3)
                            Joy.Set("C", true, 1);
                        else
                            Joy.Set("C", false, 1);
                    }
                    break;
                case 0xF6:
					if (_top != 224)
					{
						_top = 224;
						_bottom = 0;
						_left = _right = 160;
						ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
					}
					UpdatePlayer3D();
                    if (_autofireEnabled)
                        AutoFire3D();
                    break;
                case 0x20:
                case 0x28:
                case 0xAC:
					if (_top != 112)
					{
						_top = _bottom = 112;
						_left = _right = 160;
						ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
					}
					UpdatePlayer2D();
					if (_autofireEnabled)
						AutoFire2D();
                    break;
                default:
					if (_top != 112)
					{
						_top = _bottom = 112;
						_left = _right = 160;
						ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
					}
					break;
            }
            if (!_mapDumpingEnabled || (_mapDumpState == 0))
            {
                Color bg = BackdropColor();
                Gui.DrawRectangle(0, 0, _left + 320 + _right, _top, bg, bg);
                Gui.DrawRectangle(0, 0, _left, _top + 224 + _bottom, bg, bg);
                Gui.DrawRectangle(_left + 320, 0, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
                Gui.DrawRectangle(0, _top + 224, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
            }
            switch (Mem.ReadU8(AddrGlobal.GameMode))
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    if (_mapDumpState <= 1) Draw2DHud();
                    if (_mapDumpingEnabled)
                    {
                        PreProcessMapDump();
                    }
                    break;
                case 0xF6:
                    Draw3DHud();
                    break;
                default:
                    break;
            }
			_prevF = Mem.ReadU32(AddrGlobal.NonLagFrameCount);
		}
		public override void CheckLag()
		{
			uint frame = Mem.ReadU32(AddrGlobal.NonLagFrameCount);
			if ((frame <= _prevF) && !Emu.IsLagged())
			{
				Emu.SetIsLagged(true);
				Emu.SetLagCount(Emu.LagCount() + 1);
			}
			_prevF = Mem.ReadU32(AddrGlobal.NonLagFrameCount);
		}
		public override void PostFrameCallback()
        {
			uint frame = Mem.ReadU32(AddrGlobal.NonLagFrameCount);
			CheckLag();
			uint mode = Mem.ReadByte(AddrGlobal.GameMode);
            _levelTime = Mem.ReadU16(AddrGlobal.LevelFrameCount);
            ResetStatusLines();
            StatusText($"Frames: {Mem.ReadU32(AddrGlobal.FrameCount),7} Nonlag: {frame,7} Level: {_levelTime,6} GameMode: {mode:X2}");
            switch (mode)
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    Update2DTickers();
                    if (_mapDumpState != 0)
                    {
                        PostProcessMapDump();
                    }
                    DrawTurnSignal();
                    break;
                case 0xF6:
                    Update3DTickers();
                    break;
            }
            Joy.Set("C", null, 1);
            Joy.Set("Start", null, 1);
            Gui.DrawFinish();
        }
    }
}
