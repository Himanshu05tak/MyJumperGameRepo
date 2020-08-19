using UnityEngine;
namespace Jumper.Rework.Scriptable
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastController : MonoBehaviour
    {
        public const float m_boundSkinWidth = 0.0015f;
        public const float m_DstBtwRay = 0.0025f;
        public LayerMask m_CollisionMask;
        public RaycastOrigins m_RaycastOrigin;

        [HideInInspector]
        public int m_HorizontalRayCount;
        [HideInInspector]
        public int m_VerticalRayCount;
        [HideInInspector]
        public BoxCollider2D m_Collider;
        [HideInInspector]
        public float m_HorizontalRaySpacing;
        [HideInInspector]
        public float m_VerticalRaySpacing;

        public void Awake()
        {
            m_Collider = GetComponent<BoxCollider2D>();
        }

        public virtual void OnEnable()
        {
            CalculateRaySpacing();
        }

        public void UpdateRaycastOrigins()
        {
            Bounds bounds = m_Collider.bounds;
            bounds.Expand(m_boundSkinWidth * -2);

            m_RaycastOrigin.bottomLeft = new Vector2(bounds.min.x , bounds.min.y);
            m_RaycastOrigin.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            m_RaycastOrigin.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            m_RaycastOrigin.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void CalculateRaySpacing()
        {
            Bounds bounds = m_Collider.bounds;
            bounds.Expand(m_boundSkinWidth * -2);

            float boundWidth = bounds.size.x;
            float boundHeight = bounds.size.y;

            m_HorizontalRayCount = Mathf.RoundToInt(boundHeight / m_DstBtwRay);
            m_VerticalRayCount = Mathf.RoundToInt(boundWidth / m_DstBtwRay);
            //m_HorizontalRayCount = Mathf.Clamp(m_HorizontalRayCount, 2, int.MaxValue);
            //m_VerticalRayCount = Mathf.Clamp(m_VerticalRayCount, 2, int.MaxValue);
            m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
            m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
        }

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }
    }
}

