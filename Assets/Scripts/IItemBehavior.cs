using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemBehavior
{
    string GetItemType();
    void UseItem(float charge);
    void DestroyItem();
}

