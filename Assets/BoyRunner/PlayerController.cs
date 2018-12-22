using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoyRunner
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Layer Setting")]

        public LayerMask groundLayer = 1;

        [Header("Basic Setting")]

        [SerializeField] [Range(0.1f, 20f)] float runSpeed = 10.0f;
        [SerializeField] [Range(0.1f, 10f)] float jumpVelocity = 5.0f;

        //private property
        float speed = 0;
        bool isJumping = false;
        float angulerVelocity = 50.0f;
        Vector2 horizontalVelocity = Vector2.zero;

        void Update()
        {
            horizontalVelocity.x = Input.GetAxis("Horizontal");
            horizontalVelocity.y = Input.GetAxis("Vertical");

            if(Input.GetKeyDown(KeyCode.Space)) {
                Jump();
            }
        }

        void FixedUpdate()
        {
            JumpUpdate();

            UpdateDirection();
            UpdateLocomotion();
            UpdateAnimation();
        }

        //--------------------------------------------------

        //移動
        private void UpdateLocomotion()
        {
            speed = Mathf.Abs(horizontalVelocity.x) + Mathf.Abs(horizontalVelocity.y);
            speed = Mathf.Clamp01(speed);

            if (isJumping)
            {
                speed *= 0.4f;
            }

            if (speed > 0f)
            {
                Vector3 velocity = transform.forward * runSpeed * speed;
                velocity.y = _rigidbody.velocity.y;
                _rigidbody.velocity = velocity;
            }
        }

        //回転
        private void UpdateDirection()
        {
            if (!Camera.main.transform) return;

            var forward = Camera.main.transform.forward;
            var right = Camera.main.transform.right;
            forward.y = 0;
            Vector3 targetDirection = horizontalVelocity.x * right + horizontalVelocity.y * forward;

            if (horizontalVelocity != Vector2.zero && targetDirection.magnitude > 0.1f)
            {
                var lookDirection = targetDirection.normalized;
                var targetRotation = Quaternion.LookRotation(lookDirection, transform.up);
                var euler = transform.eulerAngles;
                euler.y = targetRotation.eulerAngles.y;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), angulerVelocity * Time.fixedDeltaTime);
            }
        }

        //アニメーションの切り替え
        private void UpdateAnimation()
        {
            if (!animator.enabled) return;

            animator.SetBool("IsGrounded", IsGround());
            animator.SetFloat("GroundDistance", GetGroundDistance());
            animator.SetFloat("VerticalVelocity", 0);

            if (!IsGround())
            {
                animator.SetFloat("VerticalVelocity", _rigidbody.velocity.y);
            }

            animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        }

        //--------------------------------------------------
        //ジャンプ

        private void Jump()
        {
            if (isJumping) return;
            isJumping = true;

            var vel = _rigidbody.velocity;
            vel.y = jumpVelocity;
            _rigidbody.velocity = vel;
        }

        private void JumpUpdate()
        {
            if (!isJumping) return;

            //地面に近づいたらジャンプ終了
            if (_rigidbody.velocity.y < 0 && IsGround())
            {
                isJumping = false;
            }
        }

        //--------------------------------------------------

        #region IsGround

        //地面と接しているかどうか
        private bool IsGround()
        {
            if (GetGroundDistance() > 0.05f)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //地面との距離を取得する
        private float GetGroundDistance()
        {
            var distance = 10f;
            var layLength = 10f;
            RaycastHit groundHit;

            // スフィアを飛ばして足場との距離を取る
            Ray ray = new Ray(transform.position + Vector3.up * (_capsuleCollider.radius), Vector3.down);
            if (Physics.SphereCast(ray, _capsuleCollider.radius * 0.9f, out groundHit, _capsuleCollider.radius + layLength, groundLayer))
            {
                distance = (groundHit.distance - _capsuleCollider.radius * 0.1f);
            }

            return (float)System.Math.Round(distance, 2);
        }

        #endregion

        #region DelayMethod

        private IEnumerator DelayMethod(float waitTime, Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action();
        }

        #endregion

        #region Components

        private Rigidbody m_Rigidbody;
        public Rigidbody _rigidbody
        {
            get
            {
                if (m_Rigidbody == null) m_Rigidbody = GetComponent<Rigidbody>();
                return m_Rigidbody;
            }
        }

        private CapsuleCollider m_CapsuleCollider;
        public CapsuleCollider _capsuleCollider
        {
            get
            {
                if (m_CapsuleCollider == null) m_CapsuleCollider = GetComponent<CapsuleCollider>();
                return m_CapsuleCollider;
            }
        }

        private Animator m_Animator;
        public Animator animator
        {
            get
            {
                if (m_Animator == null) m_Animator = GetComponent<Animator>();
                return m_Animator;
            }
        }

        #endregion
    }
}
