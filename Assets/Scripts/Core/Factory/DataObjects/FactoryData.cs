using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Factory.DataObjects
{
    [CreateAssetMenu(menuName = SoNames.FACTORY_DATA+nameof(FactoryData),fileName = nameof(FactoryData))]
    public class FactoryData : ScriptableObject, IFactoryData
    {
        [SerializeField, ListDrawerSettings(ListElementLabelName = "Key", ElementColor = "GetElementColor"), Searchable, TableList] private FactoryEntityData[] _objects;
        public FactoryEntityData[] Objects => _objects;

       
        
#if UNITY_EDITOR
        public Color GetElementColor(int index, Color defaultColor)
        {
            var item = _objects[index];
            if (_objects.Count(x => x.Key == item.Key) > 1)
            {
                return Color.red;
            }
            return defaultColor;
        }
        /// <summary>
        /// Editor Only
        /// </summary>
        public static FactoryData EditorInstance;

        public static string[] AllIds;
		
        public static IEnumerable<string> ByType(EntityType type) 
        {
            return EditorInstance.Objects.Where(x => x.Type == type).Select(so => so.Key);
        }
        
        public void UpdateValues()
        {
            AllIds = Objects.Select(so => so.Key).ToArray();
        }
		
        private void OnEnable()
        {
            EditorInstance = this;
        }
#endif
    }
}