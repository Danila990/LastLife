using BrunoMikoski.Pahtfinding.Grid;
using Priority_Queue;

namespace BrunoMikoski.Pahtfinding
{
    public sealed class Tile : FastPriorityQueueNode
    {
        private Tile parent;
        public float StrongCost;
        public Tile Parent
        {
            get { return parent; }
        }

        private int positionX;
        public int PositionX
        {
            get
            {
                return positionX;
            }
        }
        private int positionY;
        public int PositionY
        {
            get
            {
                return positionY;
            }
        }
        
        private int positionZ;
        public int PositionZ
        {
            get
            {
                return positionZ;
            }
        }

        private float gCost;
        public float GCost
        {
            get { return gCost; }
        }

        private float hCost;

        public float FCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        private TileType tileType;
        public TileType TileType
        {
            get
            {
                return tileType;
            }
        }

        private int index;
        public int Index
        {
            get
            {
                return index;
            }
        }

        public Tile( int targetTileIndex, int targetPositionX, int targetPositionY, int targetPositionZ)
        {
            index = targetTileIndex;
            SetTilePosition( targetPositionX, targetPositionY, targetPositionZ);
        }

        public void SetParent( Tile targetTile )
        {
            parent = targetTile;
        }

        private void SetTilePosition( int targetPositionX, int targetPositionY, int targetPositionZ )
        {
            positionX = targetPositionX;
            positionY = targetPositionY;
            positionZ = targetPositionZ;
        }

        public void SetGCost( float targetGCost )
        {
            gCost = targetGCost;
        }

        public void SetHCost( float targetHCost )
        {
            hCost = targetHCost;
        }

        public void SetType( TileType targetType )
        {
            tileType = targetType;
        }

        public override bool Equals( object obj )
        {
            if ( obj is not Tile otherTile )
                return false;

            return Index == otherTile.Index;
        }
    }
}
