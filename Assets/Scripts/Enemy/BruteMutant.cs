using UnityEngine;

// Quái Sức Mạnh: chậm, nhiều máu, gây sát thương lớn và rơi Power Soul.
public class BruteMutant : EnemyBase
{
    // Gọi logic chung của EnemyBase, sau đó thay bằng chỉ số riêng của Brute Mutant.
    protected override void Start()
    {
        base.Start();

        maxHP = 150f;
        currentHP = maxHP;
        moveSpeed = 1.2f;
        damage = 25f;
        soulDropType = SoulType.Power;
        soulDropCount = 2;
        xpReward = 15f;

        // Màu cam là hình ảnh tạm để phân biệt khi chưa có sprite chính thức.
        GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.27f, 0f);
    }
}
