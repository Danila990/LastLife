using UnityEngine;

namespace Common
{
    public class Rotation : MonoBehaviour
    {

        public float degreesPerSec = 360f;

        void Start()
        {
        }

        void Update()
        {
            float rotAmount = degreesPerSec * Time.deltaTime;
            float curRot = transform.localRotation.eulerAngles.y;
            transform.localRotation = Quaternion.Euler(new Vector3(0, curRot + rotAmount, 0));
        }

    }
}