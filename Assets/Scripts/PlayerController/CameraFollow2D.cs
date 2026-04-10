using UnityEngine;

namespace PlayerController
{
    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public float smoothSpeed = 5f;
        public Vector3 offset;

        private void LateUpdate()
        {
            if (target == null)
                return;
        
            Vector3 desiredPos = target.position + offset;

            desiredPos.z = transform.position.z;

            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

            transform.position = smoothedPos;
        }
    }
}