using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHurtBox : MonoBehaviour
{
    PlayerController pc;
    [HideInInspector] public CircleCollider2D hb;
    List<Collider2D> Collisions;

    [SerializeField] float pushOutPower;


    // Start is called before the first frame update
    void Start()
    {
        pc = GetComponentInParent<PlayerController>();
        hb = GetComponent<CircleCollider2D>();
        Collisions = new List<Collider2D>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!Collisions.Contains(col))
        {
            Collisions.Add(col);
            pc.OnHurtboxTriggerEnter(col);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        Collisions.Remove(col);

    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            if(col.GetComponent<PlayerHurtBox>() != null)
            {
                if(col == col.GetComponent<PlayerHurtBox>().hb)
                {
                    Debug.Log("players TriggerStay!!");

                    Vector2 away = (col.transform.parent.position - transform.parent.position).normalized;

                    col.transform.parent.position = (Vector2)col.transform.parent.position + (pushOutPower * away);
                }
            }
            
        }

    }
}
