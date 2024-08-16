using System.Collections.Generic;
using BrunoMikoski.Pahtfinding.Grid;
using Priority_Queue;
using UnityEngine;

namespace BrunoMikoski.Pahtfinding
{
    public static class Pathfinder
    {
        private static GridController gridController;

        private static FastPriorityQueue<Tile> openListPriorityQueue;
        private static Tile[] neighbors = new Tile[6];
        private static List<Tile> finalPath;
        private static Dictionary<int, Tile> closeDictionary;

        public static void Initialize( GridController targetGridController )
        {
            gridController = targetGridController;
            openListPriorityQueue = new FastPriorityQueue<Tile>( gridController.GridSizeX * gridController.GridSizeY * gridController.GridSizeZ );
            finalPath = new List<Tile>( Mathf.RoundToInt( gridController.Tiles.Length * 0.1f ) );
            closeDictionary = new Dictionary<int, Tile>( Mathf.RoundToInt( gridController.Tiles.Length * 0.1f ) );
        }

        public static List<Tile> GetPath(Vector3Int from, Vector3Int to)
        {
            finalPath.Clear();

            var fromIndex = gridController.TilePosToIndex( from.x, from.y, from.z );
            var toIndex = gridController.TilePosToIndex( to.x, to.y, to.z );
            if (fromIndex < 0 || fromIndex > gridController.Tiles.Length)
            {
                return finalPath;
            }
            if (toIndex < 0 || toIndex > gridController.Tiles.Length)
            {
                return finalPath;
            }
            var initialTile = gridController.Tiles[fromIndex];
            var destinationTile = gridController.Tiles[toIndex];

            openListPriorityQueue.Enqueue( initialTile, 0 );
            
            Tile currentTile = null;
            while ( openListPriorityQueue.Count > 0 )
            {
                currentTile = openListPriorityQueue.Dequeue();

                closeDictionary.Add( currentTile.Index, currentTile );

                if ( Equals( currentTile, destinationTile ) )
                    break;

                UpdateNeighbors( currentTile );

                for ( var i = neighbors.Length - 1; i >= 0; --i )
                {
                    var neighbourPathTile = neighbors[i];
                    if ( neighbourPathTile == null )
                        continue;
                    

                    if(closeDictionary.ContainsKey( neighbourPathTile.Index ))
                        continue;

                    var isAtOpenList = openListPriorityQueue.Contains( neighbourPathTile );
                    
                    var movementCostToNeighbour = 
                        currentTile.GCost +
                        GetDistance( currentTile, neighbourPathTile) +
                        (neighbourPathTile.TileType == TileType.STRONG ? neighbourPathTile.StrongCost : 0);
                    if ( movementCostToNeighbour < neighbourPathTile.GCost || !isAtOpenList )
                    {
                        neighbourPathTile.SetGCost( movementCostToNeighbour );
                        neighbourPathTile.SetHCost( GetDistance( neighbourPathTile, destinationTile ) );
                        neighbourPathTile.SetParent( currentTile );

                        if ( !isAtOpenList )
                        {
                            openListPriorityQueue.Enqueue( neighbourPathTile,
                                                           neighbourPathTile.FCost );
                        }
                        else
                        {
                            openListPriorityQueue.UpdatePriority( neighbourPathTile, neighbourPathTile.FCost );
                        }
                    }
                }
            }

            while ( currentTile is { Parent: not null } && !Equals( currentTile, initialTile ) )
            {
                finalPath.Add( currentTile );

                currentTile = currentTile.Parent;
            }

            finalPath.Add( initialTile );

            openListPriorityQueue.Clear();
            closeDictionary.Clear();
            return finalPath;
        }


        private static float GetDistance( Tile targetFromTile, Tile targetToTile )
        {
            var fromPositionX = targetFromTile.PositionX;
            var toPositionX = targetToTile.PositionX;
            var fromPositionY = targetFromTile.PositionY;
            var toPositionY = targetToTile.PositionY;
            
            var fromPositionZ = targetFromTile.PositionZ;
            var toPositionZ = targetToTile.PositionZ;
            
            return  
                (fromPositionX - toPositionX) *
                (fromPositionX - toPositionX) +
                (fromPositionY - toPositionY) *
                (fromPositionY - toPositionY) +
                (fromPositionZ - toPositionZ) *
                (fromPositionZ - toPositionZ);
        }

