using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Jumper.Rework.Scriptable
{
    [RequireComponent(typeof(PlayerController))]
    public class Player : MonoBehaviour
    {
        public Animator m_anim;
        public float m_MaxJumpHeight;
        public float m_MiniJumpHeight;
        public float m_TimetoJumpApex;
        public float m_WallSlideMaxSpeed;
        public Vector2 m_WallJumpClimb;
        public Vector2 m_WallJumpOff;
        public Vector2 m_WallJumpLeap;
        public GameObject m_Env;
        public GameObject m_gameOver;
        public Image m_TransitionColor;

        private float m_MoveSpeed = 15;
        private float m_AccelerationTimeInAirbone = .2f;
        private float m_AccelerationTimeInGrounded = .15f;
        private float m_WallUnstickTime = 0.25f;
        private float m_WallStickTime;

        private PlayerController m_Controller;
        private Vector2 m_Velocity;
        private float m_Gravity;
        private float m_MaxJumpVelocity;
        private float m_MiniJumpVelocity;
        private float m_SmoothVelocityX;
        private Vector2 m_DirectionalInput;
        private bool m_WallSliding;
        private int WallDirX;
        private SkinnedMeshRenderer m_SkinMeshRender;
        public bool m_IsPlayerDead;

        private void OnEnable()
        {
            m_Controller = GetComponent<PlayerController>();
            m_anim = GetComponent<Animator>();
            var parrentObj = GetComponentsInChildren<Transform>()[1];
            m_SkinMeshRender = parrentObj.GetComponentsInChildren<SkinnedMeshRenderer>()[0];
            m_Gravity = -(2 * m_MaxJumpHeight) / Mathf.Pow(m_TimetoJumpApex, 2);
            m_MaxJumpVelocity = Mathf.Abs(m_Gravity * m_TimetoJumpApex);
            m_MiniJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(m_Gravity) * m_MiniJumpHeight);
            //m_SkinMeshRender.sharedMaterials[0].shader = Shader.Find("Shader Graphs/PlayerFresnalGlow");
            //m_SkinMeshRender.sharedMaterials[1].shader = Shader.Find("Shader Graphs/PlayerFresnalGlow");
        }

        private void Update()
        {
            IsPlayerAlive();
            CalculateVelocity();
            HandleWallSliding();
            m_Controller.Move(m_Velocity * Time.deltaTime, m_DirectionalInput, m_anim);
            if (m_Controller.m_CollisionInfo.above || m_Controller.m_CollisionInfo.below)
            {
                if (m_Controller.m_CollisionInfo.slidingDownMaxSlope)
                    m_Velocity.y += m_Controller.m_CollisionInfo.slopeNormal.y * -m_Gravity * Time.deltaTime;
                else
                    m_Velocity.y = 0;
            }
        }

        private void CalculateVelocity()
        {
            var targetVelocity = m_DirectionalInput.x * m_MoveSpeed * Time.deltaTime;
            m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocity, ref m_SmoothVelocityX, m_Controller.m_CollisionInfo.below ? m_AccelerationTimeInGrounded : m_AccelerationTimeInAirbone);
            m_Velocity.y += m_Gravity * Time.deltaTime;

            if (m_Velocity.x != 0)
            {
                if (m_Velocity.x > 0)
                {
                    m_anim.SetFloat("Speed", m_Velocity.x);
                    transform.localScale = new Vector3(1, 1, 1);
                }
                else
                {
                    m_anim.SetFloat("Speed", -m_Velocity.x);
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            //else
            //{
            //    m_anim.SetFloat("Speed", m_Velocity.x);
            //    m_anim.SetBool("Jump", false);
            //    m_anim.SetBool("Slide", false);
            //}
        }


        private void HandleWallSliding()
        {
            WallDirX = m_Controller.m_CollisionInfo.left ? -1 : 1;
            m_WallSliding = false;
            if ((m_Controller.m_CollisionInfo.left || m_Controller.m_CollisionInfo.right) && !m_Controller.m_CollisionInfo.below && m_Velocity.y < 0)
            {
                m_WallSliding = true;
                if (m_Velocity.y < -m_WallSlideMaxSpeed)
                    m_Velocity.y = -m_WallSlideMaxSpeed;
                if (m_WallUnstickTime > 0)
                {
                    m_Velocity.x = 0;
                    m_SmoothVelocityX = 0;
                    if (m_Velocity.x != WallDirX && m_Velocity.x != 0)
                        m_WallUnstickTime -= Time.deltaTime;
                    else
                        m_WallUnstickTime = m_WallStickTime;
                }
            }
        }

        public void SetDirectionalInput(Vector2 input)
        {
            m_DirectionalInput = input;
        }

        public void OnJumpInputDown()
        {
            if (m_WallSliding)
            {
                if (WallDirX == m_DirectionalInput.x)
                {
                    m_Velocity.x = -WallDirX * m_WallJumpClimb.x;
                    m_Velocity.y = m_WallJumpClimb.y;
                }
                else if (m_DirectionalInput.x == 0)
                {
                    m_Velocity.x = -WallDirX * m_WallJumpOff.x;
                    m_Velocity.y = m_WallJumpOff.y;
                }
                else
                {
                    m_Velocity.x = -WallDirX * m_WallJumpLeap.x;
                    m_Velocity.y = m_WallJumpLeap.y;
                }
            }
            if (m_Controller.m_CollisionInfo.below)
            {
                if (m_Controller.m_CollisionInfo.slidingDownMaxSlope)
                {
                    if (m_DirectionalInput.x != -Mathf.Sign(m_Controller.m_CollisionInfo.slopeNormal.x))
                    {
                        m_Velocity.x = m_MaxJumpVelocity * m_Controller.m_CollisionInfo.slopeNormal.x;
                        m_Velocity.y = m_MaxJumpVelocity * m_Controller.m_CollisionInfo.slopeNormal.y;
                    }
                }
                else
                    m_Velocity.y = m_MaxJumpVelocity;
            }
        }

        public void OnJumpInputUp()
        {
            if (m_Velocity.y > m_MiniJumpHeight) //&& m_Controller.m_CollisionInfo.below)
                m_Velocity.y = m_MiniJumpVelocity;
        }

        private bool IsPlayerAlive()
        {
           
            if (transform.position.y < 0)
            {
                StartCoroutine(IsDead());
                m_Velocity = Vector3.zero;
                return true;
            }
            return false;
        }

        public IEnumerator IsDead()
        {
            m_anim.SetBool("IsDead", true);
            //m_SkinMeshRender.sharedMaterials[0].shader = Shader.Find("Shader Graphs/PlayerDissolveEffect");
            //m_SkinMeshRender.sharedMaterials[1].shader = Shader.Find("Shader Graphs/PlayerDissolveEffect");
            m_IsPlayerDead = true;
            StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 0.95f), 1));
            yield return new WaitForSeconds(1f);
            m_gameOver.SetActive(true);
            m_TransitionColor.enabled = false;
            //m_Env.SetActive(false);
        }

        public IEnumerator Fade(Color to, Color from, float time)
        {
            float transitionSpeed = 0.5f/ time;
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