using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PlayerControllerState
{
    Idle = 0,
    Jumping = 1,
    Falling = 2,
    Walking = 4,
    Running = 8
}
