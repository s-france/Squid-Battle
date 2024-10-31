using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Numerics;
//using UnityEngine.UIElements;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class Arena2LC : ArenaLevelController
{
    Transform Box1;
    Transform Box2;

    [SerializeField] float moveDistance;
    [SerializeField] float moveSpeed;
    int moveDirection1;
    int moveDirection2;
    float arenaPosition;
    bool forward;
    UnityEngine.Vector2 movePos;


    // Start is called before the first frame update
    public override void Awake()
    {   
        Box1 = GameObject.Find("Box1").transform;
        Box2 = GameObject.Find("Box2").transform;
        arenaPosition = 0;
        moveDirection1 = 1;
        moveDirection2 = -1;
        forward = true;
        
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        MoveBox(1);
        MoveBox(2);
    }

    void MoveBox(int boxNum)
    {
        Transform box = null;
        int direction = 0;
        UnityEngine.Vector2 target = UnityEngine.Vector2.zero;
        UnityEngine.Vector2 start = UnityEngine.Vector2.zero;

        if(boxNum == 1)
        {
            box = Box1;
            direction = moveDirection1;
        } else if(boxNum == 2)
        {
            box = Box2;
            direction = moveDirection2;
        }

        start = new UnityEngine.Vector2(0, box.localPosition.y);
        target = new UnityEngine.Vector2(moveDistance*direction, box.localPosition.y);
        
        if(Mathf.Abs(box.localPosition.x) == Mathf.Abs(target.x) /*-.01*/)
        {
            forward = false;
        } else if(Mathf.Abs(box.localPosition.x) == Mathf.Abs(start.x))
        {
            forward = true;
        }

        if(forward)
        {
            box.localPosition = UnityEngine.Vector2.MoveTowards(box.localPosition, target, moveSpeed*Time.deltaTime);
        } else
        {
            box.localPosition = UnityEngine.Vector2.MoveTowards(box.localPosition, start, moveSpeed*Time.deltaTime);
        }
        
    }


    public override IEnumerator ShrinkClock()
    {
        return base.ShrinkClock();
        
    }


    public override void StartLevel()
    {
        base.StartLevel();
    

    }


    public override void EndLevel()
    {
        base.EndLevel();

    }

    public override void SetUIColors(int idx)
    {
        base.SetUIColors(idx);

    }


    //shows results + mapvote after round ends
    public override void ShowResults()
    {
        base.ShowResults();

    }

    public override void PlayAgain()
    {
        base.PlayAgain();

    }


    public override void OnConfirm(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnConfirm(playerID, ctx);

    }


    public override void OnBack(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnBack(playerID, ctx);

    }


     public override void ReadyPlayer(int idx)
    {
        base.ReadyPlayer(idx);

    }

    public override void UnReadyPlayer(int idx)
    {
        base.UnReadyPlayer(idx);

    }



    //moves player to spawnpoint
    public override void SpawnPlayer(int idx)
    {
        base.SpawnPlayer(idx);
    }

    public override int GetLevelType()
    {
        //battle arenas type = 10
        return 12;
    }
}
