using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceLight : MonoBehaviour
{
    private Light _light;
    private float _intensitySpeed;
    private float _rangeSpeed;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Update()
    {
        _intensitySpeed = Random.Range(0.05f, 0.08f);
        _rangeSpeed = Random.Range(0.05f, 0.08f);

        _light.intensity = 5 + Mathf.PingPong(Time.time * _intensitySpeed, 1.0f);
        _light.range = 11 + Mathf.PingPong(Time.time * _rangeSpeed, 1.0f);
    }
}
