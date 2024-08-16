using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Core.Entity.Ai.Conditions
{
    [Category("Ai")]
    [Description("A combination of line of sight and view angle check")]
	public class CanSeeAiTarget : ConditionTask<Transform>
	{
        [RequiredField]
        public BBParameter<IAiTarget> target;
        [Tooltip("Distance within which to look out for.")]
        public BBParameter<float> maxDistance = 50;
        [Tooltip("A layer mask to use for line of sight check.")]
        public BBParameter<LayerMask> envMask = (LayerMask)( -1 );
        [Tooltip("Distance within which the target can be seen (or rather sensed) regardless of view angle.")]
        public BBParameter<float> awarnessDistance = 0f;
        [SliderField(1, 180)]
        public BBParameter<float> viewAngle = 70f;
        public Vector3 offset;

        private RaycastHit hit;

        protected override string info {
            get { return "Can See " + target; }
        }

        protected override bool OnCheck() {

            if ( target.value is null ) {
                return false;
            }
            
            var movePoint = target.value.LookAtPoint;

            if ( !target.value.IsActive ) {
                return false;
            }

            if ( Vector3.Distance(agent.position, movePoint) <= awarnessDistance.value ) {
                if ( Physics.Linecast(agent.position + offset, movePoint + offset, out hit, envMask.value) ) {
                    return false;
                }
                return true;
            }

            if ( Vector3.Distance(agent.position, movePoint) > maxDistance.value ) {
                return false;
            }

            if ( Vector3.Angle(movePoint - agent.position, agent.forward) > viewAngle.value ) {
                return false;
            }

            if ( Physics.Linecast(agent.position + offset, movePoint + offset, out hit, envMask.value) ) {
                return false;
            }

            return true;
        }

        public override void OnDrawGizmosSelected() {
            if ( agent != null ) {
                Gizmos.DrawLine(agent.position, agent.position + offset);
                Gizmos.DrawLine(agent.position + offset, agent.position + offset + ( agent.forward * maxDistance.value ));
                Gizmos.DrawWireSphere(agent.position + offset + ( agent.forward * maxDistance.value ), 0.1f);
                Gizmos.DrawWireSphere(agent.position, awarnessDistance.value);
                Gizmos.matrix = Matrix4x4.TRS(agent.position + offset, agent.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, viewAngle.value, maxDistance.value, 0, 1f);
            }
        }
	}
}