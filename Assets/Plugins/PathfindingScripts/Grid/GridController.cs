using UnityEngine;

namespace BrunoMikoski.Pahtfinding.Grid
{
    public enum TileType : byte
    {
        EMPTY,
        ROAD,
        BLOCK,
        STRONG
    }

    public enum NeighborDirection : byte
    {
        LEFT = 0,
        TOP = 1,
        RIGHT = 2,
        DOWN = 3,
        
        LEFT_UP = 4,
        LEFT_DOWN = 5,
        RIGHT_UP = 6,
        RIGHT_DOWN = 7,
        
        FORWARD = 8,
        BACKWARD = 9,
        
        FORWARD_LEFT = 10,
        FORWARD_RIGHT = 11,
        BACKWARD_LEFT = 12,
        BACKWARD_RIGHT = 13,
        
        FORWARD_UP = 14,
        FORWARD_DOWN = 15,
        BACKWARD_UP = 16,
        BACKWARD_DOWN = 17,
        
        FORWARD_LEFT_UP = 18,
        FORWARD_RIGHT_UP = 19,
        FORWARD_LEFT_DOWN = 20,
        FORWARD_RIGHT_DOWN = 21,
        
        BACKWARD_LEFT_UP = 22,
        BACKWARD_RIGHT_UP = 23,
        BACKWARD_LEFT_DOWN = 24,
        BACKWARD_RIGHT_DOWN = 25,
    }

    public sealed class GridController
    {
        private int gridSizeX;
        public int GridSizeX
        {
            get { return gridSizeX; }
        }

        private int gridSizeY;
        public int GridSizeY
        {
            get { return gridSizeY; }
        }
        
        private int gridSizeZ;
        
        public int GridSizeZ
        {
            get { return gridSizeZ; }
        }

        public GridController(int sizeX, int sizeY, int sizeZ)
        {
            gridSizeX = sizeX;
            gridSizeY = sizeY;
            gridSizeZ = sizeZ;
        }

        private Tile[] tiles;
        public Tile[] Tiles
        {
            get
            {
                return tiles;
            }
        }

        public int TilePosToIndex( int x, int y , int z )
        {
            return x + y * gridSizeX + z * gridSizeX * gridSizeY;
        }

        public void IndexToTilePos( int index, out int x, out int y, out int z )
        {
            x = index % gridSizeX;
            z = Mathf.FloorToInt( index / (float)(gridSizeX * gridSizeY));
            y = Mathf.FloorToInt( (index - z * gridSizeX * gridSizeY) / (float) gridSizeX );
        }

        public void SetTileType( int index, TileType type )
        {
            tiles[index].SetType( type );
        }

        public void SetTileType( int x, int y, int z, TileType type )
        {
            SetTileType( TilePosToIndex( x, y, z ), type );
        }

        public void SetTileType( Vector3Int targetPosition, TileType type )
        {
            SetTileType( TilePosToIndex( targetPosition.x, targetPosition.y, targetPosition.z ), type );
        }

        public TileType GetTileType( int index )
        {
            return tiles[index].TileType;
        }

        public TileType GetTileType( int x, int y, int z )
        {
            return GetTileType( TilePosToIndex( x, y, z ) );
        }

        public void SetTileBlocked( int index, bool blocked )
        {
            SetTileType( index, blocked ? TileType.BLOCK : TileType.EMPTY );
        }

        public void SetTileStrong(int index, bool blocked, float cost)
        {
            SetTileType(index, blocked ? TileType.STRONG : TileType.EMPTY);
            if (!blocked)
                return;
            Tiles[index].StrongCost = cost;
        }

        public void SetTileBlocked( int x, int y, int z, bool blocked )
        {
            SetTileBlocked( TilePosToIndex( x, y, z ), blocked );
        }

        public void SetTileStrong(int x, int y, int z, bool blocked, float cost)
        {
            SetTileStrong(TilePosToIndex(x, y, z), blocked,cost);
        }

        public bool IsTileBlocked( int index )
        {
            return tiles[index].TileType == TileType.BLOCK;
        }

        public bool IsTileBlocked( int x, int y, int z)
        {
            return IsTileBlocked( TilePosToIndex( x, y, z ) );
        }

        public void GenerateTiles()
        {
            tiles = new Tile[gridSizeX * gridSizeY * gridSizeZ];
            for ( var i = tiles.Length - 1; i >= 0; i-- )
            {
                IndexToTilePos( i, out var positionX, out var positionY, out var positionZ );
                tiles[i] = new Tile( i, positionX, positionY, positionZ );
            }
        }

        public bool IsValidTilePosition( int targetPositionX, int targetPositionY, int targetPositionZ )
        {
            if ( targetPositionX < 0 || targetPositionX > gridSizeX - 1 )
                return false;

            if ( targetPositionY < 0 || targetPositionY > gridSizeY - 1 )
                return false;

            if (targetPositionZ < 0 || targetPositionZ > gridSizeZ - 1)
                return false;

            //int tilePosToIndex = TilePosToIndex( targetPositionX, targetPositionY );

            // if ( tiles[tilePosToIndex].TileType == TileType.BLOCK)
            //     return false;

            return true;
        }

        public bool IsValidTilePosition( Vector3Int targetPosition )
        {
            return IsValidTilePosition( targetPosition.x, targetPosition.y, targetPosition.z);
        }

        public void Clear()
        {
            for ( int i = tiles.Length - 1; i >= 0; i-- )
                SetTileType( i, TileType.EMPTY );
        }

    }
}
