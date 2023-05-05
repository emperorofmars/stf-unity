
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFMeshExporter : ASTFResourceExporter
	{
		override public List<GameObject> gatherNodes(UnityEngine.Object resource)
		{
			return null;
		}

		override public List<UnityEngine.Object> gatherResources(UnityEngine.Object resource)
		{
			return null;
		}

		override public JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var ret = new JObject();
			var mesh = (Mesh)resource;

			ret.Add("type", STFMeshImporter._TYPE);
			ret.Add("name", mesh.name);

			var bufferWidth = 3;
			var numUVs = 0;

			if(mesh.HasVertexAttribute(VertexAttribute.Normal)) bufferWidth += 3;
			if(mesh.HasVertexAttribute(VertexAttribute.Tangent)) bufferWidth += 4;
			if(mesh.HasVertexAttribute(VertexAttribute.Color)) bufferWidth += 4;
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord0)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord1)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord2)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord3)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord4)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord5)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord6)) { bufferWidth += 2; numUVs++; }
			if(mesh.HasVertexAttribute(VertexAttribute.TexCoord7)) { bufferWidth += 2; numUVs++; }

			var offset = 0;
			var vertexBuffer = new float[mesh.vertexCount * bufferWidth];
			//var buffer = new JArray();
			
			{
				ret.Add("vertex_count", mesh.vertexCount);
				var vertices = new List<Vector3>();
				mesh.GetVertices(vertices);
				for(int i = 0; i < vertices.Count; i++)
				{
					vertexBuffer[i * bufferWidth] = vertices[i].x;
					vertexBuffer[i * bufferWidth + 1] = vertices[i].y;
					vertexBuffer[i * bufferWidth + 2] = vertices[i].z;
				}
				offset += 3;
			}
			if(mesh.HasVertexAttribute(VertexAttribute.Normal))
			{
				ret.Add("normal", true);
				var normals = new List<Vector3>();
				mesh.GetNormals(normals);
				for(int i = 0; i < normals.Count; i++)
				{
					vertexBuffer[i * bufferWidth + offset] = normals[i].x;
					vertexBuffer[i * bufferWidth + offset + 1] = normals[i].y;
					vertexBuffer[i * bufferWidth + offset + 2] = normals[i].z;
				}
				offset += 3;
			}
			if(mesh.HasVertexAttribute(VertexAttribute.Tangent))
			{
				ret.Add("tangent", true);
				var tangents = new List<Vector4>();
				mesh.GetTangents(tangents);
				for(int i = 0; i < tangents.Count; i++)
				{
					vertexBuffer[i * bufferWidth + offset] = tangents[i].x;
					vertexBuffer[i * bufferWidth + offset + 1] = tangents[i].y;
					vertexBuffer[i * bufferWidth + offset + 2] = tangents[i].z;
					vertexBuffer[i * bufferWidth + offset + 3] = tangents[i].w;
				}
				offset += 4;
			}
			if(mesh.HasVertexAttribute(VertexAttribute.Color))
			{
				ret.Add("color", true);
				var colors = new List<Color>();
				mesh.GetColors(colors);
				for(int i = 0; i < colors.Count; i++)
				{
					vertexBuffer[i * bufferWidth + offset] = colors[i].r;
					vertexBuffer[i * bufferWidth + offset + 1] = colors[i].g;
					vertexBuffer[i * bufferWidth + offset + 2] = colors[i].b;
					vertexBuffer[i * bufferWidth + offset + 2] = colors[i].a;
				}
				offset += 4;
			}
			ret.Add("uvs", numUVs);
			for(int uvIdx = 0; uvIdx < numUVs; uvIdx++)
			{
				var uvs = new List<Vector2>();
				mesh.GetUVs(uvIdx, uvs);
				for(int i = 0; i < uvs.Count; i++)
				{
					vertexBuffer[i * bufferWidth + offset] = uvs[i].x;
					vertexBuffer[i * bufferWidth + offset + 1] = uvs[i].y;
				}
				offset += 2;
			}


			var weightsPerVertex = mesh.GetBonesPerVertex();
			var weights = mesh.GetAllBoneWeights();
			int pos = 0;
			for(int vertexIdx = 0; vertexIdx < weightsPerVertex.Length; vertexIdx++)
			{
				var numWeights = weightsPerVertex[vertexIdx];
				for(int weightPos = pos; weightPos < pos + numWeights; weightPos++)
				{

				}
				pos += numWeights;
			}

			var primitives = new JArray();
			var indexBuffer = new List<int>();
			for(int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; subMeshIdx++)
			{
				var primitive = new JObject();
				primitives.Add(primitive);

				if(mesh.GetSubMesh(subMeshIdx).topology == MeshTopology.Triangles) primitive.Add("topology", "tris");
				else if(mesh.GetSubMesh(subMeshIdx).topology == MeshTopology.Quads) primitive.Add("topology", "quads");

				primitive.Add("indices_pos", indexBuffer.Count);
				primitive.Add("indices_len", mesh.GetIndexCount(subMeshIdx));

				indexBuffer.AddRange(mesh.GetIndices(subMeshIdx));
				
			}
			ret.Add("primitives", primitives);

			// weights

			// blendshapes


			var byteArray = new byte[vertexBuffer.Length * sizeof(float) + indexBuffer.Count * sizeof(int)];

			Buffer.BlockCopy(vertexBuffer, 0, byteArray, 0, vertexBuffer.Length * sizeof(float));
			Buffer.BlockCopy(indexBuffer.ToArray(), 0, byteArray, vertexBuffer.Length * sizeof(float), indexBuffer.Count * sizeof(int));

			var bufferId = state.RegisterBuffer(byteArray);
			ret.Add("buffer", bufferId);
			
			return ret;
		}
	}

	public class STFMeshImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.mesh";
		override public UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id)
		{
			var ret = new Mesh();
			ret.name = (string)json["name"];

			var bufferWidth = 3;
			var numUVs = 0;
			var vertexCount = (int)json["vertex_count"];

			if(json["normal"] != null) bufferWidth += 3;
			if(json["tangent"] != null) bufferWidth += 4;
			if(json["color"] != null) bufferWidth += 4;
			if(json["uvs"] != null) { bufferWidth += 2 * (int)json["uvs"]; numUVs = (int)json["uvs"]; }

			var arrayBuffer = state.GetBuffer((string)json["buffer"]);

			var vertexbuffer = new float[vertexCount * bufferWidth];
			Buffer.BlockCopy(arrayBuffer, 0, vertexbuffer, 0, vertexCount * bufferWidth * sizeof(float));

			var offset = 0;

			{
				var vertices = new List<Vector3>();
				for(int i = 0; i < vertexCount; i++)
				{
					vertices.Add(new Vector3(vertexbuffer[i * bufferWidth], vertexbuffer[i * bufferWidth + 1], vertexbuffer[i * bufferWidth + 2]));
				}
				ret.SetVertices(vertices);
				offset += 3;
			}
			if((bool)json["normal"] == true)
			{
				var normals = new List<Vector3>();
				for(int i = 0; i < vertexCount; i++)
				{
					normals.Add(new Vector3(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1], vertexbuffer[i * bufferWidth + offset + 2]));
				}
				ret.SetNormals(normals);
				offset += 3;
			}
			if((bool)json["tangent"] == true)
			{
				var tangents = new List<Vector4>();
				for(int i = 0; i < vertexCount; i++)
				{
					tangents.Add(new Vector4(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1], vertexbuffer[i * bufferWidth + offset + 2], vertexbuffer[i * bufferWidth + offset + 3]));
				}
				ret.SetTangents(tangents);
				offset += 4;
			}
			if((bool)json["color"] == true)
			{
				var colors = new List<Color>();
				for(int i = 0; i < vertexCount; i++)
				{
					colors.Add(new Color(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1], vertexbuffer[i * bufferWidth + offset + 2], vertexbuffer[i * bufferWidth + offset + 3]));
				}
				ret.SetColors(colors);
				offset += 4;
			}
			for(int uvIdx = 0; uvIdx < numUVs; uvIdx++)
			{
				if((bool)json["normal"] == true)
				{
					var uvs = new List<Vector3>();
					for(int i = 0; i < vertexCount; i++)
					{
						uvs.Add(new Vector2(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1]));
					}
					ret.SetUVs(uvIdx, uvs);
					offset += 2;
				}
			}

			var indicesPosCounted = 0;
			var max = 0;


			var primitives = (JArray)json["primitives"];
			ret.subMeshCount = primitives.Count;
			for(int subMeshIdx = 0; subMeshIdx < primitives.Count; subMeshIdx++)
			{
				var primitive = (JObject)primitives[subMeshIdx];
				var indicesPos = (int)primitive["indices_pos"];
				var indicesLen = (int)primitive["indices_len"];

				var indexBuffer = new int[indicesLen];
				Buffer.BlockCopy(arrayBuffer, indicesPosCounted * sizeof(int) + vertexCount * bufferWidth * sizeof(float), indexBuffer, 0, indicesLen * sizeof(int));

				foreach(var index in indexBuffer) if(index > max) max = index;

				var topology = ((string)primitive["topology"]) == "tris" ? MeshTopology.Triangles : MeshTopology.Quads;
				ret.SetIndices(indexBuffer, topology, subMeshIdx);

				indicesPosCounted += indicesLen;
			}
			ret.UploadMeshData(false);

			state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {name = ret.name, resource = ret, uuid = id, external = false });

			return ret;
		}
	}
}
