using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using IORAMHelper;

namespace X2AddOnInstaller
{
	public partial class MainForm : Form
	{
		#region Konstanten

		/// <summary>
		/// Die Basis-Adresse zum Installationsserver.
		/// </summary>
		const string SERVER_BASE_URI = "http://www.agearena.de/x2/";

		#endregion

		#region Variablen

		/// <summary>
		/// Die aktuell installierte Version.
		/// </summary>
		string _installedRevision = "";

		/// <summary>
		/// Die letzte Version auf dem Server.
		/// </summary>
		string _remoteRevision = "";

		/// <summary>
		/// Der Registry-Key des Spiels.
		/// </summary>
		Microsoft.Win32.RegistryKey _aoe2Key = null;

		/// <summary>
		/// Das WebClient-Objekt für die Kommunikation mit dem Server.
		/// </summary>
		WebClient _webClient = new WebClient();

		/// <summary>
		/// Gibt an, ob die linke Maustaste aktuell gedrückt ist (zum Verschieben des Fensters).
		/// </summary>
		bool _mouseDown;

		/// <summary>
		/// Die letzte Position des Mauszeigers (zum Verschieben des Fensters).
		/// </summary>
		Point _lastLocation;

		#endregion

		#region Funktionen

		/// <summary>
		/// Konstruktor.
		/// </summary>
		public MainForm()
		{
			// Steuerelemente laden
			InitializeComponent();

			// Ereignisse beim WebClient zuweisen
			_webClient.DownloadDataCompleted += _webClient_DownloadDataCompleted;
			_webClient.DownloadProgressChanged += _webClient_DownloadProgressChanged;
		}

		/// <summary>
		/// Entpackt das übergebene LZMA-Archiv.
		/// </summary>
		/// <param name="archive">Das zu entpackende LZMA-Archiv.</param>
		/// <param name="aoe2Path">Der Ziel-Pfad.</param>
		private void Unpack(byte[] archive, string aoe2Path)
		{
			// Eingabestream öffnen
			SetStatus("Lade Installationsarchiv...");
			Stream streamIn = new MemoryStream(archive);

			// Ausgabestream erstellen (alles in den Arbeitsspeicher schreiben)
			Stream streamOut = new MemoryStream();

			// Decoder erstellen
			SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();

			// Kompressionsparameter lesen
			byte[] decProps = new byte[5];
			streamIn.Read(decProps, 0, 5);
			decoder.SetDecoderProperties(decProps);

			// Größe der Originaldaten lesen
			long outSize = 0;
			for(int i = 0; i < 8; ++i)
			{
				// Byte lesen
				byte v = (byte)streamIn.ReadByte();
				outSize |= ((long)v) << (i << 3);
			}

			// Größe der komprimierten Daten ermitteln
			long comprSize = streamIn.Length - streamIn.Position;

			// Dekomprimieren
			SetStatus("Dekomprimiere Installationsarchiv...");
			decoder.Code(streamIn, streamOut, comprSize, outSize, null);

			// Dekomprimierte Daten in Puffer schreiben
			byte[] data = new byte[outSize];
			streamOut.Seek(0, SeekOrigin.Begin);
			streamOut.Read(data, 0, (int)outSize);
			RAMBuffer buffer = new RAMBuffer(data);

			// Sicherheitshalber Games- und age2_x1-Verzeichnisse anlegen
			string rootFolderName = aoe2Path + "Games\\";
			Directory.CreateDirectory(rootFolderName);
			Directory.CreateDirectory(aoe2Path + "age2_x1\\");

			// XML-Spezifikationen lesen
			SetStatus("Installiere XML-Spezifikationen...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// XML-Datei speichern
				File.WriteAllBytes(rootFolderName + currFileName, currFile);

				// Der Name der XML-Datei ist der Name des AddOn-Ordners
				rootFolderName += Path.GetFileNameWithoutExtension(currFileName) + "\\";

				// Ordnerstruktur anlegen
				Directory.CreateDirectory(rootFolderName);
				Directory.CreateDirectory(rootFolderName + "Data");
				Directory.CreateDirectory(rootFolderName + "Data\\de");
				Directory.CreateDirectory(rootFolderName + "Data\\en");
				Directory.CreateDirectory(rootFolderName + "SaveGame\\Multi");
				Directory.CreateDirectory(rootFolderName + "Scenario");
				Directory.CreateDirectory(rootFolderName + "Screenshots");
				Directory.CreateDirectory(rootFolderName + "Script.AI");
				Directory.CreateDirectory(rootFolderName + "Script.RM");
				Directory.CreateDirectory(rootFolderName + "Sound\\scenario");
				Directory.CreateDirectory(rootFolderName + "Sound\\stream");
				Directory.CreateDirectory(rootFolderName + "ProjectSources");
			});

