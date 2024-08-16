using System;
using System.Collections.Generic;
using BrunoMikoski.Pahtfinding;
using BrunoMikoski.Pahtfinding.Grid;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

namespace Pathfinding
{
	public class GridBuilder : MonoBehaviour
	{
		[SerializeField] private Vector3Int _gridCount;
		[SerializeField] private Vector3 _gridSize;
		[SerializeField] private Vector3 _gridOffset;
		
		[SerializeField] private bool _showDebugGrid;

		private bool[,,] _obstacles;
		private Vector3 _cellSize;
		private Vector3 _localOffset;
		private GridController _gridController;

		private readonly Collider[] _array = new Collider[1];
		public Vector3 GridSize => _gridSize;
		
#if UNITY_EDITOR
		private static ProfilerMarker _grid = new ProfilerMarker("GridUpdate");
		[SerializeField] private Transform _start;
		[SerializeField] private Transform _end;
		
		[Button]
		public void CreatePath()
		{
			var points = new List<Vector3>();
			Pathfinding(points, _start.position, _end.position);
			var prevPoint = points[0];
			
			for (int i = 0; i < points.Count * 2 + 1; i++)
			{
				var copied = points.ToArray();
				Util.GetBezierCurvePoint(copied, points.Count, i / (points.Count * 2f + 1));
				Debug.DrawLine(prevPoint, copied[0], Color.green, 10f);
				prevPoint = copied[0];
			}
		}
#endif


		public void Build()
		{
			_obstacles = new bool[_gridCount.x, _gridCount.y, _gridCount.z];
			_cellSize = new Vector3(_gridSize.x / _gridCount.x, _gridSize.y / _gridCount.y, _gridSize.z / _gridCount.z);
			_localOffset = _gridOffset + _cellSize / 2;
			_gridController = new GridController(_gridCount.x, _gridCount.y, _gridCount.z);
			_gridController.GenerateTiles();
			Pathfinder.Initialize(_gridController);

			for (var y = 0; y < _gridCount.y; y++)
			{
				for (var x = 0; x < _gridCount.x; x++)
				{
					for (var z = 0; z < _gridCount.z; z++)
					{
						_gridController.SetTileStrong(x, y, z, true, 500000000);
					}
				}
			}
		}

		public async UniTaskVoid UpdateGrid()
		{
			var ct = this.GetCancellationTokenOnDestroy();
			while (!ct.IsCancellationRequested)
			{
				await UniTask.NextFrame(ct);
#if UNITY_EDITOR
				_grid.Begin();
#endif

				for (var y = 1; y < _gridCount.y - 1; y++)
				{
					await UniTask.NextFrame(ct);

					for (var x = 1; x < _gridCount.x - 1; x++)
					{
						for (var z = 1; z < _gridCount.z - 1; z++)
						{
							var overLap = OverlapCheck(ref x, ref y, ref z, ref _cellSize);
							if (overLap)
							{
								var extraCost = _array[0].gameObject.layer == Layers.EnvironmentLayer ? 10000 : 0;
								_gridController.SetTileStrong(x, y, z, true, 50000 + extraCost);
							}
							else
							{
								_gridController.SetTileType(x, y, z, TileType.EMPTY);
							}
							_obstacles[x, y, z] = overLap;
							//CollisionAvoidenc(x, y, z);
						}
					}
				}
				
#if UNITY_EDITOR
				_grid.End();
#endif
			}
		}
		
		public void ForceUpdateGrid()
		{
			for (var y = 1; y < _gridCount.y - 1; y++)
			{
				for (var x = 1; x < _gridCount.x - 1; x++)
				{
					for (var z = 1; z < _gridCount.z - 1; z++)
					{
						var overLap = OverlapCheck(ref x, ref y, ref z, ref _cellSize);
						if (overLap)
						{
							var extraCost = _array[0].gameObject.layer == Layers.EnvironmentLayer ? 10000 : 0;
							_gridController.SetTileStrong(x, y, z, true, 50000 + extraCost);
						}
						else
						{
							_gridController.SetTileType(x, y, z, TileType.EMPTY);
						}
						_obstacles[x, y, z] = overLap;
						//CollisionAvoidenc(x, y, z);
					}
				}
			}
		}

		private void CollisionAvoidenc(int x, int y, int z)
		{
			if (x < 1 || x > _gridCount.x - 1) return;
			if (y < 1 || y > _gridCount.y - 1) return;
			
			if (_gridController.GetTileType(x, y, z) == TileType.STRONG)
				return;
			
			if (GetNeighborsCount(x, y, z) <= 0)
				return;
			
			SetNeighborsCost(x, y, z, 500);

			SetNeighborsCost(x - 1, y - 1, z - 1, 500);
			SetNeighborsCost(x - 1, y, z, 500);
			SetNeighborsCost(x - 1, y + 1, z, 500);
			SetNeighborsCost(x, y - 1, z, 500);

			SetNeighborsCost(x, y + 1, z, 500);
			SetNeighborsCost(x + 1, y - 1, z, 500);
			SetNeighborsCost(x + 1, y, z, 500);
			SetNeighborsCost(x + 1, y + 1, z, 500);
		}

		private void SetNeighborsCost(int x, int y, int z, float cost)
		{
			if (_gridController.GetTileType(x, y, z) == TileType.STRONG)
				return;
			
			_gridController.SetTileStrong(x, y, z, true, 500);
		}

