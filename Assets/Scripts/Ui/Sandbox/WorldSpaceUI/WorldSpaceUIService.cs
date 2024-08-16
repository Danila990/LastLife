using System.Collections.Generic;
using Core.CameraSystem;
using GameSettings;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer.Unity;

namespace Ui.Sandbox.WorldSpaceUI
{
    public class WorldSpaceUIService : IWorldSpaceUIService, IStartable, ITickable
    {
        private readonly Dictionary<string, ParamPool<WorldSpaceUI>> _pools = new Dictionary<string, ParamPool<WorldSpaceUI>>();
        private readonly IWorldSpaceUISO _data;
        private readonly WorldSpaceUiView _holder;
        private readonly ICameraService _cameraService;
        private readonly List<WorldSpaceUI> _activeUIs = new List<WorldSpaceUI>();
        private readonly Stack<WorldSpaceUI> _toRemove = new Stack<WorldSpaceUI>();
        private Camera _camera;

        public WorldSpaceUIService(
            IWorldSpaceUISO data,
            WorldSpaceUiView holder,
            ICameraService cameraService
        )
        {
            _data = data;
            _holder = holder;
            _cameraService = cameraService;
        }
        
        public void Start()
        {
            _camera = _cameraService.CurrentBrain.OutputCamera;
            foreach (var ui in _data.UI)
            {
                var pool = new ParamPool<WorldSpaceUI>(ui.UiElement, _holder.transform);
                _pools.Add(ui.Key,pool);
            }
        }

        public T GetUI<T>(string key) where T : WorldSpaceUI
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                var ui = pool.Rent();
                ui.Key = key;
                _activeUIs.Add(ui);
                ui.transform.localScale = Vector3.one;
#if UNITY_EDITOR
                if(ui is not T)
                    throw new System.Exception($"UI {key} is not of type {typeof(T)}");
#endif
                return ui as T;
            }
            
            Debug.LogError($"no pool for {key}");
            return null;
        }

        public void Tick()
        {
            foreach (var ui in _activeUIs)
            {
                if (!ui.Target && !ui.IsInactive)
                {
                    ui.IsInactive = true;
                    continue;
                }
                if (ui.Target)
                {
                    var pos = RectTransformUtility.WorldToScreenPoint(_camera, ui.Target.position + ui.Offset);
                    ui.transform.position = pos;
                }
                if (ui.IsInactive)
                {
                    _toRemove.Push(ui);
                }
            }

            while (_toRemove.TryPop(out var del))
            {
                _activeUIs.Remove(del);
                del.IsInactive = false;
                _pools[del.Key].Return(del);
            }
        }
    }
}