using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{    
    private bool _rotate;

    private void Start()
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _rotate = true; 
        }

        if(_rotate)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(new Vector3(-90, 0, 90)), Time.deltaTime);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {

        }
    }
}
