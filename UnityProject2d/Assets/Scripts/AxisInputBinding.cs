using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/InputBindings/AxisInputBinding")]
public class AxisInputBinding : InputBinding
{
    [Flags]
    public enum AxisManipulationType
    {
        None,
        Absolute = 1,
        Clamp = 2,
        Negative = 4
    }

    private static Dictionary<AxisManipulationType, Func<float, float>> ManipulationTypes = new Dictionary<AxisManipulationType, Func<float, float>>
    {
        {AxisManipulationType.None, f => f },
        {AxisManipulationType.Absolute, Mathf.Abs },
        {AxisManipulationType.Negative, f => -f },
        {AxisManipulationType.Clamp, Mathf.Clamp01 },
    };

    public string AxisName;
    public AxisManipulationType ManipulationType;
    public override float GetActivation()
    {
        float value = Input.GetAxis(AxisName);
        
        foreach (KeyValuePair<AxisManipulationType, Func<float, float>> manipulationType in ManipulationTypes)
        {
            if ((manipulationType.Key & ManipulationType) != 0)
            {
                value = manipulationType.Value(value);
            }
        }

        return value;
    }
}