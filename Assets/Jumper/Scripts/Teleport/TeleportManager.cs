using TMPro;
using UnityEngine;
namespace Jumper.Rework.Scriptable
{
    public class TeleportManager : MonoBehaviour
    {
        public CircleCollider2D m_Circle;
        public LayerMask m_CollisionMask;
        public Transform m_TeleportPos;

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
                    Teleport(hit);
            }
        }

        private void Teleport(RaycastHit2D hit)
        {
            if (hit)
            {
                hit.transform.position = m_TeleportPos.position;
            }
        }
    }
}

