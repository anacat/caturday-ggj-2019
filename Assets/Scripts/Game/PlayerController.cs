﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool _interact;

    private float _interactionCooldown = 1f;
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
        else if(other.CompareTag("CatLamp"))
        {
            other.GetComponent<CatLamp>().ChangeColor();

            _timer = 0;
        }
        else if(other.CompareTag("Bath"))
        {
            other.GetComponent<BubbleBath>().StartBubbleBath();

            _timer = 0;
        }
        else if(other.CompareTag("Fireplace"))
        {
            other.GetComponent<Fireplace>().TurnFireplaceOn();

            _timer = 0;
        }
    }
}
