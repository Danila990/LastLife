using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Utils
{
    /*[Serializable]
    [InlineProperty]
    public struct SerializedVFXName
    {
#if UNITY_EDITOR
        [ValueDropdown("GetKeys")]
#endif
        public string VFXKey;
#if UNITY_EDITOR
        [ValueDropdown("GetAssets",FlattenTreeView = true)]public string SelectedAsset;

        public IEnumerable<string> GetAssets()
        {
            return AssetDatabase.FindAssets("t:VFXData", null).Select(AssetDatabase.GUIDToAssetPath);
        }
        public IEnumerable<string> GetKeys()
        {
            if (string.IsNullOrEmpty(SelectedAsset))
            {
                return AssetDatabase.LoadAssetAtPath<VFXData>("Assets/Settings/Data/VFX/VFXData.asset").ParticleData.Select(x=>x.Name);
            }
            var asset = AssetDatabase.LoadAssetAtPath<VFXData>(SelectedAsset);
            return asset.ParticleData.Select(x => x.Name);
        }
    }
#endif*/
}