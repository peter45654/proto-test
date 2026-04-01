using UnityEngine;

public class Enemy : Character
{
    public float Damage = 5f; // 敵人的傷害值
    public float IgnoreReverseDirectionTime = 0.5f; // 初始化時忽略反轉的時間
    public float IgnoreReverseDirectionTimeCountdown = 0f; // 當前忽略反轉的剩餘時間
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        FacePlayer(); // 生成時面向玩家
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (IgnoreReverseDirectionTimeCountdown > 0)
        {
            IgnoreReverseDirectionTimeCountdown -= Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Border"))
        {
            FacePlayer(); // 生成時面向玩家    

            // if (IgnoreReverseDirectionTimeCountdown <= 0)
            // {
            //     Debug.Log("敵人與邊界發生碰撞，將反轉方向！");
            //     SetInputVector(-InputVector); // 反轉移動方向
            //     IgnoreReverseDirectionTimeCountdown = IgnoreReverseDirectionTime; // 重置忽略反轉的計時器
            // }
        }
    }


    public void SetInputVector(Vector2 newInputVector)
    {
        InputVector = newInputVector;
        if (InputVector != Vector2.zero)
        {
            FacingDirection = InputVector; // 更新面向方向
        }
    }

    void FacePlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("場景中找不到標籤為 'Player' 的物件，請確保玩家物件已正確設置標籤！");
        }

        var directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.Normalize();

        InputVector = directionToPlayer; // 直接面向玩家
        FacingDirection = directionToPlayer; // 更新面向方向
    }
}
