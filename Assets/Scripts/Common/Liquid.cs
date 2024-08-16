using UnityEngine;

namespace Common
{
    public class Liquid : MonoBehaviour
    {
        public enum UpdateMode { Normal, UnscaledTime }
        public UpdateMode updateMode;
 
        [SerializeField] private float MaxWobble = 0.03f;
        [SerializeField] private float WobbleSpeedMove = 1f;
        [SerializeField] private float fillAmount = 0.5f;
        [SerializeField] private float Recovery = 1f;
        [SerializeField] private float Thickness = 1f;
        [Range(0, 1)]
        public float CompensateShapeAmount;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Renderer rend;
        private Vector3 _pos;
        private Vector3 _lastPos;
        private Vector3 _velocity;
        private Quaternion _lastRot;
        private Vector3 _angularVelocity;
        private float _wobbleAmountX;
        private float _wobbleAmountZ;
        private float _wobbleAmountToAddX;
        private float _wobbleAmountToAddZ;
        private float _pulse;
        private float _sinewave;
        private float _time = 0.5f;
        private Vector3 _comp;

        private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");
        private static readonly int WobbleX = Shader.PropertyToID("_WobbleX");
        private static readonly int WobbleZ = Shader.PropertyToID("_WobbleZ");

        private Material _material;

        public void SetMaterial(Material material)
        {
            _material = material;
        }
        // Use this for initialization
        private void Start()
        {
            GetMeshAndRend();

            _material = rend.material;
        }
 
        private void OnValidate()
        {
            GetMeshAndRend();
        }

        private void GetMeshAndRend()
        {
            if (mesh == null)
            {
                mesh = GetComponent<MeshFilter>().sharedMesh;
            }
            if (rend == null)
            {
                rend = GetComponent<Renderer>();
            }
        }
        private void Update()
        {
            float deltaTime = 0;
        
        
            switch (updateMode)
            {
                case UpdateMode.Normal:
                    deltaTime = Time.deltaTime;
                    break;
 
                case UpdateMode.UnscaledTime:
                    deltaTime = Time.unscaledDeltaTime;
                    break;
            }
 
            _time += deltaTime;
 
            if (deltaTime != 0)
            {
                // decrease wobble over time
                _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 0, (deltaTime * Recovery));
                _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 0, (deltaTime * Recovery));
 
 
 
                // make a sine wave of the decreasing wobble
                _pulse = 2 * Mathf.PI * WobbleSpeedMove;
                _sinewave = Mathf.Lerp(_sinewave, Mathf.Sin(_pulse * _time), deltaTime * Mathf.Clamp(_velocity.magnitude + _angularVelocity.magnitude, Thickness, 10));
 
                _wobbleAmountX = _wobbleAmountToAddX * _sinewave;
                _wobbleAmountZ = _wobbleAmountToAddZ * _sinewave;
 
 
 
                // velocity
                _velocity = (_lastPos - transform.position) / deltaTime;
 
                _angularVelocity = GetAngularVelocity(_lastRot, transform.rotation);
 
                // add clamped velocity to wobble
                _wobbleAmountToAddX += Mathf.Clamp((_velocity.x + (_velocity.y * 0.2f) + _angularVelocity.z + _angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
                _wobbleAmountToAddZ += Mathf.Clamp((_velocity.z + (_velocity.y * 0.2f) + _angularVelocity.x + _angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
            }
 
            // send it to the shader
            _material.SetFloat(WobbleX, _wobbleAmountX);
            _material.SetFloat(WobbleZ, _wobbleAmountZ);
 
            // set fill amount
            UpdatePos(deltaTime);
 
            // keep last position
            _lastPos = transform.position;
            _lastRot = transform.rotation;
        }

        private void UpdatePos(float deltaTime)
        {
 
            var worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));
            if (CompensateShapeAmount > 0)
            {
                // only lerp if not paused/normal update
                if (deltaTime != 0)
                {
                    _comp = Vector3.Lerp(_comp, (worldPos - new Vector3(0, GetLowestPoint(), 0)), deltaTime * 10);
                }
                else
                {
                    _comp = (worldPos - new Vector3(0, GetLowestPoint(), 0));
                }
 
                _pos = worldPos - transform.position - new Vector3(0, fillAmount - (_comp.y * CompensateShapeAmount), 0);
            }
            else
            {
                _pos = worldPos - transform.position - new Vector3(0, fillAmount, 0);
            }
            _material.SetVector(FillAmount, _pos);
        }
 
        //https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/#post-4302796
        private static Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation)
        {
            var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
            // No rotation?
            // You may want to increase this closer to 1 if you want to handle very small rotations.
            // Beware, if it is too close to one, your answer will be Nan
            if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
                return Vector3.zero;
            float gain;
            // handle negatives, we could just flip it, but this is faster
            if (q.w < 0.0f)
            {
                var angle = Mathf.Acos(-q.w);
                gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
            }
            else
            {
                var angle = Mathf.Acos(q.w);
                gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
            }
            var angularVel = new Vector3(q.x * gain, q.y * gain, q.z * gain);
 
            if (float.IsNaN(angularVel.z))
            {
                angularVel = Vector3.zero;
            }
            return angularVel;
        }

        private float GetLowestPoint()
        {
            var lowestY = float.MaxValue;
            var lowestVert = Vector3.zero;
            var vertices = mesh.vertices;
 
            for (var i = 0; i < vertices.Length; i++)
            {
 
                var position = transform.TransformPoint(vertices[i]);
 
                if (position.y < lowestY)
                {
                    lowestY = position.y;
                    lowestVert = position;
                }
            }
            return lowestVert.y;
        }
    }
}
 