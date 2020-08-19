using UnityEngine;
using System.Collections.Generic;

namespace Jumper.Rework.Scriptable
{
    public class SinglePlatformer : RaycastController
    {
        public LayerMask m_PassengerMask;
        public Vector2 m_MovePassengerDirection;
        public Vector3[] m_LocalwayPoints;
        public bool m_WaitingMovePassenger;
        public float m_moveSpeed;
        public float m_WaitingOnPlatform;

        float m_NextMoveWaypointsTime;
        HashSet<Transform> m_MovedPassengerTransformTracker;
        Vector3[] m_globalwayPoints;
        int m_FromWaypointIndex;
        int m_TowardNextWaypoint;
        float m_PercentageBtwWaypoints;

        private void Start()
        {
            m_globalwayPoints = new Vector3[m_LocalwayPoints.Length];
            for (int i = 0; i < m_LocalwayPoints.Length; i++)
                m_globalwayPoints[i] = m_LocalwayPoints[i] + transform.position;
        }

        private void Update()
        {
            UpdateRaycastOrigins();
            Vector3 velocity = CalculateMovement();
            PassengerMovement(velocity);
            if (m_WaitingMovePassenger)
                transform.Translate(velocity);
        }

        private Vector3 CalculateMovement()
        {
            if (Time.time < m_NextMoveWaypointsTime || m_FromWaypointIndex >= m_globalwayPoints.Length - 1)
                return Vector3.zero;
            m_FromWaypointIndex %= m_globalwayPoints.Length;
            m_TowardNextWaypoint = (m_FromWaypointIndex + 1) % m_globalwayPoints.Length;
            float distBtwWaypoints = Vector3.Distance(m_globalwayPoints[m_FromWaypointIndex], m_globalwayPoints[m_TowardNextWaypoint]);
            if (m_WaitingMovePassenger)
            {
                m_PercentageBtwWaypoints += Time.deltaTime * m_moveSpeed / distBtwWaypoints;
                if (m_PercentageBtwWaypoints >= 1)
                {
                    m_PercentageBtwWaypoints = 0;
                    m_FromWaypointIndex++;
                    m_NextMoveWaypointsTime = Time.time + m_WaitingOnPlatform;
                }
            }
            Vector3 newPos = Vector3.Lerp(m_globalwayPoints[m_FromWaypointIndex], m_globalwayPoints[m_TowardNextWaypoint], m_PercentageBtwWaypoints);
            return newPos - transform.position;
        }

        private void PassengerMovement(Vector2 moveVelocity)
        {
            float directionY = Mathf.Sign(moveVelocity.y);
            m_MovedPassengerTransformTracker = new HashSet<Transform>();
            if (moveVelocity.y != 0 || moveVelocity.y == 0)
            {
                float rayLength = Mathf.Abs(moveVelocity.y) + m_boundSkinWidth;
                for (int i = 0; i < m_VerticalRayCount; i++)
                {
                    Vector2 rayOrigin = (directionY == 1) ? m_RaycastOrigin.topLeft : m_RaycastOrigin.bottomLeft;
                    rayOrigin += Vector2.right * (m_VerticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_PassengerMask);
                    Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.blue, rayLength);
                    if (hit)
                    {
                        m_WaitingMovePassenger = true;
                        Debug.DrawRay(rayOrigin, Vector2.down * directionY, Color.yellow, rayLength);
                        if (!m_MovedPassengerTransformTracker.Contains(hit.transform))
                        {
                            m_MovedPassengerTransformTracker.Add(hit.transform);
                            float pushX = (directionY == 1) ? moveVelocity.x : 0;
                            float pushY = moveVelocity.y - (hit.distance - m_boundSkinWidth) * directionY;
                            hit.transform.Translate(new Vector2(pushX, pushY));
                        }
                    }
                }
            }

            if (directionY == -1 || moveVelocity.y == 0 && moveVelocity.x != 0)
            {
                float rayLength = m_boundSkinWidth * 2;
                for (int i = 0; i < m_VerticalRayCount; i++)
                {
                    Vector2 rayOrigin = m_RaycastOrigin.topLeft + Vector2.right * (m_VerticalRaySpacing * i);
                    Debug.DrawRay(rayOrigin, (directionY == -1) ? Vector2.down : Vector2.up, Color.yellow);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, m_PassengerMask);
                    if (hit)
                    {
                        m_WaitingMovePassenger = true;
                        if (!m_MovedPassengerTransformTracker.Contains(hit.transform))
                        {
                            m_MovedPassengerTransformTracker.Add(hit.transform);
                            float pushX = moveVelocity.x;
                            float pushY = moveVelocity.y;
                            hit.transform.Translate(new Vector2(pushX, pushY));
                            //m_PassengerMovementList.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), true, false));
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (m_LocalwayPoints != null)
            {
                Gizmos.color = Color.yellow;
                float size = 0.1f;
                for (int i = 0; i < m_LocalwayPoints.Length; i++)
                {
                    Vector3 globalPos = Application.isPlaying ? m_globalwayPoints[i] : m_LocalwayPoints[i] + transform.position;
                    Gizmos.DrawLine(globalPos + Vector3.up * size, globalPos - Vector3.up * size);
                    Gizmos.DrawLine(globalPos + Vector3.left * size, globalPos + Vector3.right * size);
                }
            }
        }
    }
}

