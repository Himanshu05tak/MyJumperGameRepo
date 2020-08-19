using UnityEngine;

namespace Jumper.Rework.Scriptable
{
    public class VectorReference : MonoBehaviour
    {
        public bool IsContant = true;
        public Vector2 Constant;
        public VectorValue VariableValue;

        public Vector2 Value
        {
            get
            {
                return IsContant ? Constant : VariableValue.Value;
            }
        }

    }
}
