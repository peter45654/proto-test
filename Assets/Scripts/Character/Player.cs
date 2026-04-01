using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{

    protected override void Update()
    {
        HandleInput();
        base.Update();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {

            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError("碰撞的物件沒有Enemy腳本，請確保敵人物件已正確附加Enemy腳本！");
                return;
            }
            CurrentHealth -= enemy.Damage; // 每次碰撞扣除敵人的傷害值
            Debug.Log($"玩家受到{enemy.Damage}傷害，當前血量：{CurrentHealth}");
        }
    }

    void HandleInput()
    {
        // 移動輸入：WASD
        InputVector = Vector2.zero;
        IsPressingDashingDownward = false; // 每幀重置向下衝刺狀態，只有在按下空格鍵時才會啟動

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) InputVector.y += 1;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) InputVector.y -= 1;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) InputVector.x -= 1;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) InputVector.x += 1;
            if (keyboard.spaceKey.isPressed) IsPressingDashingDownward = true; // 按下空格鍵啟動向下衝刺
        }

        if (InputVector != Vector2.zero)
        {
            InputVector.Normalize();
            FacingDirection = InputVector; // 更新面向方向
        }
    }
}