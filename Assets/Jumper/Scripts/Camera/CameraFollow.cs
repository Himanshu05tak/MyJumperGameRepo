using UnityEngine;

namespace Jumper.Rework.Scriptable
{
    public class CameraFollow : MonoBehaviour
    {
        public PlayerController m_TargetController;
        public Vector2 m_FocusAreaSize;
        public Vector2 m_VerticalOffset;
        public float m_LookAheadDstX;
        public float m_LookSmoothTimeOnX;
        public float m_VerticalSmoothTime;

        FocusArea m_FocusArea;
        float m_CurrentLookAheadOnX;
        float m_TargetLookAHeadX;
        float m_LookAheadDirX;
        float m_SmoothVelocityX;
        float m_SmoothVelocityY;
        bool m_IsLookAheadStopped;

        public void Start()
        {
            m_FocusArea = new FocusArea(m_TargetController.m_Collider.bounds, m_FocusAreaSize);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, .5f);
            Gizmos.DrawCube(m_FocusArea.center, m_FocusAreaSize);
        }

        private void LateUpdate()
        {
            m_FocusArea.Update(m_TargetController.m_Collider.bounds);
            Vector2 focusPos = m_FocusArea.center + m_VerticalOffset;

            if (m_FocusArea.velocity.x != 0)
            {
                m_LookAheadDirX = Mathf.Sign(m_FocusArea.velocity.x);
                if (Mathf.Sign(m_TargetController.m_PlayerInput.x) == m_LookAheadDirX && m_TargetController.m_PlayerInput.x != 0)
                {
                    m_TargetLookAHeadX = m_LookAheadDirX * m_LookAheadDstX;
                    m_IsLookAheadStopped = false;
                }
                else
                {
                    if (!m_IsLookAheadStopped)
                    {
                        m_IsLookAheadStopped = true;
                        m_TargetLookAHeadX = m_CurrentLookAheadOnX + (m_LookAheadDirX * m_LookAheadDstX - m_CurrentLookAheadOnX) / 4f;
                    }
                }
            }
            m_CurrentLookAheadOnX = Mathf.SmoothDamp(m_CurrentLookAheadOnX, m_TargetLookAHeadX, ref m_SmoothVelocityX, m_LookSmoothTimeOnX);
            focusPos += Vector2.right * m_CurrentLookAheadOnX;
            focusPos.y = Mathf.SmoothDamp(transform.position.y, focusPos.y, ref m_SmoothVelocityY, m_VerticalSmoothTime);
            transform.position = (Vector3)focusPos + Vector3.forward * -10f;
        }
    }

    public struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;
        float left, right;
        float top, bottom;

        public FocusArea(Bounds targetBound, Vector2 size)
        {
            left = targetBound.center.x - size.x / 2;
            right = targetBound.center.x + size.x / 2;
            bottom = targetBound.min.y;
            top = targetBound.min.y + size.y;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = Vector2.zero;
        }

        public void Update(Bounds targetBound)
        {
            float shiftX = 0;
            float shiftY = 0;
            if (targetBound.min.x < left)
                shiftX = targetBound.min.x - left;
            else if (targetBound.max.x > right)
                shiftX = targetBound.max.x - right;
            if (targetBound.min.y < bottom)
                shiftY = targetBound.min.y - bottom;
            else if (targetBound.max.y > top)
                shiftY = targetBound.max.y - top;
            left += shiftX;
            right += shiftX;
            top += shiftY;
            bottom += shiftY;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }

    }
}
