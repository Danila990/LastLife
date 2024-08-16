using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace FileManagerSystem
{
	public static class FmUtils
	{
		public static string CompressJson(string json)
		{
			#if RELEASE_BRANCH
			var bytes = Encoding.UTF8.GetBytes(json);
			using var memoryStream = new MemoryStream();
			using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
			{
				gzipStream.Write(bytes, 0, bytes.Length);
			}
			var compressedJsonBytes = memoryStream.ToArray();
			var compressedJson = Convert.ToBase64String(compressedJsonBytes);
			return compressedJson;
			#else
			return json;
			#endif

		}

		public static string DecompressJson(string compressedJson)
		{
			#if RELEASE_BRANCH

			try
			{
				var compressedJsonBytes = Convert.FromBase64String(compressedJson);
				using var memoryStream = new MemoryStream(compressedJsonBytes);
				using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
				using var streamReader = new StreamReader(gzipStream);
				var json = streamReader.ReadToEnd();
				return json;
			}
			catch (Exception)
			{
				Debug.LogWarning("NON VALID BASE64 ENCODE");
				return "";
			}

			#else
			return compressedJson;
			#endif
		}
	}

	[Serializable]
	public struct SimpleSavedTransform
	{
		public JsonVector3 Position;
		public JsonQuaternion Rotation;
		
		public static implicit operator SimpleSavedTransform(Transform transform)
		{
			return new SimpleSavedTransform()
			{
				Position = transform.position,
				Rotation = transform.rotation,
			};
		}
		public void Set(Transform target)
		{
			target.position = Position.AsVec();
			target.rotation = Rotation.AsQuat();
		}
	}
	
	[Serializable]
	public struct JsonVector3
	{
		public float X;
		public float Y;
		public float Z;
		
		public static implicit operator JsonVector3(Vector3 vec)
		{
			return new JsonVector3
			{
				X = vec.x,
				Y = vec.y,
				Z = vec.z
			};
		}
		public readonly Vector3 AsVec()
		{
			return new Vector3(X, Y, Z);
		}
	}
	
	[Serializable]
	public struct JsonQuaternion
	{
		public float X;
		public float Y;
		public float Z;
		public float W;
		public static implicit operator JsonQuaternion(Quaternion vec)
		{
			return new JsonQuaternion
			{
				X = vec.x,
				Y = vec.y,
				Z = vec.z,
				W = vec.w
			};
		}
		public readonly Quaternion AsQuat()
		{
			return new Quaternion(X, Y, Z, W);
		}
	}
}