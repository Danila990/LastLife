using System.Collections.Generic;
using System.Linq;
using Db.VFXDataDto.Impl;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Core.Factory.VFXFactory.Impl
{
    [CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(VFXData), fileName = nameof(VFXData))]
    public class VFXData : ScriptableObject, IVFXData
    {
        [ShowInInspector, DisplayAsString, InlineButton("PreloadCount")] private float _allPreloadedFXCount;
		
        [SerializeField, TableList, Searchable(FilterOptions = SearchFilterOptions.ValueToString)] private ParticleData[] _particleData;
        public IList<ParticleData> ParticleData => _particleData;

        private void PreloadCount()
        {
            _allPreloadedFXCount = _particleData.Sum(data => data.PreloadCount);
        }
		
#if UNITY_EDITOR
        [Button]
        private void SetPreloadFromMaxCount()
        {
            for (var index = 0; index < _particleData.Length; index++)
            {
                ref var data = ref _particleData[index];
                data.PreloadCount = data.MaxCount / 2;
            }
            EditorUtility.SetDirty(this);
        }
#endif
    }
}