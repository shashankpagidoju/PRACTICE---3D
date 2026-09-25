using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    [SerializeField]
    private StackPartController[] stackPartControls = null;

    public void ShatterAllParts()
    {
        if (transform.parent != null)
        {
            transform.parent = null;
            FindobjectOfType<Ball>().IncreaseBrokenStacks();
        }

        foreach (StackPartController o in stackPartControls)
        {
            o.Shatter();
        }
        StartCoroutine(RemoveParts());
    }

    IEnumerator RemoveParts()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}