using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatLamp : MonoBehaviour
{
    public Color mainColor;

    private enum LampState
    {
        off,
        normal,
        rainbow,
        stop
    }

    private LampState _lampState = LampState.off;
    private Material _material;
    private Gradient _rainbowGradient;
    private GradientColorKey[] _rainbowGradientKeys;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    private void Start() 
    {
        _rainbowGradient = new Gradient();
        _rainbowGradientKeys = new GradientColorKey[3];

        _rainbowGradientKeys[0].color = Color.red;
        _rainbowGradientKeys[0].time = 0;
        _rainbowGradientKeys[1].color = Color.green;
        _rainbowGradientKeys[1].time = 0.5f;
        _rainbowGradientKeys[2].color = Color.blue;
        _rainbowGradientKeys[2].time = 1f;

        _rainbowGradient.colorKeys = _rainbowGradientKeys;
    }

    private void Update()
    {
        if(_lampState == LampState.off)
        {  
            _material.DisableKeyword("_EMISSION");
        }
        else if(_lampState == LampState.normal)
        {
            _material.EnableKeyword("_EMISSION");
            _material.SetColor("_EmissionColor", mainColor);
        }
        else if(_lampState == LampState.rainbow)
        {
            float emission = Mathf.PingPong (Time.time * 0.05f, 1.0f);
 
            Color finalColor = _rainbowGradient.Evaluate(emission);
            _material.SetColor("_EmissionColor", finalColor);
        }
    }
   
    public void ChangeColor()
    {
        if(_lampState == LampState.stop)
        {
            _lampState = LampState.off;
        }
        else
        {
            _lampState += 1;
        }        
    }
}
