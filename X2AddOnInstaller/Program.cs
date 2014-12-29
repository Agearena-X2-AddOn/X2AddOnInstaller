using System;
using System.Windows.Forms;

namespace X2AddOnInstaller
{
	static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static int Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());

			// Sonst meckert Windows, das Programm wäre nicht richtig installiert worden ;-)
			return 0;
		}
	}
}
