using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FileManagerSystem
{
	public class JsonDataProducer
	{
		private readonly string _versionPatch;
		private readonly string _version;

		public JsonDataProducer(string versionPatch, string version)
		{
			_versionPatch = versionPatch;
			_version = version;
		}

		public T Deserialize<T>(string content)
		{
			content = FmUtils.DecompressJson(content);

			if (ValidateVersion(content, out var validated))
			{
				try
				{
					return JsonConvert.DeserializeObject<T>(validated);
				}
				catch (JsonException e)
				{
					UnityEngine.Debug.LogError(e);
					return default(T);
				}
			}
			
			return default(T);
		}

		public string Serialize(object data)
		{
			var json = JsonConvert.SerializeObject(data);
			json = string.Format(_versionPatch, _version) + json;
			return FmUtils.CompressJson(json);
		}

		private bool ValidateVersion(string data, out string validated)
		{
			validated = "";
			var regex = new Regex("&*&", RegexOptions.IgnoreCase);
			var match = regex.Match(data);
			if (match.Value == string.Format(_versionPatch, _version))
				return false;
			data = data.Replace(string.Format(_versionPatch, _version), "");
			validated = data;

			return true;
		}
	}
}