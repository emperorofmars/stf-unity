
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace STF.Serialisation
{
	// Read and write binary STF files
	[Serializable]
	public class STFFile
	{
		public static string _MAGIC = "STF0";

		public int VersionMajor = 0;
		public int VersionMinor = 3;
		public string Json;
		public List<byte[]> Buffers = new();
		public string OriginalFileName;

		public STFFile(string Json, List<byte[]> Buffers)
		{
			this.Json = Json;
			this.Buffers = Buffers;
		}

		public STFFile(string ImportPath)
		{
			OriginalFileName = Path.GetFileNameWithoutExtension(ImportPath);
			Parse(File.ReadAllBytes(ImportPath));
		}

		public STFFile(byte[] ByteArray)
		{
			Parse(ByteArray);
		}

		private void Parse(byte[] ByteArray)
		{
			// I know its stupid having to convert it to int everywhere.
			// TODO find a better solution for binary parsing / serialisation.
			long offset = 0;

			// Magic
			int magicLen = Encoding.UTF8.GetBytes(STFFile._MAGIC).Length;
			var magicUtf8 = new byte[magicLen];
			Buffer.BlockCopy(ByteArray, 0, magicUtf8, 0, magicUtf8.Length * sizeof(byte));
			offset += magicUtf8.Length * sizeof(byte);

			var magic = Encoding.UTF8.GetString(magicUtf8);
			if(magic != _MAGIC)
				throw new Exception("Not an STF file, invalid magic number.");
			
			// Version
			this.VersionMajor = BitConverter.ToInt32(ByteArray, (int)offset);
			offset += sizeof(int);
			this.VersionMinor = BitConverter.ToInt32(ByteArray, (int)offset);
			offset += sizeof(int);

			// Json Definition Compression Format
			Buffer.BlockCopy(ByteArray, (int)offset, magicUtf8, 0, 4 * sizeof(byte));
			offset += 4 * sizeof(byte);
			var jsonCompression = Encoding.UTF8.GetString(magicUtf8); // placeholder for now
			if(jsonCompression != "none") throw new Exception("Unsupported compression format: " + jsonCompression);
			
			// Header Size
			int headerLen = BitConverter.ToInt32(ByteArray, (int)offset);
			offset += sizeof(int);

			// Buffer Lengths
			var bufferLengths = new long[headerLen / sizeof(long)];
			for(int i = 0; i < headerLen / sizeof(long); i++)
			{
				bufferLengths[i] = BitConverter.ToInt64(ByteArray, (int)offset);
				offset += sizeof(long);
			}

			// Validity Check
			if(bufferLengths.Length < 1)
				throw new Exception("Invalid file: At least one buffer needed.");
			var totalLengthCheck = offset;
			foreach(var l in bufferLengths) totalLengthCheck += l;
			if(totalLengthCheck != ByteArray.Length)
				throw new Exception("Invalid file: Size of buffers doesn't line up with total file size. ( calculated: " + totalLengthCheck + " | actual: " + ByteArray.Length + " )");

			// First buffer, the Json definition
			this.Json = Encoding.UTF8.GetString(ByteArray, (int)offset, (int)bufferLengths[0]);
			offset += bufferLengths[0];

			for(int i = 1; i < bufferLengths.Count(); i++)
			{
				var buffer = new byte[bufferLengths[i]];
				Buffer.BlockCopy(ByteArray, (int)offset, buffer, 0, (int)bufferLengths[i]);
				offset += bufferLengths[i];
				this.Buffers.Add(buffer);
			}
		}
		
		public byte[] CreateBinaryFromBuffers()
		{
			byte[] magicUtf8 = Encoding.UTF8.GetBytes(STFFile._MAGIC);
			long headerSize = (this.Buffers.Count + 1) * sizeof(long); // +1 for the json definition
			long[] bufferInfo = new long[Buffers.Count()];
			for(int i = 0; i < Buffers.Count(); i++) bufferInfo[i] = Buffers[i].Length; // lengths of all binary buffers
			byte[] jsonUtf8 = Encoding.UTF8.GetBytes(this.Json);
			byte[] jsonCompression = Encoding.UTF8.GetBytes("none"); // compression format of the json definition, this is a placeholder for now

			// magic; version major + minor; compression format for json definition; number of buffers including the json definition; length of the json definition
			var arrayLen = magicUtf8.Length * sizeof(byte) + sizeof(int) * 2 + sizeof(byte) * 4 + sizeof(int) + headerSize + jsonUtf8.Length * sizeof(byte);
			// add the lengths of all further buffers
			foreach(var bufferLen in bufferInfo) arrayLen += bufferLen;

			var byteArray = new byte[arrayLen];
			long offset = 0;

			// Magic
			{
				var size = magicUtf8.Length * sizeof(byte);
				Buffer.BlockCopy(magicUtf8, 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Version
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(this.VersionMajor), 0, byteArray, (int)offset, size);
				offset += size;
				Buffer.BlockCopy(BitConverter.GetBytes(this.VersionMinor), 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Json definition compression
			{
				var size = jsonCompression.Length * sizeof(byte);
				Buffer.BlockCopy(jsonCompression, 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Header Size
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(headerSize), 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Header: array of buffer lengths
			// First the Json definition length
			{
				var size = sizeof(long);
				Buffer.BlockCopy(BitConverter.GetBytes((long)jsonUtf8.Length), 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Now the array of the lengths of all the binary buffers
			{
				var size = bufferInfo.Length * sizeof(long);
				Buffer.BlockCopy(bufferInfo, 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Json definition
			{
				var size = jsonUtf8.Length * sizeof(byte);
				Buffer.BlockCopy(jsonUtf8, 0, byteArray, (int)offset, size);
				offset += size;
			}

			// Now all the buffers
			foreach(var buffer in this.Buffers)
			{
				var size = buffer.Length * sizeof(byte);
				Buffer.BlockCopy(buffer, 0, byteArray, (int)offset, size);
				offset += size;
			}

			return byteArray;
		}
	}
}
