using System;
using System.Collections.Generic;
using System.Linq;
using Core.Services.SaveSystem;
using Core.Services.SaveSystem.SaveAdapters.EntitySave;
using Newtonsoft.Json;
using UnityEngine;
using Yarn.Unity;

namespace Market.OilRig
{
	public interface IConveyorSaveAdapter
	{
		public void Refresh(ConveyorObject conveyor);

		public bool TryGetConveyorData(string id, out ReadOnlyConveyorData data);
	}
	
	public class ConveyorSaveAdapter : IAutoLoadAdapter, IConveyorSaveAdapter
	{
		private Conveyors _conveyors;
		public bool CanSave => true;
		public string SaveKey => "Conveyors";

		public string CreateSave()
		{
			var result = "";
			try
			{
				result = JsonConvert.SerializeObject(_conveyors);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(ConveyorSaveAdapter)}]" + e.Message);
			}
			
			return result;
		}
		
		public void LoadSave(string value)
		{
			try
			{
				_conveyors = JsonConvert.DeserializeObject<Conveyors>(value);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(ConveyorSaveAdapter)}]" + e.Message);
			}
		}

		public void Refresh(ConveyorObject conveyor)
		{
			_conveyors.Data ??= new();

			if(!_conveyors.Data.ContainsKey(conveyor.Id))
				_conveyors.Data.Add(conveyor.Id, ParseConveyor(conveyor));
			else
				_conveyors.Data[conveyor.Id] = ParseConveyor(conveyor);

		}

		public bool TryGetConveyorData(string id, out ReadOnlyConveyorData data)
		{
			data = default;
			if (_conveyors.Data == null)
				return false;

			if (_conveyors.Data.TryGetValue(id, out var saveData))
			{
				data = new ReadOnlyConveyorData(saveData.EndPoints);
				return true;
			}
			
			return false;
		} 

		private ConveyorSaveData ParseConveyor(ConveyorObject conveyor)
		{
			return new ConveyorSaveData(conveyor.Id, ParseEndPoints(conveyor));
		}
		
		private static EndPointSaveData[] ParseEndPoints(ConveyorObject conveyor)
		{
			return conveyor._endPoints
				.Where(x => !x.IsEmpty && x.Context is ISavableEntity)
				.Select(x => new EndPointSaveData(x.Id, ((ISavableEntity)x.Context).FactoryId)).ToArray();
		}
		
		[Serializable]
		public struct Conveyors
		{
			public SerializedDictionary<string, ConveyorSaveData> Data;
		}
		
		[Serializable]
		public struct ConveyorSaveData
		{
			public string ConveyorId;
			public EndPointSaveData[] EndPoints;

			public ConveyorSaveData(string conveyorId, EndPointSaveData[] endPoints)
			{
				ConveyorId = conveyorId;
				EndPoints = endPoints;
			}
		}
		
		[Serializable]
		public struct EndPointSaveData
		{
			public string EndPointId;
			public string StoredFactoryId;

			public EndPointSaveData(string endPointId, string storedFactoryId)
			{
				EndPointId = endPointId;
				StoredFactoryId = storedFactoryId;
			}
		}
	}
	
	public struct ReadOnlyConveyorData
	{
		public readonly IReadOnlyCollection<ConveyorSaveAdapter.EndPointSaveData> EndPoints;

		public ReadOnlyConveyorData(IReadOnlyCollection<ConveyorSaveAdapter.EndPointSaveData> endPoints)
		{
			EndPoints = endPoints;
		}
	}
}