			// Language-DLLs lesen
			SetStatus("Installiere Language-DLLs...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// DLL-Datei speichern
				File.WriteAllBytes(rootFolderName + "Data\\de\\" + currFileName, currFile);
			});
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// DLL-Datei speichern
				File.WriteAllBytes(rootFolderName + "Data\\en\\" + currFileName, currFile);
			});
			string lang = (_languageGermanButton.Checked ? "de" : "en");
			foreach(string langDllFile in Directory.GetFiles(rootFolderName + "Data\\" + lang + "\\"))
				File.Copy(langDllFile, rootFolderName + "Data\\" + Path.GetFileName(langDllFile), true);

			// Datendateien lesen
			SetStatus("Installiere Datendateien...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// DAT-Datei speichern
				File.WriteAllBytes(rootFolderName + "Data\\" + currFileName, currFile);
			});

			// Ausführbare Dateien lesen
			SetStatus("Installiere ausführbare Dateien...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// Ausführbare Datei speichern
				File.WriteAllBytes(aoe2Path + "age2_x1\\" + currFileName, currFile);
			});

			// DRS-Dateien lesen
			SetStatus("Installiere DRS-Dateien...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// DRS-Datei speichern
				File.WriteAllBytes(rootFolderName + "Data\\" + currFileName, currFile);
			});

			// Projektdateien lesen
			SetStatus("Extrahiere Projektdateien...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// Projektdatei speichern
				File.WriteAllBytes(rootFolderName + "ProjectSources\\" + currFileName, currFile);
			});

			// KI-Dateien lesen
			SetStatus("Extrahiere KI-Dateien...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// KI-Datei speichern
				File.WriteAllBytes(rootFolderName + "Script.AI\\" + currFileName, currFile);
			});

			// Neue Revision speichern
			if(_aoe2Key != null)
				try
				{
					_aoe2Key.SetValue("AgearenaAddOnRevision", _remoteRevision);
				}
				catch
				{
					// Benachrichtigen
					MessageBox.Show("Warnung: Konnte die neue Versionsnummer nicht speichern.", "Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

			// Desktop-Icon erstellen
			// Problem: Abhängigkeits-DLL muss mitgeliefert werden
			/*if(MessageBox.Show("Desktop-Icon erstellen?", "Installation abschließen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				string link = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Agearena AddOn.lnk");
				var shell = new IWshRuntimeLibrary.WshShell();
				var shortcut = shell.CreateShortcut(link) as IWshRuntimeLibrary.IWshShortcut;
				shortcut.TargetPath = aoe2Path + "age2_x1\\AgearenaAddOn.exe";
				shortcut.WorkingDirectory = aoe2Path + "age2_x1";
				shortcut.Save();
			}*/

			// Fertig
			SetStatus("Installation erfolgreich!");
		}

		/// <summary>
		/// Liest einen Block aus dem Archiv-Puffer und führt für diesen die angegebene Aktion aus.
		/// </summary>
		/// <param name="buffer">Der Archiv-Puffer, aus dem gelesen werden soll.</param>
		/// <param name="handler">Der auszuführende Handler. Diesem werden als Parameter Dateiname und Dateiinhalt übergeben.</param>
		private void ReadArchiveBlock(RAMBuffer buffer, Action<string, byte[]> handler)
		{
			// Dateien lesen und Handler ausführen
			int count = buffer.ReadInteger();
			for(int i = 0; i < count; ++i)
				handler(buffer.ReadString(buffer.ReadInteger()), buffer.ReadByteArray(buffer.ReadInteger()));
		}

		/// <summary>
		/// Setzt das Status-Label auf den gegebenen Text.
		/// </summary>
		/// <param name="text">Der Status-Text.</param>
		/// <param name="isAsync">Gibt an, ob der Status von einem asynchronen Prozess aus aktualisiert wird.</param>
		private void SetStatus(string text, bool isAsync = false)
		{
			// Text setzen
			_statusLabel.Text = text;

			// Neu zeichnen
			if(!isAsync)
				Application.DoEvents();
		}

		#endregion

		#region Ereignishandler

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Age of Empires II-Schlüssel suchen
			const string aoe2KeyBasePath = "SOFTWARE\\Microsoft\\Microsoft Games\\Age of Empires II: The Conquerors Expansion\\1.0";
			try
			{
				// LocalMachine-Schlüssel abfragen
				if((_aoe2Key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(aoe2KeyBasePath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)) == null
					|| _aoe2Key.ValueCount == 0)
					if((_aoe2Key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(aoe2KeyBasePath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)) == null
						|| _aoe2Key.ValueCount == 0)
						MessageBox.Show("Warnung: Kann AoE-II-Registrierungsschlüssel nicht finden. Bestimmen der installierten Version nicht möglich.\r\n" +
						"Bitte stellen Sie sicher, dass eine ordnungsgemäße \"Age of Empires II: The Conquerors\"-Installation vorliegt.",
						"Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch(System.Security.SecurityException)
			{
				MessageBox.Show("Kein Zugriff auf die Registry.\r\nBitte stellen Sie sicher, dass das Installationsprogramm mit Administratorrechten ausgeführt wird.",
					"Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Werte laden
			if(_aoe2Key != null)
			{
				// Aktuelle Revision herausfinden
				_installedRevision = (string)_aoe2Key.GetValue("AgearenaAddOnRevision", _installedRevision);

				// AoE-II-Pfad holen
				_aoe2FolderTextBox.Text = (string)_aoe2Key.GetValue("EXE Path", "");
			}
		}

		private void _exitButton_Click(object sender, EventArgs e)
		{
			// Programm schließen
			this.Close();
		}

		private void _folderSelectButton_Click(object sender, EventArgs e)
		{
			// Dialog anzeigen
			if(Directory.Exists(_aoe2FolderTextBox.Text))
				_folderDialog.SelectedPath = _aoe2FolderTextBox.Text;
			if(_folderDialog.ShowDialog() == DialogResult.OK)
			{
				// Neuen Pfad merken
				_aoe2FolderTextBox.Text = _folderDialog.SelectedPath;
			}
		}

		private void _installButton_Click(object sender, EventArgs e)
		{
			// Steuerelemente blockieren
			_aoe2FolderTextBox.Enabled = false;
			_folderSelectButton.Enabled = false;
			_languageGermanButton.Enabled = false;
			_languageEnglishButton.Enabled = false;
			_installButton.Enabled = false;

			// Datei holen
			SetStatus("Hole Installationsarchiv vom Server...");
			try
			{
				// Asynchron, um Fortschrittsanzeige darstellen zu können
				_webClient.DownloadDataAsync(new Uri(SERVER_BASE_URI + "archive?mode=preview"));
				while(_webClient.IsBusy)
					Application.DoEvents();
			}
			catch(WebException)
			{
				// Fehler
				MessageBox.Show("Fehler: Konnte das Installationsarchiv nicht herunterladen. Bitte überprüfen Sie Ihre Internetverbindung.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetStatus("Fehler.");
				return;
			}
		}

		private void _webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			// Status aktualisieren
			SetStatus("Hole Installationsarchiv vom Server..." + e.ProgressPercentage + "% (" + (e.BytesReceived / 1000).ToString("N0") + " kB von " + (e.TotalBytesToReceive / 1000).ToString("N0") + " kB)", true);
		}

		private void _webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			// Datei entpacken
			try
			{
				Unpack(e.Result, Path.GetFullPath(_aoe2FolderTextBox.Text) + "\\");
			}
			catch(Exception ex)
			{
				// Fehler
				MessageBox.Show("Fehler: Das Installationsarchiv konnte nicht ordnungsgemäß entpackt werden. Bitte stellen Sie sicher, dass das Installationsprogramm mit Administratorrechten ausgeführt wird.\r\n\r\nFehlermeldung: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetStatus("Fehler.");
			}

			// Steuerelemente freischalten
			_aoe2FolderTextBox.Enabled = true;
			_folderSelectButton.Enabled = true;
			_languageGermanButton.Enabled = true;
			_languageEnglishButton.Enabled = true;
			_installButton.Enabled = true;
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			// Neue Version vom Server holen
			SetStatus("Hole aktuelle Versionsnummer vom Server...");
			try
			{
				_remoteRevision = _webClient.DownloadString(SERVER_BASE_URI + "revision?mode=preview");
			}
			catch { }

			// Version gefunden?
			if(_remoteRevision == "" || _remoteRevision == _installedRevision)
				SetStatus("Keine neue Version verfügbar.");
			else
				SetStatus("Neue Version verfügbar: " + _remoteRevision + (_installedRevision != "" ? " (installiert: " + _installedRevision + ")" : ""));
		}

		private void MainForm_MouseDown(object sender, MouseEventArgs e)
		{
			// Fenster-Verschieben aktivieren
			_mouseDown = true;
			_lastLocation = e.Location;
		}

		private void MainForm_MouseMove(object sender, MouseEventArgs e)
		{
			// Fenster verschieben
			if(_mouseDown)
			{
				Location = new Point(Location.X - _lastLocation.X + e.X, Location.Y - _lastLocation.Y + e.Y);
				Update();
			}
		}

		private void MainForm_MouseUp(object sender, MouseEventArgs e)
		{
			// Fenster-Verschieben deaktivieren
			_mouseDown = false;
		}
		
		#endregion
	}
}
