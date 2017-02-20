using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common;
using Assets.Scripts.Core.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.Models
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
        public List<GameObject> jumpThings = new List<GameObject>();

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
                if (!m_IsDeading)
                {
                    m_JumpCount = 0;
                    maxJumpCount = 2;
                }

                m_Animator.Play("Run");
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.tag == "JumpThing")
            {
                jumpThings.Add(col.gameObject);
                col.gameObject.SetActive(false);
                maxJumpCount = 99999;
            }

            if (col.gameObject.tag == "Love")
            {
                m_Animator.Play("Idle");
                ScenesManager.instance.StopAllTwners();
                col.transform.position = Vector3.zero;
                col.gameObject.GetComponent<SpriteRenderer>().DOFade(0.5f, 1.0f);
                col.transform.DOScale(Vector3.one * 10.0f, 1.0f).OnComplete(() =>
                {
                    if (++ScenesManager.instance.sceneIdx < ScenesManager.instance.sceneConfig.Count)
                    {
                        ScenesManager.instance.GameStart(ScenesManager.instance.sceneIdx);
                    }
                });
            }

            if (col.gameObject.tag == "Stack" && !m_IsDeading)
            {
                m_Animator.Play("Idle");
                ScenesManager.instance.StopAllTwners();
                Dead();
            }

            if (col.gameObject.tag == "Boy")
            {
                m_IsDeading = true;
                m_Animator.Play("Idle");
                GetComponent<Collider2D>().enabled = false;
                ScenesManager.instance.StopAllTwners();
                ScenesManager.instance.End();
            }
        }

        private void Update()
        {
            if ((Mathf.Abs(transform.position.y) > Camera.main.orthographicSize + 1.0f || Mathf.Abs(transform.position.x + 4.5f) > 0.1f) && !m_IsDeading)
            {
                Dead();
            }
        }

        public void Dead()
        {
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;
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
                m_DeadSequence.Kill();
            }
            m_Animator.Play("Run");
            jumpThings.ForEach(j => j.SetActive(true));
            GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(s =>
            {
                s.DOFade(0.3f, 0.3f).SetLoops(10, LoopType.Yoyo).OnComplete(() =>
                {
                    s.DOFade(1.0f, 0.3f);
                });
            });

            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().simulated = true;
            GetComponent<Collider2D>().enabled = false;
            DOTweenUtils.Delay(() =>
            {
                GetComponent<Collider2D>().enabled = true;
                m_IsDeading = false;
            }, 3.0f);
            maxJumpCount = 99999;
            m_JumpCount = 0;
        }
    }
}