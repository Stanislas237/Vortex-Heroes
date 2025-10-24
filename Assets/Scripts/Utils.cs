using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    public static Transform GetTopMostParent(Transform currentTransform) => currentTransform.parent == null ? currentTransform : GetTopMostParent(currentTransform.parent);

    public static List<Transform> GetTransformsTag(Transform parent, string tag)
    {
        var list = new List<Transform>();
        foreach (var t in parent)
        {
            if (t.CompareTag(tag))
                list.Add(t);
            list.AddRange(GetTransformsTag(t, tag));
        }
        return list;
    }
}
