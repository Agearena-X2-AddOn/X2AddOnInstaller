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
		const string SERVER_BASE_URI = "http://localhost/x2/";

		#endregion

		#region Variablen

		/// <summary>
		/// Die aktuell installierte Version.
		/// </summary>
		string _installedRevision = "";

		#endregion

		#region Funktionen

		/// <summary>
		/// Konstruktor.
		/// </summary>
		public MainForm()
		{
			// Steuerelemente laden
			InitializeComponent();
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

			// Sicherheitshalber Games-Verzeichnis anlegen
			string rootFolderName = aoe2Path + "Games\\";
			Directory.CreateDirectory(rootFolderName);

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
				Directory.CreateDirectory(rootFolderName + "SaveGame\\Multi");
				Directory.CreateDirectory(rootFolderName + "Scenario");
				Directory.CreateDirectory(rootFolderName + "Screenshots");
				Directory.CreateDirectory(rootFolderName + "Script.AI");
				Directory.CreateDirectory(rootFolderName + "Script.RM");
				Directory.CreateDirectory(rootFolderName + "Sound\\scenario");
				Directory.CreateDirectory(rootFolderName + "Sound\\stream");
			});

			// Language-DLLs lesen
			SetStatus("Installiere Language-DLLs...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// DLL-Datei speichern
				File.WriteAllBytes(rootFolderName + "Data\\" + currFileName, currFile);
			});

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
				// DAT-Datei speichern
				File.WriteAllBytes(aoe2Path + "age2_x1\\" + currFileName, currFile);
			});

			// Original-DRS-Dateien lesen
			SetStatus("Lade Original-DRS-Dateien...");
			Dictionary<string, DRSFile> drsFiles = new Dictionary<string, DRSFile>();
			drsFiles.Add("graphics", new DRSFile(aoe2Path + "DATA\\graphics.drs"));
			drsFiles.Add("interfac", new DRSFile(aoe2Path + "DATA\\interfac.drs"));

			// Ressourcen-Dateien lesen
			SetStatus("Schreibe neue Ressourcen in DRS-Dateien...");
			ReadArchiveBlock(buffer, (string currFileName, byte[] currFile) =>
			{
				// Datei umwandeln in ExternalFile-Objekt
				DRSFile.ExternalFile extF = DRSFile.ExternalFile.FromBinary(new RAMBuffer(currFile));

				// Daten in DRS schreiben
				drsFiles[extF.DRSFile].AddReplaceRessource(new RAMBuffer(extF.Data), (ushort)extF.FileID, DRSFile.ReverseString(extF.ResourceType));
			});

			// DRS-Dateien speichern
			SetStatus("Installiere neue DRS-Dateien...");
			drsFiles.ToList().ForEach(currDRS => currDRS.Value.WriteData(rootFolderName + "Data\\" + currDRS.Key + ".drs"));

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
		private void SetStatus(string text)
		{
			// Test setzen
			_statusLabel.Text = text;

			// Fenster aktualisieren
			Application.DoEvents();
		}

		#endregion

		#region Ereignishandler

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Age of Empires II-Schlüssel suchen
			string aoe2key = "SOFTWARE\\Microsoft\\Microsoft Games\\Age of Empires II: The Conquerors Expansion\\1.0";
			try
			{
				if(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(aoe2key).SubKeyCount >= 0)
					aoe2key = "HKEY_LOCAL_MACHINE\\" + aoe2key;
			}
			catch
			{
				try
				{
					if(Microsoft.Win32.Registry.CurrentUser.OpenSubKey(aoe2key).SubKeyCount >= 0)
						aoe2key = "HKEY_CURRENT_USER\\" + aoe2key;
				}
				catch
				{
					aoe2key = "";
					MessageBox.Show("Warnung: Kann AoE-II-Registrierungsschlüssel nicht finden. Bestimmen der installierten Version nicht möglich.\r\n" +
						"Bitte stellen Sie sicher, dass eine ordnungsgemäße \"Age of Empires II: The Conquerors\"-Installation vorliegt.",
						"Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}

			// Aktuelle Revision herausfinden
			if(aoe2key != "")
				_installedRevision = (string)Microsoft.Win32.Registry.GetValue(aoe2key, "X2AddOnRevision", _installedRevision);
		}

		private void _exitButton_Click(object sender, EventArgs e)
		{
			// Programm schließen
			this.Close();
		}

		private void _folderSelectButton_Click(object sender, EventArgs e)
		{
			// Dialog anzeigen
			if(_folderDialog.ShowDialog() == DialogResult.OK)
			{
				// Neuen Pfad merken
				_aoe2FolderTextBox.Text = _folderDialog.SelectedPath;
			}
		}

		private void _installButton_Click(object sender, EventArgs e)
		{
			// Neue Revision holen
			SetStatus("Hole Installationsarchiv vom Server...");
			byte[] revArchive = null;
			try
			{
				revArchive = new WebClient().DownloadData(SERVER_BASE_URI + "archive");
			}
			catch(WebException)
			{
				// Fehler
				MessageBox.Show("Fehler: Konnte das Installationsarchiv nicht herunterladen. Bitte überprüfen Sie Ihre Internetverbindung.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetStatus("Fehler.");
				return;
			}

			// Datei entpacken
			try
			{
				Unpack(revArchive, Path.GetFullPath(_aoe2FolderTextBox.Text) + "\\");
			}
			catch(Exception ex)
			{
				// Fehler
				MessageBox.Show("Fehler: Das Installationsarchiv konnte nicht ordnungsgemäß entpackt werden. Möglicherweise fehlen Schreibrechte oder die Datei ist beschädigt.\r\n\r\nFehlermeldung: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetStatus("Fehler.");
			}
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			// Neue Version vom Server holen
			SetStatus("Hole aktuelle Versionsnummer vom Server...");
			string _remoteRevision = "";
			try
			{
				_remoteRevision = new WebClient().DownloadString(SERVER_BASE_URI + "revision");
			}
			catch { }

			// Version gefunden?
			if(_remoteRevision == "" || _remoteRevision == _installedRevision)
				SetStatus("Keine neue Version verfügbar.");
			else
				SetStatus("Neue Version verfügbar: " + _remoteRevision + (_installedRevision == "" ? " (installiert: " + _installedRevision + ")" : ""));
		}

		#endregion
	}
}
