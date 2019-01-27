using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{    
    private bool _rotate;
    private Vector3 _rotationVector = new Vector3(0, 0, 90);

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _rotate = true; 
        }

        if(_rotate)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(_rotationVector), Time.deltaTime);
        }
        else 
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime);
        }
    }

    public void OpenCloseDoor()
    {
        if(_rotate)
        {
            _rotate = false;
        }
        else
        {
            _rotate = true;
        }        
    }
}
