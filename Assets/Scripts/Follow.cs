using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject followDest;
    public Vector3 nextpos;

    // Start is called before the first frame update
    void Start()
    {
        /*nextpos = new Vector3(0, 0, 1);*/
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = followDest.transform.position + nextpos;
    }
}
