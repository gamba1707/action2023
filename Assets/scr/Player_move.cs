using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player_move : MonoBehaviour
{
    CharacterController controller;//キャラクターコントローラー
    private GameObject Player_t;//Playerをすぐその方向に歩かせるため
    private Animator anim;//動きのアニメーション

    private Camera maincamera;//メインカメラ入れておく用

    private Vector3 moveDirection = Vector3.zero;//動きを入れておく用
    private Vector3 gravityDirection = Vector3.zero;//浮遊しているときにその状況を加算する用
    private Vector3 cameraForward = Vector3.zero;//本当は3D空間を歩き回ろうとしていたため用意した

    private float x, y;//キー入力値
    private float speed = 7F;//移動速度
    [SerializeField] private float speed_nomal = 7F;//nomal移動速度
    [SerializeField] private float speed_crouching = 4F;//しゃがみ時移動速度
    [SerializeField] private float jumpPower = 6F;//ジャンプ量
    [SerializeField] private bool falling;//落ちているか(落下判定用)
    [SerializeField] private bool ground;//接地しているか（接地ジャンプ用）
    [SerializeField] private bool crouching;//しゃがんでいるか

    //しゃがみ関連
    private float character_height;//初期身長
    private Vector3 character_centerpos;//初期中心
    private float crouching_height = 1.2f;//しゃがみ身長
    private Vector3 crouching_centerpos = new Vector3(0, 0.53f, 0.09f);//しゃがみ中心

    // Start is called before the first frame update
    void Start()
    {
        //コンポーネントを取得
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        //子オブジェクトを入れる（移動と向きの回転を分けるため）
        Player_t = transform.GetChild(0).gameObject;

        //メインカメラを入れておく
        maincamera = Camera.main;

        character_height = controller.height;
        character_centerpos = controller.center;
    }

    // Update is called once per frame
    void Update()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("jump_end"))
        {
            //入力値
            x = Input.GetAxis("Horizontal");    //左右矢印キーの値(-1.0~1.0)
            y = Input.GetAxis("Vertical");      //上下矢印キーの値(-1.0~1.0)
        }
        else
        {
            x = Input.GetAxis("Horizontal")*0.5f;    //左右矢印キーの値
            y = Input.GetAxis("Vertical")*0.5f;      //上下矢印キーの値
        }
        


        //着地時（たぶん）
        if (controller.isGrounded)
        {
            //落ちてない
            falling = false;
            //落ちていないので重力値は無にする
            gravityDirection = new Vector3(0, 0, 0);
            // カメラの方向から、X-Z平面の単位ベクトルを取得
            cameraForward = Vector3.Scale(maincamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            // 方向キーの入力値とカメラの向きから、移動方向を決定
            moveDirection = cameraForward * y * speed + maincamera.transform.right * x * speed;
            //ワールドに変換する
            moveDirection = transform.TransformDirection(moveDirection);
            //接地判定の精度を上げるため-0.5
            moveDirection.y = -0.5f;
        }
        else//空中にいる場合
        {
            //落ちていることにして処理能力に応じないようにFixedUpdateで処理する
            falling = true;
        }

        //動いているときは常に押されている方向を向いてほしい
        if (x != 0 || y != 0) Player_t.transform.localRotation = Quaternion.LookRotation(cameraForward * y + maincamera.transform.right * x);


        //アニメーション
        //移動
        anim.SetFloat("speed", Mathf.Abs(x) + Mathf.Abs(y));
        //ジャンプ
        if (Input.GetButtonDown("Jump") && ground&&!crouching)
        {
            Debug.Log("Jump");
            moveDirection.y = jumpPower;
            anim.SetTrigger("jump_start");
            ground = false;
        }
        //しゃがみ
        if (Input.GetButton("crouching"))
        {
            crouching = true;
            crouching_act();
        }
        else
        {
            crouching = false;
            crouching_act();
        }

        //最終的に動かす
        controller.Move(moveDirection * Time.deltaTime);
    }

    //しゃがみの処理
    void crouching_act()
    {
        //アニメーション
        anim.SetBool("Crouching", crouching);
        //キャラクターコントローラーの位置調整と移動速度
        if (crouching)
        {
            controller.height = crouching_height;
            controller.center = crouching_centerpos;
            speed = speed_crouching;
        }
        else
        {
            controller.height = character_height;
            controller.center = character_centerpos;
            speed = speed_nomal;
        }

    }

    //落下中の時のみ使用（処理能力に左右されないため）
    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, -transform.up * 0.28f, Color.red);
        if (Physics.Raycast(transform.position, -transform.up, 0.28f))
        {
            ground = true;
            //着地モーション
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("jump_air"))
            {
                anim.SetTrigger("jump_end");
                anim.ResetTrigger("air");
            }
            else anim.ResetTrigger("jump_end");
            Debug.Log("接地してます");
        }
        else
        {
            ground = false;
            //しゃがんでいないときに落下モーション
            if(!crouching)anim.SetTrigger("air");
            Debug.Log("接地してない");
        }

        //プレイ中で落ちている時
        if (falling)
        {
            gravityDirection = Vector3.zero;
            //右方向と奥方向を足し合わせたものを移動量とする
            Vector3 Direction = ((maincamera.transform.right * x * speed) + (maincamera.transform.forward * y * speed));
            //重力を加算していく
            gravityDirection.y -= 9.8f * 0.02f;
            //xとzには移動量、yには移動量と重力を与える
            moveDirection = new Vector3(Direction.x, gravityDirection.y + moveDirection.y, Direction.z);
            //ワールドに変換
            moveDirection = transform.TransformDirection(moveDirection);
        }
    }
}
