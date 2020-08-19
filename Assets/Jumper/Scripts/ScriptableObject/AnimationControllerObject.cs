using UnityEngine;
namespace Jumper.Rework.Scriptable
{
    [CreateAssetMenu(fileName = "JumperAssets", menuName = "Controller/Animation")]
    public class AnimationControllerObject : ScriptableObject
    {
        public Animator Anim;
    }
}
