using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairTarget : MonoBehaviour
{
    private Camera playerCamera;
    Ray ray;
    RaycastHit hitinfo;
    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null)
        {
            return;
        }

        ray.origin = playerCamera.transform.position;
        ray.direction = playerCamera.transform.forward;
        if (Physics.Raycast(ray, out hitinfo))
        {
            transform.position = hitinfo.point;
        }
        else
        {
            transform.position = ray.origin + ray.direction * 1000.0f;
        }
    }
}
