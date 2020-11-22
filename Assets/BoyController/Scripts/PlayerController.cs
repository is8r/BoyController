using UnityEngine;

namespace BoyController
{
    [RequireComponent (typeof (Rigidbody), typeof (CapsuleCollider), typeof (Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header ("Layer Setting")]

        public LayerMask groundLayer = 1;

        [Header ("Basic Setting")]

        [SerializeField][Range (0.1f, 10f)] float runSpeed = 5.0f;
        [SerializeField][Range (0.1f, 10f)] float jumpVelocity = 5.0f;

        //private property

        bool isJumping = false;
        Vector2 horizontalVelocity = Vector2.zero;

        Rigidbody rb;
        CapsuleCollider capsuleCollider;
        Animator animator;

        void Start ()
        {
            rb = GetComponent<Rigidbody> ();
            capsuleCollider = GetComponent<CapsuleCollider> ();
            animator = GetComponent<Animator> ();

            InitPhysicMaterial ();
        }

        void Update ()
        {
            horizontalVelocity.x = Input.GetAxis ("Horizontal");
            horizontalVelocity.y = Input.GetAxis ("Vertical");

            if (Input.GetButtonDown ("Jump"))
            {
                Jump ();
            }
        }

        void FixedUpdate ()
        {
            JumpUpdate ();

            UpdateLocomotion ();
            UpdateAnimation ();
        }

        //回転
        private void UpdateLocomotion()
        {
            if (!Camera.main.transform) return;

            var forward = Camera.main.transform.forward;
            var right = Camera.main.transform.right;
            forward.y = 0;

            Vector3 targetDirection = Vector3.zero;
            targetDirection += right * horizontalVelocity.x + forward * horizontalVelocity.y;

            Vector3 velocity = targetDirection * runSpeed;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;

            if (targetDirection.magnitude > 0.1f)
            {
                var lookDirection = targetDirection.normalized;
                var targetRotation = Quaternion.LookRotation(lookDirection, transform.up);
                var euler = transform.eulerAngles;
                euler.y = targetRotation.eulerAngles.y;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), 20 * Time.fixedDeltaTime);
            }
        }

        //アニメーションの切り替え
        private void UpdateAnimation ()
        {
            if (!animator.enabled) return;

            float speed = Mathf.Abs(horizontalVelocity.x) + Mathf.Abs(horizontalVelocity.y);
            speed = Mathf.Clamp01(speed);

            animator.SetBool ("IsGrounded", IsGround ());
            animator.SetFloat ("GroundDistance", GetGroundDistance ());
            animator.SetFloat ("VerticalVelocity", 0);

            if (!IsGround ())
            {
                animator.SetFloat ("VerticalVelocity", rb.velocity.y);
            }

            animator.SetFloat ("Speed", speed, 0.1f, Time.deltaTime);
        }

        //ジャンプ

        private void Jump ()
        {
            if (isJumping) return;
            isJumping = true;

            var vel = rb.velocity;
            vel.y = jumpVelocity;
            rb.velocity = vel;
        }

        private void JumpUpdate ()
        {
            if (!isJumping) return;

            //地面に近づいたらジャンプ終了
            if (rb.velocity.y < 0 && IsGround ())
            {
                isJumping = false;
            }
        }

        #region IsGround

        //地面と接しているかどうか
        private bool IsGround ()
        {
            if (GetGroundDistance () > 0.05f)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //地面との距離を取得する
        private float GetGroundDistance ()
        {
            var distance = 10f;
            var layLength = 10f;
            RaycastHit groundHit;

            // スフィアを飛ばして足場との距離を取る
            Ray ray = new Ray (transform.position + Vector3.up * (capsuleCollider.radius), Vector3.down);
            if (Physics.SphereCast (ray, capsuleCollider.radius * 0.9f, out groundHit, capsuleCollider.radius + layLength, groundLayer))
            {
                distance = (groundHit.distance - capsuleCollider.radius * 0.1f);
            }

            return (float) System.Math.Round (distance, 2);
        }

        #endregion

        #region InitPhysicMaterial

        //キャラクタ用のPhysicMaterialを作成してアタッチ
        private void InitPhysicMaterial ()
        {
            PhysicMaterial noFrictionPhysics = new PhysicMaterial ();
            noFrictionPhysics.name = "noFrictionPhysics";
            noFrictionPhysics.staticFriction = 0;
            noFrictionPhysics.dynamicFriction = 0;
            noFrictionPhysics.frictionCombine = PhysicMaterialCombine.Minimum;
            capsuleCollider.material = noFrictionPhysics;
        }

        #endregion

    }
}
