using UnityEngine;

public static class Utils
{
    public static Transform GetTopMostParent(Transform currentTransform) => currentTransform.parent == null ? currentTransform : GetTopMostParent(currentTransform.parent);
}
