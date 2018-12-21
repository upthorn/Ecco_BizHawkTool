using System;
using System.Windows.Forms;
using System.IO;

using BizHawk.Client.ApiHawk;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
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
		[RequiredService]
		internal IMemoryDomains _memoryDomains { get; set; }
		[RequiredService]
		private IEmulator _emu { get; set; }

        /*
        Name for our external tool
        */
        public const string ToolName = "External Tool";

        /*
        Description for our external tool
        */
        public const string ToolDescription = "External Tool";

        /*
        Icon for our external tool
        */
        public const string IconPath = "icon.ico";

        #endregion

        #region cTor(s)

        public CustomMainForm()
		{
			InitializeComponent();
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

		//We will override F10 quicksave behavior
		private void ClientApi_BeforeQuickSave(object sender, BeforeQuickSaveEventArgs e)
		{
		}

		//We will override F10 quickload behavior
		private void ClientApi_BeforeQuickLoad(object sender, BeforeQuickLoadEventArgs e)
		{
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
		/// when emulator is runnig in turbo mode
		/// </summary>
		public void FastUpdate()
		{ }

        /// <summary>
        /// Restart is called the first time you call the form
        /// but also when you start playing a movie
        /// </summary>
        public void Restart()
        {
            if (Global.Game.Name != "Null")
            {
            }
        }

        /// <summary>
        /// New extensible update method
        /// </summary>
        public void NewUpdate(ToolFormUpdateType type)
        {
            UpdateValues();
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
