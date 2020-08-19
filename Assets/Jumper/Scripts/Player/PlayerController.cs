using UnityEngine;
namespace Jumper.Rework.Scriptable
{
    public class PlayerController : RaycastController
    {
        public float m_MaxSlopeAngle;

        public CollisionInfo m_CollisionInfo;
        [HideInInspector]
        public Vector2 m_PlayerInput;

        Player m_player;

        public override void OnEnable()
        {
            base.OnEnable();
            m_player = GetComponent<Player>();
            m_CollisionInfo.faceDir = 1;
        }

        public void Move(Vector2 moveAmount, bool standingOnPlatform)
        {
            Move(moveAmount, Vector2.zero, m_player.m_anim, standingOnPlatform);
        }

        public void Move(Vector2 moveAmount, Vector2 input, Animator anim, bool standingOnPlatform = false)
        {
            UpdateRaycastOrigins();
            m_CollisionInfo.Reset();
            m_CollisionInfo.oldVelocityX = moveAmount;
            m_PlayerInput = input;
            if (moveAmount.y < 0)
                DescendSlope(ref moveAmount, anim);
            if (moveAmount.x != 0)
                m_CollisionInfo.faceDir = (int)Mathf.Sign(moveAmount.x);
            HorizontalCollisionDetection(ref moveAmount);
            if (moveAmount.y != 0)
                VerticalCollisionDetection(ref moveAmount);
            transform.Translate(moveAmount);
            if (standingOnPlatform)
                m_CollisionInfo.below = true;
        }

        public void HorizontalCollisionDetection(ref Vector2 horizontalmoveAmount)
        {
            float directionX = m_CollisionInfo.faceDir;
            float rayWidth = Mathf.Abs(horizontalmoveAmount.x) + m_boundSkinWidth;
            if (Mathf.Abs(horizontalmoveAmount.x) < m_boundSkinWidth)
                rayWidth = 2 * m_boundSkinWidth;
            for (int i = 0; i < m_HorizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigin.bottomLeft : m_RaycastOrigin.bottomRight;
                rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayWidth, m_CollisionMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.white);
                if (hit)
                {
                    if (hit.distance == 0)
                        continue;
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (i == 0 && slopeAngle <= m_MaxSlopeAngle)
                    {
                        if (m_CollisionInfo.descendSlopeAngle)
                        {
                            m_CollisionInfo.descendSlopeAngle = false;
                            horizontalmoveAmount = m_CollisionInfo.oldVelocityX;
                        }
                        float distanceToStartSlope = 0;
                        if (slopeAngle != m_CollisionInfo.lastSlopeAngle)
                        {
                            distanceToStartSlope = hit.distance - m_boundSkinWidth;
                            horizontalmoveAmount.x -= distanceToStartSlope * directionX;
                        }
                        ClimbSlope(ref horizontalmoveAmount, slopeAngle, hit.normal);
                        horizontalmoveAmount.x += distanceToStartSlope * directionX;
                    }
                    if (!m_CollisionInfo.climbingSlopeAngle || slopeAngle > m_MaxSlopeAngle)
                    {
                        horizontalmoveAmount.x = (hit.distance - m_boundSkinWidth) * directionX;
                        rayWidth = hit.distance;

                        if (m_CollisionInfo.climbingSlopeAngle)
                            horizontalmoveAmount.y = Mathf.Tan(m_CollisionInfo.currentSlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(horizontalmoveAmount.x);

                        m_CollisionInfo.left = directionX == -1;
                        m_CollisionInfo.right = directionX == 1;
                    }
                }
            }
        }

