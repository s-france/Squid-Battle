using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class PlayerState
{
    public float xPos;
    public float yPos;
    public float movePower;

    public PlayerState(float xPos, float yPos, float movePower)
    {
        this.xPos = xPos;
        this.yPos = yPos;
        this.movePower = movePower;
    }

    public override string ToString()
    {
        return "position: (" + xPos + ", " + yPos + "), power: " + movePower;
    } 



    
}