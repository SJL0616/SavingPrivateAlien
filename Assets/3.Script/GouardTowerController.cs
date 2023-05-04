using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GouardTowerController : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Color originalColor = Color.white;
    void intoNormalColor(){
        meshRenderer.material.color = originalColor;
    }
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        Invoke("intoNormalColor",0.2f);
        
    }
}
