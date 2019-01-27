using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool _interact;

    private float _interactionCooldown = 2f;
    private float _timer = 0;

    private void Update()    
    {
        if(_timer >= _interactionCooldown) 
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                _interact = true;
            }
            else if(Input.GetKeyUp(KeyCode.E))
            {
                _interact = false;
            }
        }
        else 
        {
            _timer += Time.deltaTime;
            _interact = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(!_interact)
        {
            return;
        }

        if(other.CompareTag("Door"))
        {
            other.GetComponent<DoorOpener>().OpenCloseDoor();

            _timer = 0;
        }
    }
}
