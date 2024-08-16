using System;
using System.Collections.Generic;
using System.Threading;
using Core.Equipment.Inventory;
using Core.Render;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Ui.Sandbox.CharacterMenu
{
	public class MainCharacterPreviewController : IDisposable
	{
		private readonly MainCharacterPreview _preview;
		private readonly IRendererFactory _rendererFactory;
		private readonly RenderTexture _texture;
		private readonly RendererHolder _holder;

		private CharacterRenderer _activeRenderer;
		private readonly Dictionary<string, CharacterRenderer> _renderers;

		private readonly float _dragSpeed = 20f;
		private Vector2 _currentPointerPosition;
		private Vector2 _lastPointerPosition;
		private Vector2 _currentDelta;
		private float _currentSpeed;
		private Vector2 _lastDelta;
		private bool _isDragging;

		public CharacterRenderer ActiveRenderer => _activeRenderer;
		
		public MainCharacterPreviewController(MainCharacterPreview preview, IRendererFactory rendererFactory)
		{
			_preview = preview;
			_rendererFactory = rendererFactory;
			_renderers = new();
			
			_texture = new RenderTexture(512, 720, 16,RenderTextureFormat.RGB111110Float);
			preview.Init();
			preview.RawImage.texture = _texture;
			
			preview.OnDragBegin.Subscribe(OnDragBegin).AddTo(preview);
			preview.OnDragEnd.Subscribe(OnDragEnd).AddTo(preview);
			preview.DragPosition.Subscribe(OnDrag).AddTo(preview);
			
			_holder = _rendererFactory.GetRendererHolder();
			_holder.RenderCamera.enabled = false;
			_holder.RenderCamera.targetTexture = _texture;
		}
		
		public void OnItemUnlock(string objId)
		{
			if (_renderers.TryGetValue(objId, out var renderer))
			{
				RefreshAsync(_preview.destroyCancellationToken, renderer).Forget();
			}
		}

		private async UniTaskVoid RefreshAsync(CancellationToken token, CharacterRenderer renderer)
		{
			renderer.SetIsUnlocked(true);
			await UniTask.NextFrame(token);
			renderer.Disable();
			await UniTask.NextFrame(token);
			renderer.Enable();
		}

		public void Display(string characterId)
		{
			if (_renderers.TryGetValue(characterId, out var rend))
			{
				SwitchRenderer(rend);
			}
			else
			{
				var renderer = _rendererFactory.CreateRenderer(characterId);
				_renderers.Add(characterId, renderer);
				SwitchRenderer(renderer);
			}
		}

		public void Hide()
		{
			SwitchRenderer(null);
		}

		public void OnTick()
		{
			if(!_activeRenderer || !_activeRenderer.Mannequin.gameObject.activeInHierarchy)
				return;
			
			_currentDelta = _lastPointerPosition - _currentPointerPosition;
			if (_isDragging)
			{
				_activeRenderer.Mannequin.Rotate(Vector3.up, _currentDelta.x * _dragSpeed * Time.deltaTime);
				_lastPointerPosition = _currentPointerPosition;
			}
			else
			{
				_currentSpeed = Mathf.Lerp(_currentSpeed, 0, 5f * Time.deltaTime);
				_activeRenderer.Mannequin.Rotate(Vector3.up, _currentSpeed * _lastDelta.x);
			}
		}

		private void OnDrag(Vector2 position)
		{
			_currentPointerPosition = position;
		}
		
		private void OnDragBegin(Vector2 position)
		{
			_lastPointerPosition = position;
			_currentSpeed = _dragSpeed;

			_isDragging = true;
		}
		
		private void OnDragEnd(Vector2 position)
		{
			_lastDelta = _currentDelta.normalized;
			_isDragging = false;
		}
		
		private void SwitchRenderer(CharacterRenderer newRenderer)
		{
			if (_activeRenderer && _activeRenderer == newRenderer)
			{
				DisplayCurrent();
				return;
			}

			HideCurrent();

			if(newRenderer == null)
				return;

			_activeRenderer = newRenderer;
			DisplayCurrent();
		}

		private void DisplayCurrent()
		{
			_holder.RenderCamera.enabled = true;
			_activeRenderer.Enable();
		}

		private void HideCurrent()
		{
			_holder.RenderCamera.enabled = false;
			if (!_activeRenderer)
				return;
			_activeRenderer.EquipmentRenderer.Hide();
			_activeRenderer.Disable();
		}
		
		public void Dispose()
		{
			_texture.Release();
		}
		
	}
}
