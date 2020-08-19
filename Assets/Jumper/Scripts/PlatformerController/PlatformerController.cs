using UnityEngine;
using System.Collections.Generic;
namespace Jumper.Rework.Scriptable
{

    public class PlatformerController : RaycastController
    {
        public LayerMask m_PassengerMask;
        public Vector3[] m_LocalWaypoints;
        public bool m_IsCyclic;
        public float m_WaitingOnPlatform;
        public float m_PlatformMovementSpeed;
        [Range(0, 2)]
        public float m_EaseAmount;

        int m_FromWaypointIndexTo;
        float m_PercentageBtwWaypoints;
        float m_NextMoveWaypointsTime;

        Vector3[] m_GlobalWayPoints;
        List<PassengerMovement> m_PassengerMovementList;
        Dictionary<Transform, PlayerController> passengerDictionary = new Dictionary<Transform, PlayerController>();

        public override void OnEnable()
        {
            base.OnEnable();
            m_GlobalWayPoints = new Vector3[m_LocalWaypoints.Length];
            for (int i = 0; i < m_LocalWaypoints.Length; i++)
            {
                m_GlobalWayPoints[i] = m_LocalWaypoints[i] + transform.position;
            }
        }

        void Update()
        {
            UpdateRaycastOrigins();
            Vector2 velocity = CalculatePassengerNonStops();
            CalculatePassengerMovement(velocity);
            MovePassengers(true);
            transform.Translate(velocity);
            MovePassengers(false);
        }

        float Ease(float x)
        {
            float a = m_EaseAmount + 1;
            return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
        }

        private Vector3 CalculatePassengerNonStops()
        {
            if (Time.time < m_NextMoveWaypointsTime)
                return Vector3.zero;
            m_FromWaypointIndexTo %= m_GlobalWayPoints.Length;
            int m_ToWayPointIndex = (m_FromWaypointIndexTo + 1) % m_GlobalWayPoints.Length;
            float distanceBtwWaypoints = Vector3.Distance(m_GlobalWayPoints[m_FromWaypointIndexTo], m_GlobalWayPoints[m_ToWayPointIndex]);
            m_PercentageBtwWaypoints += Time.deltaTime * m_PlatformMovementSpeed / distanceBtwWaypoints;
            m_PercentageBtwWaypoints = Mathf.Clamp01(m_PercentageBtwWaypoints);
            float easePercentageBtwWaypoints = Ease(m_PercentageBtwWaypoints);
            Vector3 newPos = Vector3.Lerp(m_GlobalWayPoints[m_FromWaypointIndexTo], m_GlobalWayPoints[m_ToWayPointIndex], easePercentageBtwWaypoints);
            if (m_PercentageBtwWaypoints >= 1)
            {
                m_PercentageBtwWaypoints = 0;
                m_FromWaypointIndexTo++;
                if (!m_IsCyclic)
                {
                    if (m_FromWaypointIndexTo >= m_GlobalWayPoints.Length - 1)
                    {
                        m_FromWaypointIndexTo = 0;
                        System.Array.Reverse(m_GlobalWayPoints);
                    }
                }
                m_NextMoveWaypointsTime = Time.time + m_WaitingOnPlatform;
            }
            return newPos - transform.position;
        }

        void MovePassengers(bool beforeMovePlatform)
        {
            foreach (PassengerMovement passenger in m_PassengerMovementList)
            {
                if (!passengerDictionary.ContainsKey(passenger.passengerTransform))
                    passengerDictionary.Add(passenger.passengerTransform, passenger.passengerTransform.GetComponent<PlayerController>());
                if (passenger.passengerMoveBeforePlatform == beforeMovePlatform)
                    passengerDictionary[passenger.passengerTransform].Move(passenger.passengerVelocity, passenger.passengerStandingOnPlatform);
            }
        }

