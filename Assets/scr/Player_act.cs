using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_act : MonoBehaviour
{
    private Animator anim;//動きのアニメーション
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("left"))
        {
            anim.SetTrigger("act_sword");
        }
    }
}
