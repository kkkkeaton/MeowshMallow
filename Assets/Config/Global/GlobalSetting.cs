using UnityEngine;

public static class GlobalSetting
{
    /// <summary>玩家附近判定与归一化范围（世界单位，X/Y 共用）。</summary>
    public static float PLAYER_NEAR_WORLD_RADIUS = 2f;

    /// <summary>拾取判定距离（世界单位），范围内可拾取 PickableItem。</summary>
    public static float PICKUP_RANGE = 2f;

    /// <summary>攻击/暗杀判定距离（世界单位），范围内可对敌人发动攻击。</summary>
    public static float ATTACK_RANGE = 1.5f;
}
