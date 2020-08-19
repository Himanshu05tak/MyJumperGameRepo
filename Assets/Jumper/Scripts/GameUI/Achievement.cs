using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Jumper.Rework.Scriptable
{
    public class Achievement : MonoBehaviour
    {
        public CircleCollider2D m_Circle;
        public LayerMask m_CollisionMask;
        public GameObject m_Finish;
        public Image m_TransitionColor;

        private void OnEnable()
        {
            m_Circle = GetComponent<CircleCollider2D>();
        }

        private void Update()
        {
            UpdateRaycast();
        }

        private void UpdateRaycast()
        {
            float rayLength = 0.1f;

            Vector2 originRay = new Vector2(m_Circle.bounds.center.x, m_Circle.bounds.center.y);
            RaycastHit2D hit = Physics2D.CircleCast(originRay, m_Circle.radius, Vector2.left, rayLength, m_CollisionMask);
            if (hit)
            {
                StartCoroutine(LevelCompleted());
            }
        }

        IEnumerator  LevelCompleted()
        {
            StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 1f), 1));
            m_Finish.SetActive(true);
            yield return new WaitForSeconds(1f);
        }

        public IEnumerator Fade(Color to, Color from, float time)
        {
            float transitionSpeed = 0.5f / time;
            float percentage = 0;
            while (percentage < 1)
            {
                percentage += Time.deltaTime * transitionSpeed;
                m_TransitionColor.color = Color.Lerp(to, from, percentage);
                yield return null;
            }

        }
    }
}


