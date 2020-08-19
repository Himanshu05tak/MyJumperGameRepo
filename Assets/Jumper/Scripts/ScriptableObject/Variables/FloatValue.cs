using UnityEngine;
namespace Jumper.Rework.Scriptable
{

    [CreateAssetMenu(fileName = "Assets", menuName = "DynamicAssets/Float")]
    public class FloatValue : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        public float Value;
    }
}


