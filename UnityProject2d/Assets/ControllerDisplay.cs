using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerDisplay : MonoBehaviour
{
    public PlayerController Controller;
    public Text StateText;
    public Text AccelText;
    public Text VelocText;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void LateUpdate()
    {
        StateText.text = $"Controller State: {Controller.ControllerState}";
        AccelText.text = $"Acceleration: {Controller.Acceleration}";
        VelocText.text = $"Velocity: {Controller.Velocity}";
    }
}
