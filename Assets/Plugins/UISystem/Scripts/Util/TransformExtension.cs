using UnityEngine;

namespace SB.UI
{
    public static class TransformExtension
    {
        public static void Identify(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
    }
}