        void CalculatePassengerMovement(Vector2 velocity)
        {
            HashSet<Transform> movedPassengers = new HashSet<Transform>();
            m_PassengerMovementList = new List<PassengerMovement>();
            float DirectionX = Mathf.Sign(velocity.x);
            float DirectionY = Mathf.Sign(velocity.y);

            if (velocity.x != 0)
            {
                float rayLength = Mathf.Abs(velocity.x) + m_boundSkinWidth;
                for (int i = 0; i < m_HorizontalRayCount; i++)
                {
                    Vector2 rayOrigin = (DirectionX == 1) ? m_RaycastOrigin.bottomRight : m_RaycastOrigin.bottomLeft;
                    rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
                    //Debug.DrawRay(rayOrigin, (DirectionX == -1) ? Vector2.left : Vector2.right, Color.red);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * DirectionX, rayLength, m_PassengerMask);
                    if (hit)
                    {
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            float pushX = velocity.x - (hit.distance - m_boundSkinWidth) * DirectionX;
                            float pushY = -m_boundSkinWidth;
                            m_PassengerMovementList.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), false, true));
                        }
                    }
                }
            }

            if (velocity.y != 0 || velocity.y == 0)
            {
                float rayLength = Mathf.Abs(velocity.y) + m_boundSkinWidth;
                for (int i = 0; i < m_VerticalRayCount; i++)
                {
                    Vector2 rayOrigin = (DirectionY == -1) ? m_RaycastOrigin.bottomLeft : m_RaycastOrigin.topLeft;
                    rayOrigin += Vector2.right * (m_VerticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * DirectionY, rayLength, m_PassengerMask);
                    if (hit)
                    {
                        Debug.DrawRay(rayOrigin, Vector2.down * DirectionY, Color.yellow, rayLength);
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            float pushX = (DirectionY == 1) ? velocity.x : 0;
                            float pushY = velocity.y - (hit.distance - m_boundSkinWidth) * DirectionY;
                            m_PassengerMovementList.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), DirectionY == 1, true));
                        }
                    }
                }
            }

            if (DirectionY == -1 || velocity.y == 0 && velocity.x != 0)
            {
                float rayLength = m_boundSkinWidth * 2;
                for (int i = 0; i < m_VerticalRayCount; i++)
                {
                    Vector2 rayOrigin = m_RaycastOrigin.topLeft + Vector2.right * (m_VerticalRaySpacing * i);
                    Debug.DrawRay(rayOrigin, (DirectionY == -1) ? Vector2.down : Vector2.up, Color.yellow);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, m_PassengerMask);
                    if (hit)
                    {
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            float pushX = velocity.x;
                            float pushY = velocity.y;
                            m_PassengerMovementList.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), true, false));
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (m_LocalWaypoints != null)
            {
                Gizmos.color = Color.yellow;
                float size = 0.1f;
                for (int i = 0; i < m_LocalWaypoints.Length; i++)
                {
                    Vector3 globalWaypointPos = Application.isPlaying ? m_GlobalWayPoints[i] : m_LocalWaypoints[i] + transform.position;
                    Gizmos.DrawLine(globalWaypointPos + Vector3.up * size, globalWaypointPos - Vector3.up * size);
                    Gizmos.DrawLine(globalWaypointPos + Vector3.left * size, globalWaypointPos - Vector3.left * size);
                    Gizmos.DrawLine(globalWaypointPos + (Vector3.up - Vector3.left) * size, globalWaypointPos - (Vector3.up - Vector3.left) * size);
                    Gizmos.DrawLine(globalWaypointPos + (Vector3.up + Vector3.left) * size, globalWaypointPos - (Vector3.up + Vector3.left) * size);
                }
            }
        }

        struct PassengerMovement
        {
            public Transform passengerTransform;
            public Vector2 passengerVelocity;
            public bool passengerStandingOnPlatform;
            public bool passengerMoveBeforePlatform;

            public PassengerMovement(Transform _passengerTransform, Vector2 _passengerVelocity, bool _passengerStandingOnPlatform, bool _passengerMoveBeforePlatform)
            {
                passengerTransform = _passengerTransform;
                passengerVelocity = _passengerVelocity;
                passengerStandingOnPlatform = _passengerStandingOnPlatform;
                passengerMoveBeforePlatform = _passengerMoveBeforePlatform;
            }
        }
    }
}


