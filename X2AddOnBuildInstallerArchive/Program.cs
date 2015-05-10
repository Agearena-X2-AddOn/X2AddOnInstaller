using IORAMHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace X2AddOnBuildInstallerArchive
{
	/// <summary>
	/// Dieses Programm erstellt das Installer-Daten-Archiv.
	/// </summary>
	class Program
	{
		#region Konstanten

		/// <summary>
		/// Der Pfad des erstellten Archivs.
		/// </summary>
		const string OUTPUT_PATH = "..\\..\\..\\X2AddOnInstaller\\bin\\Release\\X2AddOnInstaller.lzma";

		#endregion

		#region LZMA/7zip-Konstanten

		const int Z_DICT_SIZE = 1 << 26; // 64 MB
		const int Z_POS_STATE_BITS = 2;
		const int Z_LIT_CONTEXT_BITS = 3;
		const int Z_LIT_POS_BITS = 0;
		const int Z_ALGORITHM = 2;
		const int Z_NUM_FAST_BYTES = 128;
		const string Z_MF = "bt4";
		const bool Z_EOS = false;

		#endregion

		#region Funktionen

		/// <summary>
		/// Programmeinstiegspunkt.
		/// </summary>
		/// <param name="args">Die Kommandozeilenparameter.</param>
		static void Main(string[] args)
		{
			// Archiv erstellen
			RAMBuffer buffer = new RAMBuffer();

			// XML-Spezifikationen schreiben
			Console.WriteLine("Schreibe XML-Spezifikationen...");
			WriteDirectoryToArchive(buffer, "data/xml");

			// Language-DLLs schreiben
			Console.WriteLine("Schreibe Language-DLLs...");
			WriteDirectoryToArchive(buffer, "data/dll");

			// Datendateien schreiben
			Console.WriteLine("Schreibe Datendateien...");
			WriteDirectoryToArchive(buffer, "data/dat");

			// Ausführbare Dateien schreiben
			Console.WriteLine("Schreibe ausfuehrbare Dateien...");
			WriteDirectoryToArchive(buffer, "data/exe");

			// Ressourcen-Dateien schreiben
			Console.WriteLine("Schreibe Ressourcen-Dateien...");
			WriteDirectoryToArchive(buffer, "data/res");

			// Eingabestream erstellen
			Console.WriteLine("Erstelle Eingabestream...");
			buffer.Position = 0;
			byte[] inBytes;
			buffer.Read(out inBytes, buffer.Length);
			Stream streamIn = new MemoryStream(inBytes);
			Console.WriteLine("Archivgroesse (unkomprimiert): " + (inBytes.Length / 1024) + " KB");

			// Ausgabearchiv erstellen
			Console.WriteLine("Erstelle Ausgabestream...");
			Stream streamOut = new FileStream(OUTPUT_PATH, FileMode.Create, FileAccess.Write);

			// LZMA-Encoder konfigurieren
			Console.WriteLine("Konfiguriere Encoder...");
			SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
			{
				SevenZip.CoderPropID[] codPropIDs = 
				{
					SevenZip.CoderPropID.DictionarySize,
					SevenZip.CoderPropID.PosStateBits,
					SevenZip.CoderPropID.LitContextBits,
					SevenZip.CoderPropID.LitPosBits,
					SevenZip.CoderPropID.Algorithm,
					SevenZip.CoderPropID.NumFastBytes,
					SevenZip.CoderPropID.MatchFinder,
					SevenZip.CoderPropID.EndMarker
				};
				object[] codProps = 
				{
					Z_DICT_SIZE,
					Z_POS_STATE_BITS,
					Z_LIT_CONTEXT_BITS,
					Z_LIT_POS_BITS,
					Z_ALGORITHM,
					Z_NUM_FAST_BYTES,
					Z_MF,
					Z_EOS
				};
				encoder.SetCoderProperties(codPropIDs, codProps);
				encoder.WriteCoderProperties(streamOut);
			}

			// Dateigröße speichern
			Console.WriteLine("Schreibe Dateigröße...");
			Int64 fileSize = streamIn.Length;
			for(int i = 0; i < 8; i++)
				streamOut.WriteByte((Byte)(fileSize >> (8 * i)));

			// Komprimieren und speichern
			Console.WriteLine("Komprimiere Daten...");
			ZProgressInfo proginfo = new ZProgressInfo();
			encoder.Code(streamIn, streamOut, -1, -1, proginfo);
			Console.WriteLine();

			// Streams schließen
			Console.WriteLine("Schließe Streams...");
			streamOut.Close();
			streamIn.Close();

			// Neues Archiv auf den Server laden
			Console.WriteLine("Lade Archiv hoch...");
			string revision = "rev" + DateTime.Now.ToString("yyyyMMdd-HHmm");
			var pushProc = new Process();
			pushProc.StartInfo = new ProcessStartInfo("PushArchive.exe", "\"" + Path.GetFullPath(OUTPUT_PATH) + "\" " + revision)
			{
				UseShellExecute = false
			};
			pushProc.Start();
			pushProc.WaitForExit();

			// Fertig
			Console.WriteLine("Zum Beenden ENTER drücken...");
			Console.ReadLine();
		}

		/// <summary>
		/// Schreibt das gegebene Verzeichnis in den Archiv-Puffer.
		/// </summary>
		/// <param name="buffer">Der Archiv-Puffer.</param>
		/// <param name="directory">Das zu schreibende Verzeichnis.</param>
		private static void WriteDirectoryToArchive(RAMBuffer buffer, string directory)
		{
			// Dateinamenliste abrufen
			string[] filenames = Directory.GetFiles(directory);

			// Anzahl der Dateien schreiben
			buffer.WriteInteger(filenames.Length);

			// Alle Dateien durchlaufen
			byte[] file;
			string shortFilename;
			int i = 1;
			foreach(string filename in filenames)
			{
				// Statusanzeige
				Console.Write("\r{0} von {1}", i, filenames.Length);

				// Dateinamen schreiben
				shortFilename = Path.GetFileName(filename);
				buffer.WriteInteger(shortFilename.Length);
				buffer.WriteString(shortFilename);

				// Datei einlesen
				file = File.ReadAllBytes(filename);

				// Dateilänge schreiben
				buffer.WriteInteger(file.Length);

				// Datei schreiben
				buffer.Write(file);

				// Nächster
				++i;
			}

			// Fertig
			Console.WriteLine();
		}

		#endregion

		#region Strukturen

		class ZProgressInfo : SevenZip.ICodeProgress
		{
			/// <summary>
			/// Die Startzeit.
			/// </summary>
			public System.DateTime _startTime;

			/// <summary>
			/// Erstellt ein neues ZProgressInfo-Objekt.
			/// </summary>
			public ZProgressInfo()
			{
				// Startzeit merken
				_startTime = DateTime.UtcNow;
			}

			/// <summary>
			/// Aktualisiert den Fortschritt.
			/// </summary>
			/// <param name="inSize">Die Größe des bereits verarbeiteten Eingabestreams.</param>
			/// <param name="outSize">Die Größe des bereits verarbeiteten Ausgabestreams.</param>
			public void SetProgress(long inSize, long outSize)
			{
				// Fortschritt anzeigen
				TimeSpan t = DateTime.UtcNow - _startTime;
				Console.Write("\r Verarbeitet: {0,5} KB -> {1,5} KB    Kompressionsrate: {2:p}    Vergangene Zeit: {3,2:D2}:{4,2:D2}:{5,2:D2}", inSize / 1024, outSize / 1024, (float)outSize / (float)inSize, t.Hours, t.Minutes, t.Seconds);
			}
		}

		#endregion
	}
}
