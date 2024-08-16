using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class RaycastSort : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            var distanceComparison = x.distance.CompareTo(y.distance);
            if (distanceComparison != 0)
                return distanceComparison;
                
            var colliderInstanceIDComparison = x.colliderInstanceID.CompareTo(y.colliderInstanceID);
            if (colliderInstanceIDComparison != 0)
                return colliderInstanceIDComparison;
            
            return x.triangleIndex.CompareTo(y.triangleIndex);
        }
    }
    
    public class ColliderByDistanceSort : IComparer<Collider>
    {
        private Vector3 _pos;

        public void SetPos(Vector3 pos)
        {
            _pos = pos;
        }
        
        public int Compare(Collider x, Collider y)
        {
            var xDist = Vector3.Distance(x.transform.position, _pos);
            var yDist = Vector3.Distance(y.transform.position, _pos);
            var distanceComparison = xDist.CompareTo(yDist);
            if (distanceComparison != 0)
                return distanceComparison;

            var nameComparison = string.Compare(x.name, y.name, StringComparison.Ordinal);
            if (nameComparison != 0)
                return nameComparison;
            var hideFlagsComparison = x.hideFlags.CompareTo(y.hideFlags);
            if (hideFlagsComparison != 0)
                return hideFlagsComparison;
            var tagComparison = string.Compare(x.tag, y.tag, StringComparison.Ordinal);
            if (tagComparison != 0)
                return tagComparison;
            var enabledComparison = x.enabled.CompareTo(y.enabled);
            if (enabledComparison != 0)
                return enabledComparison;
            var isTriggerComparison = x.isTrigger.CompareTo(y.isTrigger);
            if (isTriggerComparison != 0)
                return isTriggerComparison;
            var contactOffsetComparison = x.contactOffset.CompareTo(y.contactOffset);
            if (contactOffsetComparison != 0)
                return contactOffsetComparison;
            var hasModifiableContactsComparison = x.hasModifiableContacts.CompareTo(y.hasModifiableContacts);
            if (hasModifiableContactsComparison != 0)
                return hasModifiableContactsComparison;
            var providesContactsComparison = x.providesContacts.CompareTo(y.providesContacts);
            if (providesContactsComparison != 0)
                return providesContactsComparison;
            return x.layerOverridePriority.CompareTo(y.layerOverridePriority);
        }
    }
}