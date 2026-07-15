public class BruteMutant : EnemyBase
{
    protected override void Start()
    {
        maxHP = 150f;
        moveSpeed = 1.2f;
        damage = 25f;
        soulDropType = SoulType.Power;
        soulDropCount = 2;
        xpReward = 15f;

        base.Start();
    }
}
