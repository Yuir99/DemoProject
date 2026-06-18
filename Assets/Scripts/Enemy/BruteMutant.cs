using UnityEngine;

public class BruteMutant : EnemyBase
{
    protected override void Start()
    {
        base.Start();

        maxHP = 150f;
        currentHP = maxHP;
        moveSpeed = 1.2f;    // Chậm nhất
        damage = 25f;        // Đau nhất
        soulDropType = SoulType.Power;
        soulDropCount = 2;   // Rớt 2 hồn Sức Mạnh
        xpReward = 15f;

        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.27f, 0f);
    }
}