        private void VerticalCollisionDetection(ref Vector2 verticalmoveAmount)
        {

            float directionY = Mathf.Sign(verticalmoveAmount.y);
            float rayLength = Mathf.Abs(verticalmoveAmount.y) + m_boundSkinWidth;
            for (int i = 0; i < m_VerticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigin.bottomLeft : m_RaycastOrigin.topLeft;
                rayOrigin += Vector2.right * (m_VerticalRaySpacing * i);// + verticalVelocity.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);
                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.white);
                if (hit)
                {
                    Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red,0.5f);
                    if (hit.collider.tag == "ThroughTheWall")
                    {
                        if (directionY == 1 || hit.distance == 0)
                            continue;
                        if (m_CollisionInfo.fallingThroughPlatform)
                            continue;
                        if (m_PlayerInput.y == -1)
                        {
                            m_CollisionInfo.fallingThroughPlatform = true;
                            Invoke("ResetFallingThroughPlatform", 0.5f);
                            continue;
                        }
                    }
                    verticalmoveAmount.y = (hit.distance - m_boundSkinWidth) * directionY;
                    rayLength = hit.distance;
                    if (m_CollisionInfo.climbingSlopeAngle)
                        verticalmoveAmount.x = verticalmoveAmount.y / Mathf.Tan(m_CollisionInfo.currentSlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(verticalmoveAmount.x);
                    m_CollisionInfo.below = directionY == -1;
                    m_CollisionInfo.above = directionY == 1;
                }
            }
            if (m_CollisionInfo.climbingSlopeAngle)
            {
                float directionX = Mathf.Sign(verticalmoveAmount.x);
                rayLength = Mathf.Abs(verticalmoveAmount.x) + m_boundSkinWidth;
                Vector2 rayOrigin = ((directionX == 1) ? m_RaycastOrigin.bottomRight : m_RaycastOrigin.bottomLeft) + Vector2.up * verticalmoveAmount.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, layerMask: m_CollisionMask);
                if (hit)
                    {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != m_CollisionInfo.currentSlopeAngle)
                    {
                        verticalmoveAmount.x = (hit.distance - m_boundSkinWidth) * directionX;
                        m_CollisionInfo.currentSlopeAngle = slopeAngle;
                        m_CollisionInfo.slopeNormal = hit.normal;
                    }
                }
            }
        }

        private void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
        {
            float moveDistance = Mathf.Abs(moveAmount.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
            if (moveAmount.y <= climbVelocityY)
            {
                moveAmount.y = climbVelocityY;
                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                m_CollisionInfo.below = true;
                m_CollisionInfo.climbingSlopeAngle = true;
                m_CollisionInfo.currentSlopeAngle = slopeAngle;
                m_CollisionInfo.slopeNormal = slopeNormal;
            }
        }

        private void DescendSlope(ref Vector2 moveAmount, Animator anim)
        {
            RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(m_RaycastOrigin.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + m_boundSkinWidth, m_CollisionMask);
            RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(m_RaycastOrigin.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + m_boundSkinWidth, m_CollisionMask);
            if (maxSlopeHitLeft ^ maxSlopeHitRight)
            //if (maxSlopeHitLeft || maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount, anim);
                SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount, anim);
            }
            if (!m_CollisionInfo.slidingDownMaxSlope)
            {
                anim.SetBool("Slide", false);
                float directionX = Mathf.Sign(moveAmount.x);
                Vector2 rayOrigin = (directionX == 1) ? m_RaycastOrigin.bottomLeft : m_RaycastOrigin.bottomRight;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down , Mathf.Infinity, layerMask: m_CollisionMask);
                if (hit)
                    {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != 0 && slopeAngle <= m_MaxSlopeAngle)
                    {
                        if (Mathf.Sign(hit.normal.x) == directionX)
                        {
                            if (hit.distance - m_boundSkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                            {
                                float moveDescendSlopeDistance = Mathf.Abs(moveAmount.x);
                                float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDescendSlopeDistance;
                                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDescendSlopeDistance * Mathf.Sign(moveAmount.x);
                                moveAmount.y -= descendVelocityY;
                                m_CollisionInfo.currentSlopeAngle = slopeAngle;
                                m_CollisionInfo.descendSlopeAngle = true;
                                m_CollisionInfo.below = true;
                                m_CollisionInfo.slopeNormal = hit.normal;
                            }
                        }
                    }
                }
            }
        }

        void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount, Animator anim)
        {
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle > m_MaxSlopeAngle)
                {
                    anim.SetBool("Slide", true);
                    moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
                    m_CollisionInfo.currentSlopeAngle = slopeAngle;
                    m_CollisionInfo.slidingDownMaxSlope = true;
                    m_CollisionInfo.slopeNormal = hit.normal;
                }
            }
        }

        public void ResetFallingThroughPlatform()
        {
            m_CollisionInfo.fallingThroughPlatform = false;
        }

        public struct CollisionInfo
        {
            public bool below, above;
            public bool left, right;
            public bool climbingSlopeAngle;
            public bool descendSlopeAngle;
            public bool slidingDownMaxSlope;
            public float currentSlopeAngle, lastSlopeAngle;
            public Vector2 oldVelocityX;
            public Vector2 slopeNormal;
            public int faceDir;
            public bool fallingThroughPlatform;

            public void Reset()
            {
                below = above = false;
                left = right = false;
                climbingSlopeAngle = false;
                descendSlopeAngle = false;
                slidingDownMaxSlope = false;
                slopeNormal = Vector2.zero;
                lastSlopeAngle = currentSlopeAngle;
                currentSlopeAngle = 0;
            }
        }
    }
}