        private static void UpdateNeighbors( Tile targetTile )
        {
            // var values = Enum.GetValues(typeof(NeighborDirection));
            // for (var index = 0; index < values.Length; index++)
            // {
            //     neighbors[index] = GetNeighborAtDirection(targetTile, (NeighborDirection)values.GetValue(index));
            // }
            
            neighbors[0] = GetNeighborAtDirection( targetTile, NeighborDirection.LEFT );
            neighbors[1] = GetNeighborAtDirection( targetTile, NeighborDirection.RIGHT );
            neighbors[2] = GetNeighborAtDirection( targetTile, NeighborDirection.TOP );
            neighbors[3] = GetNeighborAtDirection( targetTile, NeighborDirection.DOWN );
            neighbors[4] = GetNeighborAtDirection( targetTile, NeighborDirection.FORWARD );
            neighbors[5] = GetNeighborAtDirection( targetTile, NeighborDirection.BACKWARD );
        }

        private static Tile GetNeighborAtDirection( Tile targetTile, NeighborDirection targetDirection )
        {

            GetNeighbourPosition( targetTile, targetDirection, out var positionX, out var positionY, out var positionZ );
            if ( !gridController.IsValidTilePosition( positionX, positionY, positionZ ) )
                return null;
            
            var neighborIndex = gridController.TilePosToIndex( positionX, positionY, positionZ );

            var neighborAtDirection = gridController.Tiles[neighborIndex];
            return neighborAtDirection;
        }

        private static void GetNeighbourPosition( Tile targetTile, NeighborDirection targetDirection, out int targetPositionX, out int targetPositionY, out int targetPositionZ)
        {
            targetPositionX = targetTile.PositionX;
            targetPositionY = targetTile.PositionY;
            targetPositionZ = targetTile.PositionZ;
            
            switch ( targetDirection )
            {
                case NeighborDirection.LEFT:
                    targetPositionX -= 1;
                    break;
                case NeighborDirection.TOP:
                    targetPositionY += 1;
                    break;
                case NeighborDirection.RIGHT:
                    targetPositionX += 1;
                    break;
                case NeighborDirection.DOWN:
                    targetPositionY -= 1;
                    break;
                case NeighborDirection.LEFT_UP:
                    targetPositionX -= 1;
                    targetPositionY += 1;
                    break;
                case NeighborDirection.LEFT_DOWN:
                    targetPositionX -= 1;
                    targetPositionY -= 1;
                    break;
                case NeighborDirection.RIGHT_UP:
                    targetPositionX += 1;
                    targetPositionY += 1;
                    break;
                case NeighborDirection.RIGHT_DOWN:
                    targetPositionX += 1;
                    targetPositionY -= 1;
                    break;
                case NeighborDirection.FORWARD:
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.BACKWARD:
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.FORWARD_LEFT:
                    targetPositionX -= 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.FORWARD_RIGHT:
                    targetPositionX += 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.BACKWARD_LEFT:
                    targetPositionX -= 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.BACKWARD_RIGHT:
                    targetPositionX += 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.FORWARD_UP:
                    targetPositionY += 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.FORWARD_DOWN:
                    targetPositionY -= 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.BACKWARD_UP:
                    targetPositionY += 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.BACKWARD_DOWN:
                    targetPositionY -= 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.FORWARD_LEFT_UP:
                    targetPositionX -= 1;
                    targetPositionY += 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.FORWARD_RIGHT_UP:
                    targetPositionX += 1;
                    targetPositionY += 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.FORWARD_LEFT_DOWN:
                    targetPositionX -= 1;
                    targetPositionY -= 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.FORWARD_RIGHT_DOWN:
                    targetPositionX += 1;
                    targetPositionY -= 1;
                    targetPositionZ += 1;
                    break;
                case NeighborDirection.BACKWARD_LEFT_UP:
                    targetPositionX -= 1;
                    targetPositionY += 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.BACKWARD_RIGHT_UP:
                    targetPositionX += 1;
                    targetPositionY += 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.BACKWARD_LEFT_DOWN:
                    targetPositionX -= 1;
                    targetPositionY -= 1;
                    targetPositionZ -= 1;
                    break;
                case NeighborDirection.BACKWARD_RIGHT_DOWN:
                    targetPositionX += 1;
                    targetPositionY -= 1;
                    targetPositionZ -= 1;
                    break;
            }
        }
    }
}
