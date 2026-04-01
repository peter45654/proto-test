using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [Header("血量")]
    public float MaxHealth = 60;
    public float CurrentHealth;
    public bool IsLoosingHP = false;
    public float HPLossPerSecond = 1f;


    [Header("移動設定")]
    public float Speed = 5f;
    public Vector2 InputVector;
    public Vector2 FacingDirection = Vector2.up; // 預設面向上方
    public Vector2 MovementVector;

    [Header("技能狀態")]
    public bool IsPressingDashingDownward = false;

    public float DashCostPerSecond = 1f; // 向下衝刺的HP消耗
    public float DashSpeed = 10f;

    private Color NormalColor = Color.white;
    public Color DashingColor = Color.red;

    [Header("角色組件")]
    public Rigidbody2D Rigidbody2D;
    public BoxCollider2D BoxCollider2D;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        CurrentHealth = MaxHealth; // 初始化當前血量
        if (Rigidbody2D == null) Rigidbody2D = GetComponent<Rigidbody2D>();
        if (BoxCollider2D == null) BoxCollider2D = GetComponent<BoxCollider2D>();
        if (Rigidbody2D == null || BoxCollider2D == null)
        {
            Debug.LogError("玩家缺少 Rigidbody2D 或 BoxCollider2D 組件，無法正常運作！");
            enabled = false;
        }
        if (GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogError("玩家缺少 SpriteRenderer 組件，無法正常運作！");
            enabled = false;
        }
        NormalColor = GetComponent<SpriteRenderer>().color; // 儲存初始顏色以便切換回來

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // 根據衝刺狀態切換顏色
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().color = IsPressingDashingDownward ? DashingColor : NormalColor;

        if (IsLoosingHP)
        {
            CurrentHealth -= HPLossPerSecond * Time.deltaTime;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                GameManager.Instance.CharacterDied(this); // 通知GameManager角色已死亡
            }
        }
        if (IsPressingDashingDownward)
        {
            CurrentHealth -= DashCostPerSecond * Time.deltaTime;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                GameManager.Instance.CharacterDied(this); // 通知GameManager角色已死亡
            }
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = currentPos;

        // 根據InputVector計算轉向
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(FacingDirection.y, FacingDirection.x) * Mathf.Rad2Deg - 90);

        if (IsPressingDashingDownward) MovementVector = InputVector * (Speed + DashSpeed) * Time.fixedDeltaTime;
        else MovementVector = InputVector * Speed * Time.fixedDeltaTime;

        // X 方向移動與碰撞檢測
        if (MovementVector.x != 0)
        {
            float xMovement = MovementVector.x;
            float actualXMovement = GetMaxMovementDistance(Vector2.right * Mathf.Sign(xMovement), Mathf.Abs(xMovement));
            nextPos.x += actualXMovement;
        }

        // Y 方向移動與碰撞檢測（包括重力）
        if (MovementVector.y != 0)
        {
            float yMovement = MovementVector.y;
            float actualYMovement = GetMaxMovementDistance(Vector2.up * Mathf.Sign(yMovement), Mathf.Abs(yMovement));
            nextPos.y += actualYMovement;
        }

        transform.position = nextPos;
    }

    float GetMaxMovementDistance(Vector2 direction, float desiredDistance)
    {
        Vector2 currentPos = transform.position;
        float topBorder = 12;
        float bottomBorder = -12;
        float leftBorder = -21;
        float rightBorder = 21;

        bool isHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);

        if (isHorizontal)
        {
            // 水平移動計算
            float targetX = currentPos.x + direction.x * desiredDistance;

            if (direction.x > 0) // 向右移動
            {
                if (targetX > rightBorder)
                    return Mathf.Max(0, rightBorder - currentPos.x);

                return desiredDistance; // 沒有碰撞，返回原始移動距離
            }
            else if (direction.x < 0) // 向左移動
            {
                if (targetX < leftBorder)
                    return Mathf.Max(0, currentPos.x - leftBorder);
                return -desiredDistance; // 沒有碰撞，返回原始移動距離
            }
        }
        else
        {
            // 垂直移動計算
            float targetY = currentPos.y + direction.y * desiredDistance;

            if (direction.y > 0) // 向上移動
            {
                if (targetY > topBorder)
                    return Mathf.Max(0, topBorder - currentPos.y);
                return desiredDistance; // 沒有碰撞，返回原始移動距離
            }
            else if (direction.y < 0) // 向下移動
            {
                if (targetY < bottomBorder)
                    return Mathf.Max(0, currentPos.y - bottomBorder);
                return -desiredDistance; // 沒有碰撞，返回原始移動距離
            }

        }
        return 0;
    }
}
