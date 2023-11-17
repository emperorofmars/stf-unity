
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using STF.IdComponents;

namespace STF.Serde
{
	public class STFBuffers
	{
		public int versionMajor = 0;
		public int versionMinor = 0;
		public string json;
		// id -> buffer
		public List<byte[]> buffers = new List<byte[]>();
	}

	// Parses binary STF files
	public class STFBufferImporter
	{
		public static string _MAGIC = "STF0";

		public static STFBuffers ParseBuffersFromBinary(byte[] byteArray)
		{
			var ret = new STFBuffers();

			var offset = 0;

			// Magic
			int magicLen = Encoding.UTF8.GetBytes(_MAGIC).Length;
			var magicUtf8 = new byte[magicLen];
			Buffer.BlockCopy(byteArray, 0, magicUtf8, 0, magicUtf8.Length * sizeof(byte));
			offset += magicUtf8.Length * sizeof(byte);

			var magic = Encoding.UTF8.GetString(magicUtf8);
			if(magic != _MAGIC)
				throw new Exception("Not an STF file, invalid magic number.");
			
			// Version
			ret.versionMajor = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);
			ret.versionMinor = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);
			
			// Header Length
			int headerLen = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);

			// Buffer Lengths
			var bufferLengths = new int[headerLen / sizeof(int)];
			for(int i = 0; i < headerLen / sizeof(int); i++)
			{
				bufferLengths[i] = BitConverter.ToInt32(byteArray, offset);
				offset += sizeof(int);
			}

			// Validity Check
			if(bufferLengths.Length < 1)
				throw new Exception("Invalid file: At least one buffer needed.");
			var totalLengthCheck = offset;
			foreach(var l in bufferLengths) totalLengthCheck += l;
			if(totalLengthCheck != byteArray.Length)
				throw new Exception("Invalid file: Size of buffers doesn't line up with total file size. ( calculated: " + totalLengthCheck + " | actual: " + byteArray.Length + " )");

			// First buffer, the Json definition
			ret.json = Encoding.UTF8.GetString(byteArray, offset, bufferLengths[0]);
			offset += bufferLengths[0];

			for(int i = 1; i < bufferLengths.Count(); i++)
			{
				var buffer = new byte[bufferLengths[i]];
				Buffer.BlockCopy(byteArray, offset, buffer, 0, bufferLengths[i]);
				offset += bufferLengths[i];
				ret.buffers.Add(buffer);
			}

			return ret;
		}

		public byte[] CreateBinaryFromBuffers(STFBuffers buffers)
		{
			byte[] magicUtf8 = Encoding.UTF8.GetBytes(STFBufferImporter._MAGIC);
			var headerSize = (buffers.buffers.Count + 1) * sizeof(int); // +1 for the json definition
			var bufferInfo = buffers.buffers.Select(buffer => buffer.Length).ToArray(); // lengths of all binary buffers
			byte[] jsonUtf8 = Encoding.UTF8.GetBytes(buffers.json);

			var arrayLen = magicUtf8.Length * sizeof(byte) + sizeof(int) * 2 + sizeof(int) + headerSize + jsonUtf8.Length * sizeof(byte);
			foreach(var bufferLen in bufferInfo) arrayLen += bufferLen;

			// handle endianness at some point maybe

			var byteArray = new byte[arrayLen];
			var offset = 0;

			// Magic
			{
				var size = magicUtf8.Length * sizeof(byte);
				Buffer.BlockCopy(magicUtf8, 0, byteArray, offset, size);
				offset += size;
			}

			// Version
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(0), 0, byteArray, offset, size); // Mayor
				offset += size;
				Buffer.BlockCopy(BitConverter.GetBytes(0), 0, byteArray, offset, size); // Minor
				offset += size;
			}

			// Header Length
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(headerSize), 0, byteArray, offset, size);
				offset += size;
			}

			// Header: array of buffer lengths
			// First the Json definition length
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(jsonUtf8.Length), 0, byteArray, offset, size);
				offset += size;
			}

			// Now the array of the lengths of all the binary buffers
			{
				var size = bufferInfo.Length * sizeof(int);
				Buffer.BlockCopy(bufferInfo, 0, byteArray, offset, size);
				offset += size;
			}

			// Json definition
			{
				var size = jsonUtf8.Length * sizeof(byte);
				Buffer.BlockCopy(jsonUtf8, 0, byteArray, offset, size);
				offset += size;
			}

			// Now all the buffers
			foreach(var buffer in buffers.buffers)
			{
				var size = buffer.Length * sizeof(byte);
				Buffer.BlockCopy(buffer, 0, byteArray, offset, size);
				offset += size;
			}

			return byteArray;
		}
	}
}

#endif
