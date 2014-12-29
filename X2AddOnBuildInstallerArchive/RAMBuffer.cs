using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IORAMHelper
{
		/// <summary>
		/// Definiert einen Puffer, der ganze Dateien einlesen und byteweise durchgehen kann.
		/// </summary>
		public class RAMBuffer : ICloneable
		{
			/// <summary>
			/// Die enthaltenen Bytedaten.
			/// </summary>
			private List<byte> _data;

			/// <summary>
			/// Die aktuelle Position im Puffer.
			/// </summary>
			private int _pos = 0;


			/// <summary>
			/// Erstellt ein neues RAMBuffer-Objekt.
			/// </summary>
			public RAMBuffer()
			{
				// Datenarray initialisieren
				_data = new List<byte>();
			}

			/// <summary>
			/// Erstellt ein neues RAMBuffer-Objekt aus den übergebenen Daten.
			/// </summary>
			/// <param name="data">Die einzufügenden Daten.</param>
			public RAMBuffer(byte[] data)
			{
				// Datenarray mit übergebenen Daten anlegen
				_data = new List<byte>(data);
			}

			/// <summary>
			/// Erstellt ein neues RAMBuffer-Objekt aus der angegebenen Datei.
			/// </summary>
			/// <param name="filename">Die zu ladende Datei.</param>
			public RAMBuffer(string filename)
			{
				// Puffer erstellen
				byte[] buffer;

				// Datei laden
				using(BinaryReader r = new BinaryReader(File.Open(filename, FileMode.Open)))
				{
					// Puffer initialisieren
					buffer = new byte[r.BaseStream.Length];

					// Puffer mit sämtlichen Dateidaten füllen
					r.Read(buffer, 0, (int)r.BaseStream.Length);

					// Stream schließen
					r.Close();
				}

				// Datenarray mit Pufferinhalt initialisieren
				_data = new List<byte>(buffer);
			}

			/// <summary>
			/// Liest ab der aktuellen Position die angegebene Menge von Bytes in ein angegebenes Byte-Array und gibt die tatsächliche Zahl der gelesenen Bytes zurück.
			/// </summary>
			/// <param name="buffer">Der Puffer, in den die Daten gelesen werden sollen (muss nicht initialisiert sein).</param>
			/// <param name="length">Die Länge der zu lesenden Daten.</param>
			/// <returns></returns>
			public int Read(out byte[] buffer, int length)
			{
				// Länge neu berechnen und ggf. verkleinern, falls diese über das Array hinausgeht
				int length2;
				if(length == 0 || _pos == _data.Count)
				{
					// Es können keine Daten gelesen werden, Array sicherheitshalber leer initialisieren (Null-Verweis-Fehler)
					buffer = new byte[0];

					// Nix zurückgeben
					return 0;
				}
				else if(_pos + length > _data.Count)
				{
					// Es dürfen nicht zu viele Daten gelesen werden
					length2 = _data.Count - _pos;
				}
				else
				{
					// Die Länge ist akzeptabel
					length2 = length;
				}

				// Puffer sicherheitshalber initialisieren
				buffer = new byte[length2];

				// Daten in Puffer kopieren
				_data.CopyTo(_pos, buffer, 0, length2);

				// Aktuelle Pufferposition erhöhen
				_pos += length2;

				// Datenlänge zurückgeben
				return length2;
			}

			/// <summary>
			/// Schreibt die angegebenen Daten an der aktuellen Position in den Puffer.
			/// </summary>
			/// <param name="data">Die zu schreibenden Daten.</param>
			/// <param name="overWrite">Legt fest, ob die Daten im Puffer mit den neuen Daten überschrieben werden sollen.</param>
			public void Write(byte[] data, bool overWrite = true)
			{
				// Datenlänge merken
				int len = data.Length;

				// Soll überschrieben werden?
				if(overWrite)
				{
					// Es muss ein bestimmter Bereich gelöscht werden; der Löschbereich darf die Puffergrenze nicht überschreiten
					if(_pos + len < _data.Count - 1)
					{
						// Alles OK, Bereich löschen
						_data.RemoveRange(_pos, len);
					}
					else
					{
						// Nur bis zum Listenende löschen
						_data.RemoveRange(_pos, _data.Count - _pos);
					}
				}

				// Daten einfügen
				_data.InsertRange(_pos, data);

				// Die Position um die Datenlänge erhöhen
				_pos += len;
			}

			/// <summary>
			/// Löscht alle enthaltenen Daten und setzt alle Statusvariablen zurück.
			/// </summary>
			public void Clear()
			{
				// Datenarray löschen
				_data.Clear();

				// Position zurücksetzen
				_pos = 0;
			}

			/// <summary>
			/// Schreibt alle Pufferinhalte in die angegebene Datei (bestehende Dateien werden überschrieben).
			/// </summary>
			/// <param name="filename">Die Zieldatei für die Pufferinhalte.</param>
			public void Save(string filename)
			{
				// Datei öffnen
				using(BinaryWriter w = new BinaryWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None)))
				{
					// Daten schreiben
					w.Write((byte[])_data.ToArray());

					// Stream schließen
					w.Close();
				}
			}

			/// <summary>
			/// Erstellt eine Kopie des aktuellen Objekts und gibt diese zurück.
			/// </summary>
			/// <returns></returns>
			public object Clone()
			{
				return this.MemberwiseClone();
			}

			#region Konvertierende Hilfsfunktionen

			#region Lesen
			/// <summary>
			/// Liest ein Byte-Array der angegebenen Länge ab der aktuellen Position aus dem Puffer.
			/// </summary>
			/// <param name="len">Die Länge des zu lesenden Byte-Arrays.</param>
			/// <returns></returns>
			public byte[] ReadByteArray(int len)
			{
				// Puffer-Byte-Array
				byte[] buffer;

				// Lesen
				Read(out buffer, len);

				// Byte-Array zurückgeben
				return buffer;
			}

			/// <summary>
			/// Liest einen Byte-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public byte ReadByte()
			{
				// Wert zurückgeben
				return ReadByteArray(1)[0];
			}

			/// <summary>
			/// Liest einen Integer-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public int ReadInteger()
			{
				// Wert zurückgeben
				return BitConverter.ToInt32(ReadByteArray(4), 0);
			}

			/// <summary>
			/// Liest einen vorzeichenlosen Integer-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public uint ReadUInteger()
			{
				// Wert zurückgeben
				return BitConverter.ToUInt32(ReadByteArray(4), 0);
			}

			/// <summary>
			/// Liest einen Short-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public short ReadShort()
			{
				// Wert zurückgeben
				return BitConverter.ToInt16(ReadByteArray(2), 0);
			}

			/// <summary>
			/// Liest einen vorzeichenlosen Short-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public ushort ReadUShort()
			{
				// Wert zurückgeben
				return BitConverter.ToUInt16(ReadByteArray(2), 0);
			}

			/// <summary>
			/// Liest einen Long-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public long ReadLong()
			{
				// Wert zurückgeben
				return BitConverter.ToInt64(ReadByteArray(8), 0);
			}

			/// <summary>
			/// Liest einen vorzeichenlosen Long-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public ulong ReadULong()
			{
				// Wert zurückgeben
				return BitConverter.ToUInt64(ReadByteArray(8), 0);
			}

			/// <summary>
			/// Liest einen Float-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public float ReadFloat()
			{
				// Wert zurückgeben
				return BitConverter.ToSingle(ReadByteArray(4), 0);
			}

			/// <summary>
			/// Liest einen Double-Wert aus dem Puffer.
			/// </summary>
			/// <returns></returns>
			public double ReadDouble()
			{
				// Wert zurückgeben
				return BitConverter.ToDouble(ReadByteArray(8), 0);
			}

			/// <summary>
			/// Liest eine ANSI-Zeichenkette angegebener Länge aus dem Puffer.
			/// </summary>
			/// <param name="length">Die Länge der zu lesenden Zeichenkette.</param>
			/// <returns></returns>
			public string ReadString(int length)
			{
				// Wert zurückgeben
				return System.Text.Encoding.Default.GetString(ReadByteArray(length));
			}
			#endregion

			#region Schreiben
			/// <summary>
			/// Schreibt einen Byte-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteByte(byte value)
			{
				// Wert schreiben
				Write(new byte[] { value });
			}

			/// <summary>
			/// Schreibt einen Float-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteFloat(float value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen Integer-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteInteger(int value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen vorzeichenlosen Integer-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteUInteger(uint value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen Short-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteShort(short value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen vorzeichenlosen Short-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteUShort(ushort value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen Long-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteLong(long value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen vorzeichenlosen Long-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteULong(ulong value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen Float-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteLong(float value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt einen Double-Wert in den Puffer.
			/// </summary>
			/// <param name="value">Der zu schreibende Wert.</param>
			public void WriteLong(double value)
			{
				// Wert schreiben
				Write(BitConverter.GetBytes(value));
			}

			/// <summary>
			/// Schreibt eine ANSI-Zeichenkette in den Puffer.
			/// </summary>
			/// <param name="value">Die zu schreibende Zeichenkette.</param>
			public void WriteString(string value)
			{
				// Zeichenkette schreiben
				Write(System.Text.Encoding.Default.GetBytes(value));
			}
			#endregion

			#endregion

			#region Eigenschaften

			/// <summary>
			/// Ruft die aktuelle Pufferposition ab oder legt diese fest.
			/// </summary>
			public int Position
			{
				get
				{
					// Position zurückgeben
					return _pos;
				}
				set
				{
					// Position überprüfen und ggf. Fehler auslösen
					if(value <= _data.Count)
						_pos = value;
					else
						throw new ArgumentException("Die angegebene Position liegt nicht innerhalb der Daten!");
				}
			}

			/// <summary>
			/// Ruft die aktuelle Puffergröße ab.
			/// </summary>
			public int Length
			{
				get
				{
					// Pufferlänge zurückgeben
					return _data.Count;
				}
			}

			#endregion
		}
}