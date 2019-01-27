using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{   
    public Vector3 angle;

    private bool _rotate;
    private Vector3 _rotationVector;

    private void Start()
    {
        _rotationVector = angle;
    }

    private void Update()
    {
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
