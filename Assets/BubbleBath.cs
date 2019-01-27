using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBath : MonoBehaviour
{
    public ParticleSystem _particleSystem;

    public void StartBubbleBath()
    {
        _particleSystem.gameObject.SetActive(true);
    }

    public void StopBubbleBath()
    {
        _particleSystem.gameObject.SetActive(false);
    }
}
