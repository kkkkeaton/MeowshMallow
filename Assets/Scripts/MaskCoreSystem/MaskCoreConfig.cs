using System.Collections.Generic;

/// <summary>用“两个整数的无序集合”作为键索引到浮点数；(a,b) 与 (b,a) 视为同一键。</summary>
public static class MaskCoreConfig
{
    /// <summary>无序整数对 → 浮点值；键为 (较小值, 较大值)，保证 (1,2) 与 (2,1) 同键。</summary>
    public static Dictionary<(int, int), float> PairToFloat { get; } = new Dictionary<(int, int), float>
    {
        [(1, 2)] = 0.1f,
        [(1, 3)] = 1f
    };













    

    /// <summary>把两个整数规范为无序对键（小者在前）。</summary>
    public static (int, int) Normalize(int a, int b)
    {
        return a <= b ? (a, b) : (b, a);
    }

    /// <summary>用两个整数（乱序）查找浮点数；未找到返回 false。</summary>
    public static bool TryGetValue(int a, int b, out float value)
    {
        var key = Normalize(a, b);
        if (a==b)
        {
            value = 1f;
            return true;
        }
        return PairToFloat.TryGetValue(key, out value);
    }

    /// <summary>用两个整数（乱序）查找浮点数；未找到返回 null。</summary>
    public static float? GetValue(int a, int b)
    {
        var key = Normalize(a, b);
        return PairToFloat.TryGetValue(key, out var v) ? v : (float?)null;
    }

    /// <summary>为两个整数（乱序）设置对应的浮点值。</summary>
    public static void SetValue(int a, int b, float value)
    {
        var key = Normalize(a, b);
        PairToFloat[key] = value;
    }
}
