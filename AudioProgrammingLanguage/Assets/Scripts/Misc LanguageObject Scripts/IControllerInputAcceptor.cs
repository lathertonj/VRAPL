using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllerInputAcceptor 
{
    void TouchpadDown();
    void TouchpadUp();
    void TouchpadAxis( Vector2 pos );
    // sent only between TouchpadDown and TouchpadUp
    void TouchpadTransform( Transform touchpad );
}
