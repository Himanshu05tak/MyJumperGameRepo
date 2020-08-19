using UnityEngine;

namespace Jumper.Rework.Scriptable
{
    [CreateAssetMenu(fileName ="JumperAssets",menuName ="Controller/Player")]
    public class PlayerControllerObject : ScriptableObject
    {
        public float InputDirection;
        public float VerticalVelocity;
        public float Speed;
        public float JumpForce;
        public float Gravity;
        public bool SecondJump;
        public CharacterController PlayerController;
        public Vector3 LastVector;
        public Vector3 InitalVector;
    }
}
