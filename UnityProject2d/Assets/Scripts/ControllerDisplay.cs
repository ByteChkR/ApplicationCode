using UnityEngine;
using UnityEngine.UI;

public class ControllerDisplay : MonoBehaviour
{
    public PlayerController Controller;
    public Text StateText;
    public Text AccelText;
    public Text VelocText;
    
    // Update is called once per frame
    private void LateUpdate()
    {
        StateText.text = $"Controller State: {Controller.ControllerState}";
        AccelText.text = $"Acceleration: {Controller.Acceleration}";
        VelocText.text = $"Velocity: {Controller.Velocity}";
    }
}
