using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemBehavior
{
    void UseItem(float charge);
    void DestroyItem();
}

