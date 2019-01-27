using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceLight : MonoBehaviour
{
    private Light _light;
    private float _intensitySpeed = 0.05f;
    private float _rangeSpeed = 0.025f;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Update()
    {
        _light.intensity = 1f + Mathf.PingPong(Time.time * _intensitySpeed, 1.0f);
        _light.range = 8f + Mathf.PingPong(Time.time * _rangeSpeed, 1.0f);
    }
}
