using System;
using System.Collections.Generic;
using System.Linq;
using Db.ObjectData;
using Db.Roulette;
using SharedUtils;
using Ui.Widget;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.CharacterMenu.Roulette
{
	public class RouletteController : IDisposable
	{
		private readonly RouletteData _rouletteData;
		private readonly Stack<ObjectData> _shuffledItems = new Stack<ObjectData>();
		private readonly ReactiveCommand<bool> _onButtonState;

		private float _elementWidth;
		private float _screenWidth;
		private int _rndElementsCount;
		private double _rollDist;
		
		private Stack<RouletteObjectSo> _shuffledFakeItems = new Stack<RouletteObjectSo>();
		private List<HighlightableImage> _itemBuffer;
		private List<RouletteObjectSo> _availableRouletteObjectData;
		private HighlightableImage _current;
		private ObjectData[] _dataPool;
		private double _speed = -1f;
		public ObjectData LastDrop { get; private set; }
		public IReactiveCommand<bool> OnButtonState => _onButtonState;

		public bool IsAllItemsUnlocked => _availableRouletteObjectData.Count == 0; 
		public bool RouletteIsOpened { get; set; } = true;

		public RouletteController(RouletteData rouletteData)
		{
			_rouletteData = rouletteData;
			_onButtonState = new ReactiveCommand<bool>();
		}

		public void Init()
		{
			_rouletteData.PointerImg.enabled = false;
			_elementWidth = ((RectTransform)_rouletteData.ItemPrefab.transform).sizeDelta.x + _rouletteData.LayoutGroup.spacing;
			_screenWidth = _rouletteData.RouletteHolder.sizeDelta.x;
			var count = Mathf.CeilToInt(_screenWidth / _elementWidth);
			count += 1 - count % 2;
			_rouletteData.ElementsCount = count;
			_itemBuffer = new List<HighlightableImage>(_rouletteData.ElementsCount);
			_dataPool = new ObjectData[_rouletteData.ElementsCount];
			
			_availableRouletteObjectData = _rouletteData.ObjectsData.ToList();

			for (int i = 0; i < _rouletteData.ElementsCount; i++)
			{
				var rndObj = _availableRouletteObjectData.GetRandom().ObjectSo.ObjectData;
				var item = Object.Instantiate(_rouletteData.ItemPrefab, _rouletteData.Content);
				item.Ico.sprite = rndObj.Ico;
				_itemBuffer.Add(item);
				_dataPool[i] = rndObj;
				item.name = $"item {i}";
			}
		}

		public void Refresh()
		{
			_availableRouletteObjectData 
				= _availableRouletteObjectData.Where(x => PlayerPrefs.GetInt(x.ObjectSo.ObjectData.UnlockKey, 0) == 0).ToList();
			_availableRouletteObjectData.Shuffle();

			_shuffledFakeItems.Clear();
			foreach (var item in _availableRouletteObjectData)
				_shuffledFakeItems.Push(item);
			
			_shuffledItems.Clear();
			foreach (var item in _availableRouletteObjectData)
			{
				_shuffledItems.Push(item.ObjectSo.ObjectData);
			}
		}
		
		public void Tick(float fixedUnscaledDeltaTime)
		{
			if (_speed < 0) return;
			RollTick(fixedUnscaledDeltaTime);
		}
		
		private void NullifyOffset()
		{
			var fullRect = _elementWidth * _rouletteData.ElementsCount - _rouletteData.LayoutGroup.spacing;
			var deltaScreen = fullRect - _screenWidth;
			var position = _rouletteData.Content.anchoredPosition;
			position.x = -deltaScreen / 2f;
			_rouletteData.Content.anchoredPosition = position;
		}

		public ObjectData ClaimLastDrop()
		{
			var drop = LastDrop;
			LastDrop = null;
			_rouletteData.PointerImg.enabled = false;
			return drop;
		}

		private void Shuffle()
		{
			_shuffledItems.Clear();
			_availableRouletteObjectData.Shuffle();
			foreach (var item in _availableRouletteObjectData)
			{
				_shuffledItems.Push(item.ObjectSo.ObjectData);
			}
		}

		public void OnSpinStart()
		{
			_onButtonState.Execute(false);
		}
		
		public void Spin(int count)
		{
			if (_shuffledItems.Count <= 0)
			{
				Shuffle();
			}
			Spin(_shuffledItems.Pop(), count);
		}

		private void Spin(ObjectData drop, int count)
		{
			_rouletteData.PointerImg.enabled = true;
			NullifyOffset();
			_rollDist = _elementWidth * (count + (_rouletteData.ElementsCount - 1f) / 2);
			_rndElementsCount = count;
			LastDrop = drop;
			
			_speed = math.sqrt(2 * _rollDist * _rouletteData.Acceleration);
			if (_current)
			{
				_current.Highlight(false);
				_current = null;
			}
		}
		
		private void FinishSpin()
		{
			_onButtonState.Execute(true);
			_current = _itemBuffer[(_dataPool.Length / 2) - (_dataPool.Length % 2)];
			_current.Highlight(true);
		}
		
		private void OnScroll()
		{
			var position = _rouletteData.Content.anchoredPosition;

			if (!(Mathf.Abs(position.x) > _elementWidth))
				return;

			Shift(position.x);
			var newPos = position.x % _elementWidth;
			position.x = newPos;
			_rouletteData.Content.anchoredPosition = position;
		}
		
		private void Shift(float dir)
		{
			if (dir < 0)
			{
				ShiftLeft();
			}
			UpdateImages();
		}
		
		private void UpdateImages()
		{
			for (int i = 0; i < _dataPool.Length; i++)
			{
				_itemBuffer[i].Ico.sprite = _dataPool[i].Ico;
				_itemBuffer[i].Highlight(false);
			}
		}

		private void ShiftLeft()
		{
			for (var i = 0; i < _dataPool.Length - 1; i++)
			{
				_dataPool[i] = _dataPool[i + 1];
			}
			_dataPool[^1] = GetElement();
		}

		private ObjectData GetElement()
		{
			_rndElementsCount--;
			if (_rndElementsCount == 0)
			{
				return LastDrop;
			}
			return GetShuffled();
		}

		private ObjectData GetShuffled()
		{
			if (_shuffledFakeItems.Count == 0)
			{
				_shuffledFakeItems = new Stack<RouletteObjectSo>(_availableRouletteObjectData.Shuffle());
			}
			
			return  _shuffledFakeItems.Pop().ObjectSo.ObjectData;
		}

		private void RollTick(float delta)
		{
			_rouletteData.Content.anchoredPosition += Vector2.right * (float)-_speed * delta;
			OnScroll();
		}
		
		public void UpdateSpeed(float fixedUnscaledDeltaTime)
		{
			if (_speed < 0) return;
			RollTick(fixedUnscaledDeltaTime);
			_speed -= _rouletteData.Acceleration * fixedUnscaledDeltaTime;
			if (_speed < 0)
			{
				FinishSpin();
			}
		}
		
		public void Dispose()
		{
			_onButtonState?.Dispose();
		}
	}
}