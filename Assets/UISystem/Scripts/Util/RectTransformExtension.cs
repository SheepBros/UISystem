using UnityEngine;

namespace SB.UI
{
    public static class RectTransformExtension
    {
        public static void Identify(this RectTransform transform)
        {
            transform.anchoredPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
    }
}