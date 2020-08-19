using UnityEngine;

namespace Jumper.Rework.Scriptable
{
    [CreateAssetMenu(fileName = "Assets", menuName = "DynamicAssets/Vector")]
    public class VectorValue : ScriptableObject
    {
        [Multiline]
        public string Description = "";
        public Vector2 Value; 
    }
}
