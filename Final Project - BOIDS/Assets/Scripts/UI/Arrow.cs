using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Material baseMat; 
    [SerializeField] private Material selectedMat; 
    [SerializeField] public bool isXAxis = false;
    private Renderer meshRenderer;

    void Start()
    {
        meshRenderer = this.gameObject.GetComponent<Renderer>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelect(){
        meshRenderer.material = selectedMat; 
    }

    public void OnDeselect(){
        meshRenderer.material = baseMat; 
    }
}
