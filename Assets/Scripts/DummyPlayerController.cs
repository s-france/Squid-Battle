using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Dummy Player, - created from Double item
//operates like normal PlayerController but is missing PlayerInput,
//receives PlayerInput from original player
public class DummyPlayerController : PlayerController3
{
    [HideInInspector] public PlayerController parent; //player that created this dummy (set in doubleBehavior)

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Init()
    {
        //DontDestroyOnLoad(this);

        rb = this.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log("got RB!!");
        }

        im = parent.im;
        if (im != null)
        {
            Debug.Log("got IM!!");
        }

        gm = parent.gm;
        pm = parent.pm; 

        /*
        gp = pi.GetDevice<Gamepad>();
        if (gp != null)
        {
            Debug.Log("got GP!!");
            Debug.Log("device type: " + gp.GetType().ToString());
        }
        */

        alc = parent.alc;


        //pi.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();

        //not needed...
        //transform.parent = gm.transform.GetChild(0);

        //Dummy ID = parentID
        //use isDummy to differentiate
        idx = parent.idx;
        Debug.Log("player idx (from playercontroller): " + idx);

        colorID = parent.colorID;

        sr = this.gameObject.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Debug.Log("got SR!!");
        }
        rc = GetComponentInChildren<ReticleController>();

        ChangeColor(colorID);

        //set team UI colors
        if(gm.gameMode == 1)
        {
            ParticleSystem.MainModule bubbles = transform.Find("BubbleTrail").GetComponent<ParticleSystem>().main;

            //player fx match team
            bubbles.startColor = pm.teamColors[pm.TeamList[pm.PlayerList[idx].team].color];
            rc.ChangeColor(parent.rc.colorID);
        }


        //match parent hat
        if(parent.hatSR.enabled)
        {
            hatSR.color = parent.hatSR.color;
            hatSR.enabled = true;
        } else
        {
            hatSR.enabled = false;
        }

        Clones = new List<DummyPlayerController>();

        OverpowerPeerPrioTable = new Dictionary<PlayerController, float>();
        IntangiblePeerPrioTable = new Dictionary<PlayerController, float>();  

        heldItems = new List<ItemBehavior>();

        prevStates = new Queue<PlayerState>(rewindSize);
        prevPos = new List<Vector2>(3);
        lastPrevStatePos = Vector2.zero;

        

        selectedItemIdx = 0;
        isCoolingDown = false;
        i_move = Vector2.zero;
        true_i_move = Vector2.zero;
        rotation = Vector3.zero;

        isRewind = false;

        stamina = maxStamina;

        //set default stats
        defaultScale = transform.localScale;
        defaultChargeStrength = chargeStrength;

        defaultMaxChargeTime = maxChargeTime;
        defaultMaxMoveSpeed = maxMoveSpeed;
        defaultMaxMoveTime = maxMoveTime;
        defaultMaxMovePower = maxMovePower;
        defaultMaxHitstop = maxHitstop;

        passiveArmor = maxPassiveArmor;
        defaultMaxPassiveArmor = maxPassiveArmor;
        defaultMaxMoveArmor = maxMoveArmor;
        

        moveTime = 0;
        moveTimer = 0;
        hitStopTimer = 0;
        isMoving = false;
        isKnockback = false;
        isHitStop = false;
        ApplyMove(0, Vector2.zero, 0);
    }

    public override void KillPlayer()
    {
        isAlive = false;
        parent.Clones.Remove(this);
        base.KillPlayer();
    }

    public override void CheckIfAlive()
    {
        //true if any clones are alive
        if(!(isAlive || parent.isAlive || parent.Clones.Any(dummy => dummy.isAlive)))
        {
            Debug.Log("Player" + idx + " killed by Player" + killCredit);

            pm.KillPlayer(parent.idx, killCredit);
        }
    }
}
