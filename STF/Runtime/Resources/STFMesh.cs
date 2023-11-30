
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace STF.Serialisation
{
	public class STFMesh : ASTFResource
	{
		public string OriginalBufferId;
		public string ArmatureId;
	}

	public class STFMeshExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var ret = new JObject();
			var mesh = (Mesh)Resource;
			var meta = State.LoadMeta<STFMesh>(Resource);
			
			var usedResources = new JArray();

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
			var weightLength = 0;
			byte[] weightBuffer = null;
			if(mesh.HasVertexAttribute(VertexAttribute.BlendIndices))
			{
				ret.Add("skinned", true);
				ret.Add("armature", meta.ArmatureId);
				usedResources.Add(meta.ArmatureId);


				/*state.AddTask(new Task(() => {
					ret.Add("skinned", true);
					var resourceContext = state.GetResourceContext(mesh);
					if(resourceContext != null && resourceContext.ContainsKey("armature"))
					{
						ret.Add("armature", state.GetResourceId((STFArmature)resourceContext["armature"]));
					}
					else if(resourceContext != null && resourceContext.ContainsKey("armature_id"))
					{
						ret.Add("armature", (string)resourceContext["armature_id"]);
					}
					else
					{
						throw new Exception("No armature resource context for mesh :(");
					}
				}));*/

				foreach(var num in mesh.GetBonesPerVertex()) weightLength += num;
				weightBuffer = new byte[weightLength * (sizeof(float) + sizeof(int))];
				var unityWeights = mesh.GetAllBoneWeights();
				for(int weightIdx = 0; weightIdx < weightLength; weightIdx++)
				{
					Buffer.BlockCopy(BitConverter.GetBytes(unityWeights[weightIdx].boneIndex), 0, weightBuffer, weightIdx * (sizeof(float) + sizeof(int)), sizeof(int));
					Buffer.BlockCopy(BitConverter.GetBytes(unityWeights[weightIdx].weight), 0, weightBuffer, weightIdx * (sizeof(float) + sizeof(int)) + sizeof(int), sizeof(float));
				}
			}
			else
			{
				ret.Add("skinned", false);
			}

			// blendshapes
			var blendshapeBuffers = new List<List<byte>>();
			if(mesh.blendShapeCount > 0)
			{
				ret.Add("blendshape_count", mesh.blendShapeCount);
				var blendshapes = new JArray();
				for(int i = 0; i < mesh.blendShapeCount; i++)
				{
					var blendshapeJson = new JObject();
					blendshapeJson.Add("name",  mesh.GetBlendShapeName(i));
					var blendshapeVertecies = new Vector3[mesh.vertexCount];
					var blendshapeNormals = new Vector3[mesh.vertexCount];
					var blendshapeTangents = new Vector3[mesh.vertexCount];

					var indicesLen = 0;
					var blendshapeBuffer = new List<byte>();

					mesh.GetBlendShapeFrameVertices(i, 0, blendshapeVertecies, blendshapeNormals, blendshapeTangents);
					var hasNormals = false;
					var hasTangents = false;
					for(int vertexIdx = 0; vertexIdx < mesh.vertexCount; vertexIdx++)
					{
						if(blendshapeNormals[vertexIdx].magnitude > 0)
						{
							hasNormals = true;
							break;
						}
					}
					for(int vertexIdx = 0; vertexIdx < mesh.vertexCount; vertexIdx++)
					{
						if(blendshapeTangents[vertexIdx].magnitude > 0)
						{
							hasTangents = true;
							break;
						}
					}

					for(int vertexIdx = 0; vertexIdx < mesh.vertexCount; vertexIdx++)
					{
						if(blendshapeVertecies[vertexIdx] != null && blendshapeVertecies[vertexIdx].magnitude > 0)
						{
							indicesLen++;
							blendshapeBuffer.AddRange(BitConverter.GetBytes(vertexIdx));
							blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeVertecies[vertexIdx].x));
							blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeVertecies[vertexIdx].y));
							blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeVertecies[vertexIdx].z));
							if(hasNormals)
							{
								blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeNormals[vertexIdx].x));
								blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeNormals[vertexIdx].y));
								blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeNormals[vertexIdx].z));
							}
							if(hasTangents)
							{
								blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeTangents[vertexIdx].x));
								blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeTangents[vertexIdx].y));
								blendshapeBuffer.AddRange(BitConverter.GetBytes(blendshapeTangents[vertexIdx].z));
							}
						}
					}
					blendshapeBuffers.Add(blendshapeBuffer);
					blendshapeJson.Add("indices_len", indicesLen);
					blendshapeJson.Add("normal", hasNormals);
					blendshapeJson.Add("tangent", hasTangents);
					blendshapes.Add(blendshapeJson);
				}
				ret.Add("blendshapes", blendshapes);
			}

			var vertexBufferLength = vertexBuffer.Length * sizeof(float);
			var indexBufferLength = indexBuffer.Count * sizeof(int);
			var weightLengthBufferLength = mesh.vertexCount * sizeof(byte);
			var weightBufferLength = weightLength * (sizeof(float) + sizeof(int));
			long blendshapeBufferLength = 0;
			foreach(var buffer in blendshapeBuffers) blendshapeBufferLength += buffer.Count;

			var byteArray = new byte[vertexBufferLength + indexBufferLength + weightLengthBufferLength + weightBufferLength + blendshapeBufferLength];

			Buffer.BlockCopy(vertexBuffer, 0, byteArray, 0, vertexBufferLength);
			Buffer.BlockCopy(indexBuffer.ToArray(), 0, byteArray, vertexBufferLength, indexBufferLength);
			if(weightLength > 0)
			{
				Buffer.BlockCopy(mesh.GetBonesPerVertex().ToArray(), 0, byteArray, vertexBufferLength + indexBufferLength, weightLengthBufferLength);
				Buffer.BlockCopy(weightBuffer, 0, byteArray, vertexBufferLength + indexBufferLength + weightLengthBufferLength, weightBufferLength);
			}
			if(blendshapeBuffers.Count > 0)
			{
				var blendshapePosition = vertexBufferLength + indexBufferLength + weightLengthBufferLength + weightBufferLength;
				var blendshapeOffset = 0;
				foreach(var buffer in blendshapeBuffers)
				{
					Buffer.BlockCopy(buffer.ToArray(), 0, byteArray, blendshapePosition + blendshapeOffset, buffer.Count);
					blendshapeOffset += buffer.Count;
				}
			}

			var bufferId = State.AddBuffer(byteArray, meta.OriginalBufferId);
			ret.Add("buffer", bufferId);
			
			ret.Add("used_resources", usedResources);
			ret.Add("used_buffers", new JArray() {bufferId});
			return State.AddResource(mesh, ret, meta.Id);
		}
	}

	public class STFMeshImporter : ISTFResourceImporter
	{
		public const string _TYPE = "STF.mesh";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var mesh = new Mesh();
			mesh.name = (string)Json["name"];
			
			var meta = ScriptableObject.CreateInstance<STFMesh>();
			meta.ResourceLocation = Path.Combine(State.TargetLocation, STFConstants.ResourceDirectoryName, mesh.name + "_" + Id + ".mesh");
			meta.OriginalBufferId = (string)Json["buffer"];
			meta.Name = mesh.name;
			meta.ArmatureId = (string)Json["armature"];

			var bufferWidth = 3;
			var numUVs = 0;
			var vertexCount = (int)Json["vertex_count"];

			if(Json["normal"] != null) bufferWidth += 3;
			if(Json["tangent"] != null) bufferWidth += 4;
			if(Json["color"] != null) bufferWidth += 4;
			if(Json["uvs"] != null) { bufferWidth += 2 * (int)Json["uvs"]; numUVs = (int)Json["uvs"]; }

			var arrayBuffer = State.Buffers[(string)Json["buffer"]];

			var vertexbuffer = new float[vertexCount * bufferWidth];
			Buffer.BlockCopy(arrayBuffer, 0, vertexbuffer, 0, vertexCount * bufferWidth * sizeof(float));

			var offset = 0;

			{
				var vertices = new List<Vector3>();
				for(int i = 0; i < vertexCount; i++)
				{
					vertices.Add(new Vector3(vertexbuffer[i * bufferWidth], vertexbuffer[i * bufferWidth + 1], vertexbuffer[i * bufferWidth + 2]));
				}
				mesh.SetVertices(vertices);
				offset += 3;
			}
			if((bool)Json["normal"] == true)
			{
				var normals = new List<Vector3>();
				for(int i = 0; i < vertexCount; i++)
				{
					normals.Add(new Vector3(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1], vertexbuffer[i * bufferWidth + offset + 2]));
				}
				mesh.SetNormals(normals);
				offset += 3;
			}
			if((bool)Json["tangent"] == true)
			{
				var tangents = new List<Vector4>();
				for(int i = 0; i < vertexCount; i++)
				{
					tangents.Add(new Vector4(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1], vertexbuffer[i * bufferWidth + offset + 2], vertexbuffer[i * bufferWidth + offset + 3]));
				}
				mesh.SetTangents(tangents);
				offset += 4;
			}
			if((bool)Json["color"] == true)
			{
				var colors = new List<Color>();
				for(int i = 0; i < vertexCount; i++)
				{
					colors.Add(new Color(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1], vertexbuffer[i * bufferWidth + offset + 2], vertexbuffer[i * bufferWidth + offset + 3]));
				}
				mesh.SetColors(colors);
				offset += 4;
			}
			for(int uvIdx = 0; uvIdx < numUVs; uvIdx++)
			{
				if((bool)Json["normal"] == true)
				{
					var uvs = new List<Vector3>();
					for(int i = 0; i < vertexCount; i++)
					{
						uvs.Add(new Vector2(vertexbuffer[i * bufferWidth + offset], vertexbuffer[i * bufferWidth + offset + 1]));
					}
					mesh.SetUVs(uvIdx, uvs);
					offset += 2;
				}
			}

			var indicesPosCounted = 0;

			var primitives = (JArray)Json["primitives"];
			mesh.subMeshCount = primitives.Count;
			for(int subMeshIdx = 0; subMeshIdx < primitives.Count; subMeshIdx++)
			{
				var primitive = (JObject)primitives[subMeshIdx];
				var indicesPos = (int)primitive["indices_pos"];
				var indicesLen = (int)primitive["indices_len"];

				var indexBuffer = new int[indicesLen];
				Buffer.BlockCopy(arrayBuffer, indicesPosCounted * sizeof(int) + vertexCount * bufferWidth * sizeof(float), indexBuffer, 0, indicesLen * sizeof(int));

				var topology = ((string)primitive["topology"]) == "tris" ? MeshTopology.Triangles : MeshTopology.Quads;
				mesh.SetIndices(indexBuffer, topology, subMeshIdx);

				indicesPosCounted += indicesLen;
			}

			var bufferOffset = indicesPosCounted * sizeof(int) + vertexCount * bufferWidth * sizeof(float);

			if(Json["skinned"] != null && (bool)Json["skinned"])
			{
				var bonesPerVertex = new byte[vertexCount];
				Buffer.BlockCopy(arrayBuffer, bufferOffset, bonesPerVertex, 0, vertexCount * sizeof(byte));
				bufferOffset += vertexCount * sizeof(byte);
				var weightLength = 0;
				foreach(var num in bonesPerVertex) weightLength += num;
				var weightBuffer = new byte[weightLength * (sizeof(float) + sizeof(int))];

				Buffer.BlockCopy(arrayBuffer, bufferOffset, weightBuffer, 0, weightLength * (sizeof(float) + sizeof(int)));
				bufferOffset += weightLength * (sizeof(float) + sizeof(int));

				var weights = new NativeArray<BoneWeight1>(weightLength, Allocator.Temp);
				var weightBufferOffset = 0;
				for(int vertIdx = 0; vertIdx < vertexCount; vertIdx++)
				{
					for(int weightIdx = 0; weightIdx < bonesPerVertex[vertIdx]; weightIdx++)
					{
						var boneIdx = BitConverter.ToInt32(weightBuffer, weightBufferOffset * (sizeof(float) + sizeof(int)));
						var weight = BitConverter.ToSingle(weightBuffer, weightBufferOffset * (sizeof(float) + sizeof(int)) + sizeof(int));
						weights[weightBufferOffset] = new BoneWeight1 {boneIndex = boneIdx, weight = weight};
						weightBufferOffset++;
					}
				}
				var bonesPerVertexNat = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);

				mesh.SetBoneWeights(bonesPerVertexNat, weights);

				State.AddTask(new Task(() => {
					var armature = (STFArmature)State.Resources[(string)Json["armature"]];
					if(armature != null)
					{
						mesh.bindposes = armature.Bindposes;
					}
				}));
			}
			
			if(Json["blendshape_count"] != null && (int)Json["blendshape_count"] > 0)
			{

				var blendshapeDefinitions = (JArray)Json["blendshapes"];
				for(int blendshapeIdx = 0; blendshapeIdx < (int)Json["blendshape_count"]; blendshapeIdx++)
				{
					
					var blendshapeBufferWidth = 4;
					var hasNormals = blendshapeDefinitions[blendshapeIdx]["normal"] != null ? (bool)blendshapeDefinitions[blendshapeIdx]["normal"] : false;
					var hasTangents = blendshapeDefinitions[blendshapeIdx]["tangent"] != null ? (bool)blendshapeDefinitions[blendshapeIdx]["tangent"] : false;
					if(hasNormals) blendshapeBufferWidth += 3;
					if(hasTangents) blendshapeBufferWidth += 3;
					
					var blendshapeLength = (int)blendshapeDefinitions[blendshapeIdx]["indices_len"];
					var blendshapeName = (string)blendshapeDefinitions[blendshapeIdx]["name"];

					var blendshapeVertecies = new Vector3[vertexCount];
					var blendshapeNormals = new Vector3[vertexCount];
					var blendshapeTangents = new Vector3[vertexCount];

					Array.Clear(blendshapeVertecies, 0, blendshapeVertecies.Length);
					Array.Clear(blendshapeNormals, 0, blendshapeNormals.Length);
					Array.Clear(blendshapeTangents, 0, blendshapeTangents.Length);

					for(int vertexIdx = 0; vertexIdx < blendshapeLength; vertexIdx++)
					{
						var index = BitConverter.ToInt32(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int));
						blendshapeVertecies[index].x = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int));
						blendshapeVertecies[index].y = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float));
						blendshapeVertecies[index].z = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 2);
						if(hasNormals)
						{
							blendshapeNormals[index].x = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 3);
							blendshapeNormals[index].y = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 4);
							blendshapeNormals[index].z = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 5);
						}
						if(hasTangents)
						{
							blendshapeTangents[index].x = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 6);
							blendshapeTangents[index].y = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 7);
							blendshapeTangents[index].z = BitConverter.ToSingle(arrayBuffer, bufferOffset + vertexIdx * blendshapeBufferWidth * sizeof(int) + sizeof(int) + sizeof(float) * 8);
						}
					}

					mesh.AddBlendShapeFrame(blendshapeName, 100, blendshapeVertecies, blendshapeNormals, blendshapeTangents);
					bufferOffset += blendshapeLength * blendshapeBufferWidth * sizeof(float);
				}
			}

			mesh.UploadMeshData(false);
			mesh.RecalculateBounds();

			State.SaveResource(mesh, "mesh", meta, Id);
			return;
		}
	}
}
