using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class PlayerState
{
    public float xPos;
    public float yPos;
    public float movePower;
    public Vector2 velocity;

    public PlayerState(float xPos, float yPos, float movePower, Vector2 velocity)
    {
        this.xPos = xPos;
        this.yPos = yPos;
        this.movePower = movePower;
        this.velocity = velocity;

    }

    public override string ToString()
    {
        return "position: (" + xPos + ", " + yPos + "), power: " + movePower + ", velocity: " + velocity;
    } 



    
}
