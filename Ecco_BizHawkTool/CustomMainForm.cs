using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using BizHawk.Client.ApiHawk;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Consoles.Sega.gpgx;
using BizHawk.Client.ApiHawk.Classes.Events;

namespace BizHawk.Client.EmuHawk
{
	/// <summary>
	/// Here your first form
	/// /!\ it MUST be called CustomMainForm and implements IExternalToolForm
	/// Take also care of the namespace
	/// </summary>
	public partial class CustomMainForm : Form, IExternalToolForm
	{
        #region Fields
        /*
		The following stuff will be automatically filled
		by BizHawk runtime
		*/
        [RequiredApi]
        private IMem Mem { get; set; }

        [RequiredApi]
        private IGui Gui { get; set; }

        [RequiredApi]
        private IJoypad Joy { get; set; }

        [RequiredApi]
        private IEmu Emu { get; set; }

        [RequiredApi]
        private IGameInfo GI { get; set; }

        [RequiredApi]
        private IMemorySaveState MemSS { get; set; }

        /*
        Name for our external tool
        */
        public const string ToolName = "Ecco TAS Assistant Tool";

        /*
        Description for our external tool
        */
        public const string ToolDescription = "Provides HUD, fastest-swim autofire, lag detection.";

        /*
        Icon for our external tool
        */
        public const string IconPath = "Ecco_icon.ico";

        #endregion

        #region cTor(s)

        public CustomMainForm()
		{
			InitializeComponent();
            ClientApi.StateLoaded += OnStateLoaded;
		}

		#endregion

		#region Methods

		private void button1_Click(object sender, EventArgs e)
		{
			ClientApi.DoFrameAdvance();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			ClientApi.GetInput(1);
		}

