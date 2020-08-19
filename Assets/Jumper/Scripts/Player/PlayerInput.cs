using UnityEngine;
namespace Jumper.Rework.Scriptable
{
    [RequireComponent(typeof(Player))]
    public class PlayerInput : MonoBehaviour
    {
        Player m_Player;

        private void OnEnable()
        {
            m_Player = GetComponent<Player>();
            m_Player.m_IsPlayerDead = false;
        }

        private void Update()
        {
            if (!m_Player.m_IsPlayerDead)
            {
                Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                m_Player.SetDirectionalInput(directionalInput);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_Player.OnJumpInputDown();
                    m_Player.m_anim.SetBool("Jump", true);
                }
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    m_Player.OnJumpInputUp();
                    m_Player.m_anim.SetBool("Jump", true);
                }
                if (Input.GetKey(KeyCode.Space))
                {
                    return;
                }
                else
                    m_Player.m_anim.SetBool("Jump", false);
            }
            else
            {
                return;
            }
        }
    }
}
