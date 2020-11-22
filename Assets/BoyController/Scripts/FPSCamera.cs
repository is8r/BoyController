using UnityEngine;

namespace BoyController
{
    public class FPSCamera : MonoBehaviour
    {
        [Header("Follow Target")]

        public Transform follow;

        [Header("Rotate Setting")]

        public Vector2 sensitivity = new Vector2(1f, 1f);
        public float clampRotationMax = 20f;
        public float clampRotationMin = -20f;

        //private property

        private Vector3 offset = Vector3.zero;
        private Quaternion targetRotCharactor;
        private Quaternion targetRotCamera;

        void Start()
        {
            offset = transform.position - follow.transform.position;

            targetRotCharactor = follow.transform.localRotation;
            targetRotCamera = transform.localRotation;

        }

        void LateUpdate()
        {
            transform.position = follow.transform.position + offset;
            UpdateRotation();
        }

        //マウスに合わせてカメラの角度を変更
        void UpdateRotation()
        {
            //Horizontal
            float yRot = Input.GetAxis("Right Horizontal") * sensitivity.x;
            targetRotCharactor *= Quaternion.Euler(0f, yRot, 0f);

            transform.localRotation = targetRotCharactor;

            //Vertical
            float xRot = Input.GetAxis("Right Vertical") * sensitivity.y;
            targetRotCamera *= Quaternion.Euler(-xRot, 0f, 0f);
            targetRotCamera = ClampRotationAroundXAxis(targetRotCamera);

            transform.localRotation *= targetRotCamera;
        }

        //上限下限にあわせて角度を変更
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, clampRotationMin, clampRotationMax);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
            return q;
        }
    }
}
