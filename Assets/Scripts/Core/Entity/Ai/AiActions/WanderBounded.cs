using Core.Entity.Characters.Adapters;
using Db.Map;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Entity.Ai.AiActions
{
    public class WanderBounded : ActionTask<AiCharacterAdapter>
    {
        [Tooltip("The speed to wander with.")]
        public BBParameter<float> speed = 4;
        [Tooltip("The distance to keep from each wander point.")]
        public BBParameter<float> keepDistance = 0.1f;
        [Tooltip("A wander point can't be closer than this distance")]
        public BBParameter<float> minWanderDistance = 5;
        [Tooltip("A wander point can't be further than this distance")]
        public BBParameter<float> maxWanderDistance = 20;
        [Tooltip("If enabled, will keep wandering forever. If not, only one wander point will be performed.")]
        public bool repeat = true;

        private ZoneBoundsObject[] _zones = new ZoneBoundsObject[5];
        private int _zoneCount = -1;

        private NavMeshAgent _agent => agent.NavMeshAgent;
        
        protected override void OnExecute() {
            _agent.speed = speed.value;
            FindZones();
            DoWander();
        }

        private void FindZones()
        {
            if (_zoneCount < 0)
            {
                _zoneCount = agent.MapZoneService.TryGetZones(ZoneType.Walkable, ref _zones);
            }
        }
        
        protected override void OnUpdate() {
            
            if(!_agent.isOnNavMesh)
                return;
            
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + keepDistance.value ) {
                if ( repeat && agent.enabled ) {
                    DoWander();
                } else {
                    EndAction();
                }
            }
        }

        void DoWander() {
            var min = minWanderDistance.value;
            var max = maxWanderDistance.value;
            min = Mathf.Clamp(min, 0.01f, max);
            max = Mathf.Clamp(max, min, max);
            var wanderPos = _agent.transform.position;
            while ( ( wanderPos - _agent.transform.position ).magnitude < min ) {
                wanderPos = ( Random.insideUnitSphere * max ) + _agent.transform.position;
            }

            if (_zoneCount > 0)
            {
                wanderPos = _zones[0].GetRandomPointInside();
                var curDist = (_agent.transform.position - wanderPos).magnitude;
                for (var i = 0; i < _zoneCount; i++)
                {
                    var pos = _zones[i].GetRandomPointInside();
                    var newDist = (_agent.transform.position - wanderPos).magnitude;
                    if (newDist > curDist) continue;
                    wanderPos = pos;
                    curDist = newDist;
                }
                Debug.DrawLine(_agent.transform.position,wanderPos,Color.cyan,5);
                Debug.DrawLine(wanderPos+Vector3.up,wanderPos,Color.blue,5);
            }

            var ray = new Ray(wanderPos, Vector3.down * 250);
            if (Physics.Raycast(ray, out var rayHit))
            {
                wanderPos = rayHit.point;
            }
            NavMeshHit hit;
            if ( NavMesh.SamplePosition(wanderPos, out hit, _agent.height * 2, NavMesh.AllAreas) ) {
                _agent.SetDestination(hit.position);
                Debug.DrawLine(_agent.transform.position,hit.position,Color.yellow,5);
                Debug.DrawLine(hit.position+Vector3.up,hit.position,Color.green,5);
            }
        }

        protected override void OnPause() { OnStop(); }
        protected override void OnStop() {
            if ( agent.gameObject.activeSelf && _agent.isOnNavMesh ) {
                _agent.ResetPath();
            }
        }
    }
}