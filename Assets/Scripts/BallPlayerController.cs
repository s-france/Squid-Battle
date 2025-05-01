using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Soccer Ball PlayerController
public class BallPlayerController : DummyPlayerController
{
    // Start is called before the first frame update
    void Start()
    {
        Init();
        
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

        //no im
        //no parent

        /*
        im = parent.im;
        if (im != null)
        {
            Debug.Log("got IM!!");
        }
        */

        gm = FindFirstObjectByType<GameManager>();
        pm = FindFirstObjectByType<PlayerManager>();

        //no input
        /*
        gp = pi.GetDevice<Gamepad>();
        if (gp != null)
        {
            Debug.Log("got GP!!");
            Debug.Log("device type: " + gp.GetType().ToString());
        }
        */

        alc = GameObject.Find("LevelController").GetComponent<ArenaLevelController>();


        //pi.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();

        //not needed...
        //transform.parent = gm.transform.GetChild(0);

        //CHECK THIS!!!
        //Ball ID = 99
        //use isDummy to differentiate
        idx = 99;
        //Debug.Log("player idx (from playercontroller): " + idx);

        //no colorId (for now)
        //colorID = parent.colorID;

        sr = this.gameObject.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Debug.Log("got SR!!");
        }

        //not needed...
        rc = GetComponentInChildren<ReticleController>();

        //ChangeColor(colorID);

        //no team
        /*
        //set team UI colors
        if(gm.gameMode == 1)
        {
            ParticleSystem.MainModule bubbles = transform.Find("BubbleTrail").GetComponent<ParticleSystem>().main;

            //player fx match team
            bubbles.startColor = pm.teamColors[pm.TeamList[pm.PlayerList[idx].team].color];
            rc.ChangeColor(parent.rc.colorID);
        }
        */


        //no hat
        /*
        //match parent hat
        if(parent.hatSR.enabled)
        {
            hatSR.color = parent.hatSR.color;
            hatSR.enabled = true;
        } else
        {
            hatSR.enabled = false;
        }
        */

        //no clones
        //Clones = new List<DummyPlayerController>();

        OverpowerPeerPrioTable = new Dictionary<PlayerController, float>();
        IntangiblePeerPrioTable = new Dictionary<PlayerController, float>();  

        //no items
        /*
        heldItems = new List<ItemBehavior>();
        */

        prevStates = new Queue<PlayerState>(rewindSize);
        prevPos = new List<Vector2>(2)
        {
            //preload prevPos
            transform.position,
            transform.position
        };

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
}
