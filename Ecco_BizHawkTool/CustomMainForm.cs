using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using BizHawk.Client.ApiHawk;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using BizHawk.Client.ApiHawk.Classes.Events;
using BizHawk.Tool.Ecco;

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
        public IMem Mem { get; set; }

        [RequiredApi]
        public IGui Gui { get; set; }

        [RequiredApi]
        public IJoypad Joy { get; set; }

        [RequiredApi]
        public IEmu Emu { get; set; }

        [RequiredApi]
        public IGameInfo GI { get; set; }

        [RequiredApi]
        public IMemorySaveState MemSS { get; set; }

        /*
        Name for our external tool
        */
        public const string ToolName = "Ecco TAS Assistant";

        /*
        Description for our external tool
        */
        public const string ToolDescription = "Provides HUD, fastest-swim autofire, lag detection.";

        /*
        Icon for our external tool
        */
        public const string IconPath = "Ecco_icon.ico";

        private enum Modes { disabled, Ecco1, Ecco2 }
        private EccoToolBase _tool;
		private List<string> _statusLines = new List<string>();
		private List<Color?> _statusColors = new List<Color?>();
        #endregion

        #region cTor(s)

        public CustomMainForm()
		{
			InitializeComponent();
            ClientApi.StateLoaded += OnStateLoaded;
            FormClosed += CustomMainForm_FormClosed;
		}

		#endregion

		#region Winform Methods
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

        private void mapDumpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _tool.SetMapDumping(mapDumpCheckbox.Checked);
        }
        private void autoFireCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            _tool.SetAutofire(autoFireCheckbox.Checked);
        }
        private void mapDumpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (mapFolderBrowseDialog.ShowDialog() == DialogResult.OK)
            {
                mapDumpFolder.Text = mapFolderBrowseDialog.SelectedPath;
            }
        }
		public void CustomMainForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				SuspendLayout();
			}
			else
			{
				ResumeLayout();
			}
		}
		public void CustomMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Gui.DrawNew("emu");
            Gui.DrawFinish();
            ClientApi.SetGameExtraPadding(0);
        }
        //private void PostFrameCallback()
        #endregion
        #region Tool Methods
        public void SetStatusLine(string message, int line, Color? fg)
        {
			if (WindowState != FormWindowState.Minimized)
			{
				while (StatusTextBox.Lines.Length <= line)
				{
					StatusTextBox.AppendText(Environment.NewLine);
				}
				StatusTextBox.SelectionStart = StatusTextBox.GetFirstCharIndexFromLine(line);
				StatusTextBox.SelectionLength = StatusTextBox.Lines[line].Length;
				StatusTextBox.SelectionColor = fg ?? StatusTextBox.ForeColor;
				StatusTextBox.SelectedText = message;
			}
        }
        private void OnStateLoaded(object sender, StateLoadedEventArgs e)
        {
            Gui.DrawNew("emu");
            _tool.PreFrameCallback();
            _tool.PostFrameCallback();
            Gui.DrawFinish();
        }

        private void Init()
        {
            Mem.SetBigEndian();
            string gameName = GI.GetRomName();
            switch (gameName)
            {
                case "ECCO - The Tides of Time (J) [!]":
                    _tool = new Ecco2Tool(this, GameRegion.J);
                    break;
                case "ECCO - The Tides of Time (U) [!]":
                    _tool = new Ecco2Tool(this, GameRegion.U);
                    break;
                case "ECCO - The Tides of Time (E) [!]":
                    _tool = new Ecco2Tool(this, GameRegion.E);
                    break;
                case "ECCO The Dolphin (J) [!]":
                case "ECCO The Dolphin (UE) [!]":
                /*_tool = new EccoTool(this, GameRegion.UE);*/
                default:
                    Close();
                    break;
            }
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
                    SuspendLayout();
                    _tool.PreFrameCallback();
                    break;
                case ToolFormUpdateType.PostFrame:
                    _tool.PostFrameCallback();
                    ResumeLayout(false);
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
