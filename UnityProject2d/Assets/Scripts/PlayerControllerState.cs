using System;

[Flags]
public enum PlayerControllerState
{
    Idle = 0,
    Jumping = 1,
    Falling = 2,
    Walking = 4,
    Running = 8
}