		private int GetNeighborsCount(int x, int y, int z)
		{
			var res = 0;
			res += IsObstacle(x - 1, y - 1, z) * 1;
			res += IsObstacle(x - 1, y, z) * 2;
			res += IsObstacle(x - 1, y + 1, z) * 4;
			res += IsObstacle(x, y - 1, z) * 8;
			res += IsObstacle(x, y + 1, z) * 16;
			res += IsObstacle(x + 1, y - 1, z) * 32;
			res += IsObstacle(x + 1, y, z) * 64;
			res += IsObstacle(x + 1, y + 1, z) * 128;
			return res;
		}
		
		private int IsObstacle(int x, int y, int z)
		{
			return _obstacles[x, y, z] ? 1 : 0;
		}

		private bool OverlapCheck(ref int x, ref int y, ref int z, ref Vector3 cellSize)
		{
			var count = Physics.OverlapBoxNonAlloc(
				GetWorldPos(x, y, z),
				new Vector3(cellSize.x, cellSize.y, cellSize.z) / 2, _array);
			return count > 0;
		}

		public void Pathfinding(IList<Vector3> pointPath, Vector3 from, Vector3 to)
		{
			//var stopwatch = Stopwatch.StartNew();
			from = ClampInsideGrid(from);
			to = ClampInsideGrid(to);

			try
			{
				var path = Pathfinder.GetPath(GetGridPos(from), GetGridPos(to));
				pointPath.Clear();

				for (int i = path.Count - 1; i > 0; i--)
				{
#if UNITY_EDITOR
					Debug.DrawLine(GetWorldPos(path[i]), GetWorldPos(path[i - 1]), Color.magenta, 1f);
#endif
					pointPath.Add(GetWorldPos(path[i]));
				}
				
			}
			catch (Exception e)
			{
				pointPath.Clear();
				Debug.LogError("Handled:" + e.Message);
			}
			
			
			//stopwatch.Stop();
			//Debug.Log(stopwatch.ElapsedMilliseconds);
		}

		private Vector3 ClampInsideGrid(Vector3 pos)
		{
			return new Vector3( 
				Mathf.Clamp(pos.x, _gridOffset.x, _gridOffset.x + _gridSize.x),
				Mathf.Clamp(pos.y, _gridOffset.y, _gridOffset.y + _gridSize.y),
				Mathf.Clamp(pos.z, _gridOffset.z, _gridOffset.z + _gridSize.z));
		}

		private void OnDrawGizmos()
		{
			if (!_showDebugGrid)
				return;
			
			if (_gridCount.x <= 0 || _gridCount.y <= 0)
				return;
			
			var cellSize = new Vector3(_gridSize.x / _gridCount.x, _gridSize.y / _gridCount.y, _gridSize.z / _gridCount.z);

			var localOffset = _gridOffset + cellSize / 2;
			for (var x = 0; x < _gridCount.x; x++)
			{
				for (var y = 0; y < _gridCount.y; y++)
				{
					for (var z = 0; z < _gridCount.z; z++)
					{
						DrawCube(x, y, z, cellSize, localOffset);
					}
				}
			}
			//Debug.DrawLine(_currPos,_currPos-Vector3.forward,Color.green);
			//Debug.DrawLine(_target,_target-Vector3.forward,Color.green);
		}

		private Vector3 GetWorldPos(Tile tile)
		{
			return GetWorldPos(tile.PositionX, tile.PositionY, tile.PositionZ);
		}

		private Vector3 GetWorldPos(int x, int y, int z)
		{
			return new Vector3(_localOffset.x + x * _cellSize.x, _localOffset.y + y * _cellSize.y, _localOffset.z + z * _cellSize.z);
		}

		private Vector3Int GetGridPos(Vector3 pos)
		{
			var x = Mathf.RoundToInt((pos.x - _localOffset.x) / _cellSize.x);
			var y = Mathf.RoundToInt((pos.y - _localOffset.y) / _cellSize.y);
			var z = Mathf.RoundToInt((pos.z - _localOffset.z) / _cellSize.z);
			return new Vector3Int(x, y, z);
		}

		private void DrawCube(int x, int y, int z, Vector3 cellSize, Vector3 localOffset, bool force = false)
		{
#if UNITY_EDITOR
			if (force)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(GetWorldPos(x, y, z),
					new Vector3(cellSize.x, cellSize.y, cellSize.z));
				return;
			}

			if (EditorApplication.isPlaying)
			{
				//var tile = _gridController.Tiles[_gridController.TilePosToIndex(x, y, z)];
				//Handles.Label(GetWorldPos(x, y, z), tile.StrongCost.ToString(CultureInfo.InvariantCulture) + tile.TileType);

				if (!_obstacles[x, y, z])
					return;

				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(GetWorldPos(x, y, z),
					new Vector3(cellSize.x, cellSize.y, cellSize.z));
				return;
			}

			_cellSize = new Vector3(_gridSize.x / _gridCount.x, _gridSize.y / _gridCount.y, _gridSize.z / _gridCount.z);

			_localOffset = _gridOffset + _cellSize / 2;
			Gizmos.DrawWireCube(GetWorldPos(x, y, z),
				new Vector3(cellSize.x, cellSize.y, cellSize.z));
#endif
		}
	}
}