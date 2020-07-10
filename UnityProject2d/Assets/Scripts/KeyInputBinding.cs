using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/InputBindings/KeyInputBinding")]
public class KeyInputBinding : InputBinding
{
    public KeyCode KeyForActivation;
    public KeyInputBindingType BindingType;
    public override float GetActivation()
    {
        switch (BindingType)
        {
            case KeyInputBindingType.Down:
                return Input.GetKey(KeyForActivation) ? 1 : 0;
            case KeyInputBindingType.Up:
                return !Input.GetKey(KeyForActivation) ? 1 : 0;
                break;
            case KeyInputBindingType.OnDown:
                return Input.GetKeyDown(KeyForActivation) ? 1 : 0;
            case KeyInputBindingType.OnUp:
                return Input.GetKeyUp(KeyForActivation) ? 1 : 0;
        }

        return 0; //Default value, should not be called.
    }
}