		private void button3_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < 600; i++)
			{
				if (i % 60 == 0)
				{
					Joypad j1 = ClientApi.GetInput(1);
					j1.AddInput(JoypadButton.A);
					ClientApi.SetInput(1, j1);

					ClientApi.DoFrameAdvance();

					j1.RemoveInput(JoypadButton.A);
					ClientApi.SetInput(1, j1);
					ClientApi.DoFrameAdvance();
				}
				ClientApi.DoFrameAdvance();
			}
			Joypad j = ClientApi.GetInput(1);
			j.ClearInputs();
			ClientApi.SetInput(1, j);
		}

		private void OnStateLoaded(object sender, StateLoadedEventArgs e)
		{
            Gui.DrawNew("emu");
            NewUpdate(ToolFormUpdateType.PreFrame);
            Gui.DrawFinish();
        }

        private Color BackdropColor()
        {
            uint color = Mem.ReadU16(0, "CRAM");
            int r = (int)((color & 0x7) * 0x22);
            int g = (int)(((color >> 3) & 0x7) * 0x22);
            int b = (int)(((color >> 6) & 0x7) * 0x22);
            return Color.FromArgb(r, g, b);
        }
        private void PreFrameCallback()
        {
            Gui.ClearText();
            if (!Gui.HasGUISurface)
                Gui.DrawNew("emu");
            if (_mode != Modes.disabled)
            {
                _camX = Mem.ReadS16(_camXAddr) - _left;
                _camY = Mem.ReadS16(_camYAddr) - _top;
                EccoAutofire(Joy.Get(1)["Start"]);
                if (_dumpMap == 0)
                {
                    Color bg = BackdropColor();
                    Gui.DrawRectangle(0, 0, _left + 320 + _right, _top, bg, bg);
                    Gui.DrawRectangle(0, 0, _left, _top + 224 + _bottom, bg, bg);
                    Gui.DrawRectangle(_left + 320, 0, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
                    Gui.DrawRectangle(0, _top + 224, _left + 320 + _right, _top + 224 + _bottom, bg, bg);
                }
                uint mode = Mem.ReadByte(0xFFA555);
                switch (mode)
                {
                    case 0x20:
                    case 0x28:
                    case 0xAC:
                        //ClientApi.SetGameExtraPadding(160, 112, 160, 112);
                        if (_dumpMap <= 1) EccoDrawBoxes();
                        // Uncomment the following block to enable mapdumping
/*                        if ((Mem.ReadU16(0xFFA7C8) > 1) && (Mem.ReadU16(0xFFA7C8) < 4))
                        {
                            _dumpMap = 1;
                            _rowStateGuid = string.Empty;
                            _top = _bottom = _left = _right = 0;
                            ClientApi.SetGameExtraPadding(0, 0, 0, 0);
                        }*/
                        if (_dumpMap == 3)
                        {
                            var levelID = Mem.ReadS8(0xFFA7D0);
                            int[] nameGroupLengths =
                            {
                                7,1,11,6,
                                4,3,3,3,
                                7,1,2,1,
                                0,0,0,0
                            };
                            int[] nameStringPtrOffsets =
                            {
                                0xECBD0, 0x106BC0, 0x10AF8C, 0x135A48,
                                0x1558E8, 0x15F700, 0x16537C, 0x180B00,
                                0x193920, 0x1B3ECC, 0x1D7A44, 0x1DBF70,
                                0x2DF2, 0x2DF6, 0x2DFA, 0x2DFE
                            };
                            int nameGroup = 0;
                            var i = levelID;
                            while ((i >= 0) && (nameGroup < nameGroupLengths.Length))
                            {
                                i -= nameGroupLengths[nameGroup];
                                if (i >= 0) nameGroup++;
                            }
                            string name = "map";
                            if (i < 0)
                            {
                                i += nameGroupLengths[nameGroup];
                                uint strOffset = Mem.ReadU32(nameStringPtrOffsets[nameGroup] + 0x2E);
                                Console.WriteLine($"{i}");
                                strOffset = Mem.ReadU32(strOffset + ((i << 3) + (i << 5)) + 0x22);
                                strOffset += 0x20;
                                List<byte> strTmp = new List<byte>();
                                byte c;
                                do
                                {
                                    c = (byte)Mem.ReadByte(strOffset++);
                                    if (c != 0)
                                        strTmp.Add(c);
                                } while (c != 0);
                                name = System.Text.Encoding.ASCII.GetString(strTmp.ToArray());
                                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                name = textInfo.ToTitleCase(name).Replace(" ", string.Empty);
                            }
                            ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}_{name}_top.png");
                            _destX = _destY = 0;
                            ClientApi.SetGameExtraPadding(0, 0, 0, 0);
                            _dumpMap++;
                        }
                        if (_dumpMap == 6)
                        {
                            var levelID = Mem.ReadS8(0xFFA7D0);
                            int[] nameGroupLengths =
                            {
                                7,1,11,6,
                                4,3,3,3,
                                7,1,2,1,
                                0,0,0,0
                            };
                            int[] nameStringPtrOffsets =
                            {
                                0xECBD0, 0x106BC0, 0x10AF8C, 0x135A48,
                                0x1558E8, 0x15F700, 0x16537C, 0x180B00,
                                0x193920, 0x1B3ECC, 0x1D7A44, 0x1DBF70,
                                0x2DF2, 0x2DF6, 0x2DFA, 0x2DFE
                            };
                            int nameGroup = 0;
                            var i = levelID;
                            while ((i >= 0) && (nameGroup < nameGroupLengths.Length))
                            {
                                i -= nameGroupLengths[nameGroup];
                                if (i >= 0) nameGroup++;
                            }
                            string name = "map";
                            if (i < 0)
                            {
                                i += nameGroupLengths[nameGroup];
                                uint strOffset = Mem.ReadU32(nameStringPtrOffsets[nameGroup] + 0x2E);
                                Console.WriteLine($"{i}");
                                strOffset = Mem.ReadU32(strOffset + ((i << 3) + (i << 5)) + 0x22);
                                strOffset += 0x20;
                                List<byte> strTmp = new List<byte>();
                                byte c;
                                do
                                {
                                    c = (byte)Mem.ReadByte(strOffset++);
                                    if (c != 0)
                                        strTmp.Add(c);
                                } while (c != 0);
                                name = System.Text.Encoding.ASCII.GetString(strTmp.ToArray());
                                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                name = textInfo.ToTitleCase(name).Replace(" ", string.Empty);
                            }
                            ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}_{name}_bottom.png");
                            _destX = _destY = 0;
                            _left = _right = 160;
                            _top = _bottom = 112;
                            ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
                            _dumpMap = 0;
                        }
                        break;
                    case 0xF6:
                        EccoDraw3D();
                        break;
                    default:
                        break;
                }
                _prevF = Mem.ReadU32(0xFFA524);
            }
        }
        private void PostFrameCallback()
        {
            uint frame = Mem.ReadU32(0xFFA524);
            if ((frame <= _prevF) && !Emu.IsLagged())
            {
                Emu.SetIsLagged(true);
                Emu.SetLagCount(Emu.LagCount() + 1);
            }
            uint mode = Mem.ReadByte(0xFFA555);
            _tickerY = 81;
            string valueTicker = $"{Mem.ReadU32(0xFFA520)}:{Mem.ReadU32(0xFFA524)}:{Mem.ReadU16(0xFFA7C8)}:{mode:X2}";
            TickerText(valueTicker);
            switch (mode)
            {
                case 0x20:
                case 0x28:
                case 0xAC:
                    valueTicker = $"{Mem.ReadS16(0xFFAD9C)}:{Mem.ReadS16(0xFFAD9E)}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA1A) / 65536.0:0.######}:{Mem.ReadS32(0xFFAA1E) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA32) / 65536.0:0.######}:{Mem.ReadU8(0xFFAA6D)}:{Mem.ReadU8(0xFFAA6E)}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA36) / 65536.0:0.######}:{Mem.ReadS32(0xFFAA3A) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFA9D6) / 65536.0:0.######}:{Mem.ReadS32(0xFFA9DA) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFAA3E) / 65536.0:0.######}:{Mem.ReadS32(0xFFAA42) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{(Mem.ReadS32(0xFFAA36) + Mem.ReadS32(0xFFA9D6) + Mem.ReadS32(0xFFAA3E)) / 65536.0:0.######}:" +
                                  $"{(Mem.ReadS32(0xFFAA3A) + Mem.ReadS32(0xFFA9DA) + Mem.ReadS32(0xFFAA42)) / 65536.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadU8(0xFFAB72)}:{Mem.ReadU8(0xFFAB70)}:{(short)Mem.ReadS16(0xFFAA52):X4}:{(short)Mem.ReadS16(0xFFAA5A):X4}";
                    TickerText(valueTicker);
                    switch (Mem.ReadU8(0xFFA7D0))
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 30:
                        case 46:
                            var globeFlags = Mem.ReadU32(0xFFD434) >> 1;
                            var globeFlags2 = Mem.ReadU32(0xFFD438) >> 1;
                            int i, j = i = 0;
                            while (globeFlags > 0)
                            {
                                globeFlags >>= 1;
                                i++;
                            }
                            while (globeFlags2 > 0)
                            {
                                globeFlags2 >>= 1;
                                j++;
                            }
                            TickerText($"{i}:{j}", Color.Blue);
                            break;
                        default:
                            break;
                    }
                    if (_dumpMap != 0)
                    {
                        Mem.WriteS16(0xFFAA16, 7);
                        Mem.WriteS16(0xFFAA18, 56);
                        int PlayerX = Mem.ReadS16(0xFFAA1A) - _camX;
                        int PlayerY = Mem.ReadS16(0xFFAA1E) - _camY;
                        if ((PlayerX < -64) || (PlayerX > 384) || (PlayerY < -64) || (PlayerY > 288))
                        {
                            Mem.WriteByte(0xFFAA70, 0xC);
                            Mem.WriteS16(0xFFA7CA, 1);
                        }
                        else
                        {
                            Mem.WriteByte(0xFFAA70, 0x0);
                            Mem.WriteS16(0xFFA7CA, 0);
                        }
                    }
                    if (_dumpMap == 1)
                    {
                        int levelWidth = Mem.ReadS16(0xFFA7A8);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        var levelID = Mem.ReadByte(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = false;
                        s.DrawBGB = false;
                        s.DrawBGW = false;
                        s.DrawObj = false;
                        s.Backdrop = true;
                        Emu.PutSettings(s);
                        if ((_camX == _destX) && (_camY == _destY))
                        {
                            if ((_prevX != _camX) || (_prevY != _camY))
                            {
                                if (_destX == 0)
                                {
                                    if (_rowStateGuid != string.Empty)
                                    {
                                        MemSS.DeleteState(_rowStateGuid);
                                    }
                                    _rowStateGuid = MemSS.SaveCoreStateToMemory();
                                }
                                _snapPast = 1;
                            }
                            else
                            {
                                _snapPast--;
                            }
                            if (_snapPast == 0)
                            {
                                ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}\\{_destY}_{_destX}_top.png");
                                if (_destX >= levelWidth - 320)
                                {
                                    if (_destY < levelHeight - 224)
                                    {
                                        if (_rowStateGuid != string.Empty)
                                        {
                                            MemSS.LoadCoreStateFromMemory(_rowStateGuid);
                                        }
                                        _destX = 0;
                                        _destY = Math.Min(_destY + 111, levelHeight - 224);
                                    }
                                }
                                else
                                    _destX = Math.Min(_destX + 159, levelWidth - 320);
                                if ((_prevX == _destX) && (_prevY == _destY))
                                {
                                    ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                                    _dumpMap++;
                                }
                            }
                        }
                        Mem.WriteS16(0xFFAD8C, _destX);
                        Mem.WriteS16(0xFFAD90, _destY);
                    }
                    else if (_dumpMap == 2)
                    {
                        if (_rowStateGuid != String.Empty)
                            MemSS.DeleteState(_rowStateGuid);
                        _rowStateGuid = String.Empty;
                        int levelWidth = Mem.ReadS16(0xFFA7A8);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                        var levelID = Mem.ReadS8(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = false;
                        s.DrawBGB = false;
                        s.DrawBGW = false;
                        s.DrawObj = false;
                        s.Backdrop = true;
                        Emu.PutSettings(s);

                        var a = Gui.GetAttributes();
                        a.SetColorKey(Color.FromArgb(0, 0x11, 0x22, 0x33), Color.FromArgb(0, 0x11, 0x22, 0x33));
                        Gui.SetAttributes(a);
                        Gui.ToggleCompositingMode();

                        Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{levelWidth - 320}_top.png", 2, 2, 318, 222, (levelWidth - 318), (levelHeight - 222), 318, 222);
                        for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                        {
                            var dx = (x == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{x}_top.png", dx, 2, 320 - dx, 222, x + dx, (levelHeight - 222), 320 - dx, 222);
                        }
                        for (int y = ((levelHeight - 224) / 111) * 111; y >= 0; y -= 111)
                        {
                            var dy = (y == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{levelWidth - 320}_top.png", 2, dy, 318, 224 - 2, levelWidth - 318, y + dy, 318, 224 - dy);
                            for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                            {
                                var dx = (x == 0) ? 0 : 2;
                                Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{x}_top.png", dx, dy, 320 - dx, 224 - dy, x + dx, y + dy, 320 - dx, 224 - dy);
                            }
                        }

                        Gui.ToggleCompositingMode();
                        Gui.SetAttributes(new System.Drawing.Imaging.ImageAttributes());
                        Gui.DrawFinish();
                        _dumpMap++;
                    }
                    else if (_dumpMap == 4)
                    {
                        int levelWidth = Mem.ReadS16(0xFFA7A8);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        var levelID = Mem.ReadByte(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = (levelID != 29);
                        s.DrawBGB = (levelID == 7);
                        s.DrawBGW = true;
                        s.DrawObj = true;
                        s.Backdrop = false;
                        Emu.PutSettings(s);
                        if ((_camX == _destX) && (_camY == _destY))
                        {
                            if ((_prevX != _camX) || (_prevY != _camY))
                            {
                                if (_destX == 0)
                                {
                                    if (_rowStateGuid != string.Empty)
                                    {
                                        MemSS.DeleteState(_rowStateGuid);
                                    }
                                    _rowStateGuid = MemSS.SaveCoreStateToMemory();
                                }
                                _snapPast = 1;
                            }
                            else
                            {
                                _snapPast--;
                            }
                            if (_snapPast == 0)
                            {
                                ClientApi.Screenshot($"c:\\Ecco2Maps\\{levelID}\\{_destY}_{_destX}_bottom.png");
                                if (_destX >= levelWidth - 320)
                                {
                                    if (_destY < levelHeight - 224)
                                    {
                                        if (_rowStateGuid != string.Empty)
                                        {
                                            MemSS.LoadCoreStateFromMemory(_rowStateGuid);
                                        }
                                        _destX = 0;
                                        _destY = Math.Min(_destY + 111, levelHeight - 224);
                                    }
                                }
                                else
                                    _destX = Math.Min(_destX + 159, levelWidth - 320);
                                if ((_prevX == _destX) && (_prevY == _destY))
                                {
                                    ClientApi.SetGameExtraPadding(levelWidth - 320, levelHeight - 224, 0, 0);
                                    _dumpMap++;
                                }
                            }
                        }
                        Mem.WriteS16(0xFFAD8C, _destX);
                        Mem.WriteS16(0xFFAD90, _destY);
                    }
                    else if (_dumpMap == 5)
                    {
                        if (_rowStateGuid != String.Empty)
                            MemSS.DeleteState(_rowStateGuid);
                        _rowStateGuid = String.Empty;
                        int levelWidth = Mem.ReadS16(0xFFA7A8);
                        int levelHeight = Mem.ReadS16(0xFFA7AC);
                        var levelID = Mem.ReadS8(0xFFA7D0);
                        var s = Emu.GetSettings() as GPGX.GPGXSettings;
                        s.DrawBGA = (levelID != 29);
                        s.DrawBGB = (levelID == 7);
                        s.DrawBGW = true;
                        s.DrawObj = true;
                        s.Backdrop = false;
                        Emu.PutSettings(s);
                        Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{levelWidth - 320}_bottom.png", 2, 2, 318, 222, (levelWidth - 318), (levelHeight - 222), 318, 222);
                        for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                        {
                            var dx = (x == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{levelHeight - 224}_{x}_bottom.png", dx, 2, 320 - dx, 222, x + dx, (levelHeight - 222), 320 - dx, 222);
                        }
                        for (int y = ((levelHeight - 224) / 111) * 111; y >= 0; y -= 111)
                        {
                            var dy = (y == 0) ? 0 : 2;
                            Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{levelWidth - 320}_bottom.png", 2, dy, 318, 224 - 2, levelWidth - 318, y + dy, 318, 224 - dy);
                            for (int x = ((levelWidth - 320) / 159) * 159; x >= 0; x -= 159)
                            {
                                var dx = (x == 0) ? 0 : 2;
                                Gui.DrawImageRegion($"c:\\Ecco2Maps\\{levelID}\\{y}_{x}_bottom.png", dx, dy, 320 - dx, 224 - dy, x + dx, y + dy, 320 - dx, 224 - dy);
                            }
                        }
                        Gui.DrawFinish();
                        _dumpMap++;
                    }
                    _prevX = _camX;
                    _prevY = _camY;
                    break;
                case 0xF6:
                    valueTicker = $"{Mem.ReadS32(0xFFD5E0) / 4096.0:0.######}:{Mem.ReadS32(0xFFD5E8) / 4096.0:0.######}:{Mem.ReadS32(0xFFD5E4) / 2048.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFB13A) / 4096.0:0.######}:{Mem.ReadS32(0xFFB142) / 4096.0:0.######}:{Mem.ReadS32(0xFFB13E) / 2048.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadS32(0xFFB162) / 4096.0:0.######}:{Mem.ReadS32(0xFFB16A) / 4096.0:0.######}:{Mem.ReadS32(0xFFB166) / 2048.0:0.######}";
                    TickerText(valueTicker);
                    valueTicker = $"{Mem.ReadU8(0xFFB19B)}:{Mem.ReadU8(0xFFB1A6)}:{Mem.ReadU8(0xFFB1A9)}";
                    TickerText(valueTicker);
                    int SpawnZ = Mem.ReadS32(0xFFD5F0) + 0x180000;
                    int nextRingZ = SpawnZ;
                    while (((nextRingZ >> 17) & 0xF) != 0)
                    {
                        nextRingZ += 0x20000;
                    }
                    valueTicker = $"{Mem.ReadS32(0xFFD856) / 4096.0:0.######}:{Mem.ReadS32(0xFFD85A) / 4096.0:0.######}:{(nextRingZ - 0x160000) / 2048.0:0.######}:{nextRingZ / 2048.0:0.######}";
                    TickerText(valueTicker);
                    var levelId = -1 - Mem.ReadS16(0xFFA79E);
                    bool spawn = false;
                    bool firstRand = true;
                    int SpawnX, SpawnY, z;
                    int CamX = (Mem.ReadS32(0xFFD5E0) >> 0xC) - _left;
                    int CamY = (Mem.ReadS32(0xFFD5E8) >> 0xC) + _top;
                    int CamZ = (Mem.ReadS32(0xFFD5E4) >> 0xC) + _top;
                    while (!spawn)
                    {
                        var temp = (SpawnZ >> 17) & 0xFF;
                        var controlList = Mem.ReadS32(0x7B54 + (levelId << 2));
                        temp = Mem.ReadS16(controlList + (temp << 1));
                        var v = temp & 0xFF;
                        var num = (temp >> 8) + v;
                        temp = v;
                        spawn = (num > 2);
                        if (spawn) for (; temp < num; temp++)
                            {
                                switch (temp)
                                {
                                    case 0:
                                    case 1:
                                    case 13:
                                        // Nothing important spawns
                                        break;
                                    case 2:
                                        // Jellyfish
                                        SpawnX = Mem.ReadS32(0xFFB13A) + 0x40000 - (EccoRand(firstRand) << 3);
                                        firstRand = false;
                                        SpawnY = -0xC0000 + (EccoRand() << 3);
                                        z = SpawnZ + 0x20000;// ? 
                                        valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{z / 2048.0:0.######}";
                                        TickerText(valueTicker);
                                        SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                        SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                        z = _top + 112 - ((z >> 0xC) - CamZ);
                                        DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                        DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                        break;
                                    case 3:
                                        // Eagle
                                        SpawnX = Mem.ReadS32(0xFFB13A) + 0x40000 - (EccoRand(firstRand) << 3);
                                        firstRand = false;
                                        SpawnY = 0x50000;
                                        z = SpawnZ - 0x40000 + 0x20000;// ? 
                                        valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{z / 2048.0:0.######}";
                                        TickerText(valueTicker);
                                        SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                        SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                        z = _top + 112 - ((z >> 0xC) - CamZ);
                                        DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                        DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                        break;
                                    case 4:
                                        // Shark
                                        bool left = (EccoRand(firstRand) > 0x8000);
                                        firstRand = false;
                                        var xdiff = 0xC0000 + (EccoRand() << 3);
                                        SpawnX = Mem.ReadS32(0xFFB13A) + (left ? -xdiff : xdiff);
                                        SpawnY = Math.Min(Mem.ReadS32(0xFFB142), -0x10000) - (EccoRand() + 0x10000);
                                        z = SpawnZ + 0x20000;
                                        valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{z / 2048.0:0.######}";
                                        TickerText(valueTicker);
                                        SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                        SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                        z = _top + 112 - ((z >> 0xC) - CamZ);
                                        DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                        DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                        break;
                                    case 5:
                                    case 6:
                                    case 7:
                                    case 8:
                                        // Vine
                                        EccoRand(firstRand);
                                        firstRand = false;
                                        if ((temp & 1) == 1) EccoRand();
                                        EccoRand();
                                        break;
                                    case 9:
                                    case 10:
                                    case 11:
                                    case 12:
                                        // Unknown, possibly just rand incrementation?
                                        EccoRand(firstRand);
                                        firstRand = false;
                                        if ((temp & 1) == 1) EccoRand();
                                        break;
                                    case 14:
                                        // Shell
                                        SpawnX = Mem.ReadS32(0xFFB13A) - 0x20000 + (EccoRand(firstRand) << 2);
                                        firstRand = false;
                                        SpawnY = -0x80000;
                                        z = SpawnZ + 0x20000;
                                        EccoRand();
                                        valueTicker = $"{SpawnX / 4096.0:0.######}:{SpawnY / 4096.0:0.######}:{(z - 0x180000) / 2048.0:0.######}:{(z - 0x80000) / 2048.0:0.######}";
                                        TickerText(valueTicker);
                                        SpawnX = 160 + ((SpawnX >> 0xC) - CamX);
                                        SpawnY = 112 - ((SpawnY >> 0xC) - CamY);
                                        z = _top + 112 - ((z >> 0xC) - CamZ);
                                        DrawBoxMWH(SpawnX, SpawnY, 1, 1, Color.Gray);
                                        DrawBoxMWH(SpawnX, z, 1, 1, Color.Gray);
                                        break;
                                }
                            }
                        SpawnZ += 0x20000;
                    }
                    break;
            }
            Joy.Set("C", null, 1);
            Joy.Set("Start", null, 1);
            var color = _turnSignalColors[Mem.ReadS8(0xFFA7C9) & 7];
            Gui.DrawRectangle(_left - 48, _top - 112, 15, 15, color, color);
            Gui.DrawFinish();
        }
        #endregion

        #region BizHawk Required methods

        /// <summary>
        /// Return true if you want the <see cref="UpdateValues"/> method
        /// to be called before rendering
        /// </summary>
        public bool UpdateBefore
		{
			get
			{
				return true;
			}
		}

		public bool AskSaveChanges()
		{
			return true;
		}

		/// <summary>
		/// This method is called instead of regular <see cref="UpdateValues"/>
		/// when emulator is running in turbo mode
		/// </summary>
		public void FastUpdate()
		{ }

        private void Init()
        {
            Mem.SetBigEndian();
            string gameName = GI.GetRomName();
            if ((gameName == "ECCO - The Tides of Time (J) [!]") ||
                (gameName == "ECCO - The Tides of Time (U) [!]") ||
                (gameName == "ECCO - The Tides of Time (E) [!]"))
            {
                _mode = Modes.Ecco2;
                _camXAddr = 0xFFAD9C;
                _camYAddr = 0xFFAD9E;
                _top = _bottom = 112;
                _left = _right = 160;
                ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
            }
            else if ((gameName == "ECCO The Dolphin (J) [!]") ||
                     (gameName == "ECCO The Dolphin (UE) [!]"))

            {
                _mode = Modes.Ecco1;
                _camXAddr = 0xFFB836;
                _camYAddr = 0xFFB834;
                _top = _bottom = 112;
                _left = _right = 160;
                ClientApi.SetGameExtraPadding(_left, _top, _right, _bottom);
            }
            else
            {
                _mode = Modes.disabled;
            }

        }

        /// <summary>
        /// Restart is called the first time you call the form
        /// but also when you start playing a movie
        /// </summary>
        public void Restart()
        {
            Init();
        }

        /// <summary>
        /// New extensible update method
        /// </summary>
        public void NewUpdate(ToolFormUpdateType type)
        {
            switch (type)
            {
                case ToolFormUpdateType.Reset:
                    Init();
                    break;
                case ToolFormUpdateType.PreFrame:
                    PreFrameCallback();
                    break;
                case ToolFormUpdateType.PostFrame:
                    PostFrameCallback();
                    break;
                default:
                    break;
            }
        }
            
        /// <summary>
        /// This method is called when a frame is rendered
        /// You can comapre it the lua equivalent emu.frameadvance()
        /// </summary>
        public void UpdateValues()
		{
			if (Global.Game.Name != "Null")
			{
				//Update form
			}
		}

		#endregion BizHawk Required methods
	}
}
