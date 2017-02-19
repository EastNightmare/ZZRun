using Assets.Scripts.Core.Client.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Core.Client.Models
{
    public class Girl : MonoBehaviour
    {
        public Button btnJump;
        public float jumpForce;
        public Rigidbody2D body;
        public Vector3 startPos;
        public int maxJumpCount = 2;
        private int m_JumpCount = 0;
        private bool m_IsDeading = false;
        private Sequence m_DeadSequence;
        private Animator m_Animator;

        private void Start()
        {
            m_Animator = GetComponent<Animator>();
            transform.position = startPos;
            btnJump.onClick.AddListener(() =>
            {
                if (m_JumpCount <= maxJumpCount)
                {
                    m_JumpCount++;
                    m_Animator.Play("Jump");
                    body.velocity = Vector2.zero;
                    body.AddForce(jumpForce * Vector2.up);
                }
            });
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.tag == "Ground")
            {
                m_JumpCount = 0;
                maxJumpCount = 2;
                m_Animator.Play("Run");
            }
            else if (col.gameObject.tag == "Stack" && !m_IsDeading)
            {
                Dead();
            }
            else if (col.gameObject.tag == "Boy")
            {
                m_Animator.Play("Idle");
                ScenesManager.instance.StopAllTwners();
                if (++ScenesManager.instance.sceneIdx < ScenesManager.instance.sceneConfig.Count)
                {
                    ScenesManager.instance.GameStart(ScenesManager.instance.sceneIdx);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.tag == "JumpThing")
            {
                GameObject.Destroy(col.gameObject);
                maxJumpCount = 99999;
            }
        }

        private void Update()
        {
            if (Mathf.Abs(transform.position.y) > Camera.main.orthographicSize + 1.0f && !m_IsDeading)
            {
                Dead();
            }
        }

        public void Dead()
        {
            GetComponent<Collider2D>().enabled = false;
            m_Animator.Play("Idle");
            m_IsDeading = true;
            m_DeadSequence = DOTween.Sequence();
            var upY = 3.0f;
            var distance = Mathf.Abs(upY - transform.position.y);
            var upT = distance / 12.0f;
            var downY = -10.0f;
            var downT = (Mathf.Abs(transform.position.y - downY) + distance) / 15.0f;

            var twnUp = transform.DOMoveY(upY, upT).SetEase(Ease.OutSine);
            var twnDown = transform.DOMoveY(downY, downT).SetEase(Ease.InSine);
            m_DeadSequence.Append(twnUp);
            m_DeadSequence.Append(twnDown);

            ScenesManager.instance.Fail();
        }

        public void Reborn()
        {
            if (m_DeadSequence != null)
            {
                m_DeadSequence.Complete();
            }
            transform.position = Vector3.zero;
            GetComponent<Collider2D>().enabled = true;
            m_IsDeading = false;
        }
    }
}