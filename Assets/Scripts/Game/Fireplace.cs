using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireplace : MonoBehaviour
{
    public GameObject fire;
    
    public void TurnFireplaceOn()
    {
        fire.SetActive(!fire.activeSelf);        
    }
}
