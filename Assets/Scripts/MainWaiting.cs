using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainWaiting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Waiting());
    }

    IEnumerator Waiting()
    {
        yield return new WaitForSeconds(3f);
    }
}
