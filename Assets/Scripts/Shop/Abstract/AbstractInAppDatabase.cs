using System.Collections.Generic;
using System.Linq;
using Shop.Models.Product;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shop.Abstract
{
	public abstract class AInAppDatabase : ScriptableObject, IInAppDatabase
	{
		[SerializeField][TableList][Searchable]
		private List<InAppProduct> _products;

		public IList<InAppProduct> GetProducts() => _products;
		
		
		public InAppProduct GetProductById(string inAppID)
		{
			foreach (var product in _products)
			{
				if (product.InAppID != inAppID)
					continue;
				return product;
			}

			Debug.Log($"[{GetType().Name}] Cannot find product {inAppID}");
			return null;
		}
		
#if UNITY_EDITOR
		/// <summary>
		/// Editor Only
		/// </summary>
		public static AInAppDatabase EditorInstance;

		public static string[] AllIds;
		
		public void UpdateValues()
		{
			AllIds = _products.Select(product => product.InAppID).ToArray();
		}

		// private IEnumerable<string> GetNames()
		// {
		// 	foreach (var data in CharacterDataSo)
		// 	{
		// 		yield return data.ObjectData.Id;
		// 	}
		//
		// 	foreach (var data in ItemsData)
		// 	{
		// 		yield return data.ObjectData.Id;
		// 	}
		// 	
		// 	foreach (var data in InventoryData)
		// 	{
		// 		yield return data.ObjectData.Id;
		// 	}
		// }
		//
		private void OnEnable()
		{
			EditorInstance = this;
			UpdateValues();
		}
		
		[Button]
		public void ParseFromString(string str)
		{
			var words = str.Split(" ");
			foreach (var word in words)
			{
				var trimmedWord = word.Trim();
				_products.Add(new InAppProduct()
				{
					Name = trimmedWord,
					InAppID = trimmedWord
				});
			}
		}	
#endif
	}
}