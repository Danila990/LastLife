using System;
using Core.Entity;
using UnityEngine;

namespace Core.Factory.DataObjects
{
    [Serializable]
    public struct FactoryEntityData
    {
        public string Key;
        public EntityContext Object;
        public EntityType Type;
    }
}