using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    public GameObject ReplacementObject;
    private Transform SpawnPosition;
    GameObject obj;
    public GameObject ReplaceObj;
    private Transform Parent;
    private MeshRenderer meshRenderer;
    public void DestroyAndReplace()
    {
        SpawnPosition = null;
        SpawnPosition = gameObject.transform;
        Parent = transform.parent;
        obj = Instantiate(ReplacementObject, transform.position, transform.rotation);
        StartCoroutine(Respawn(obj));
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }
    IEnumerator Respawn(GameObject obj)
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(obj);
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
    }
}
