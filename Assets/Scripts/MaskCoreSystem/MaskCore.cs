using System;
using System.Collections.Generic;
using UnityEngine;

static class CompareLogicCore
{
    public static float TypeFactor = 1 ;
    public static float PosFactor = 1 ;
    public static float RotFactor  = 1 ;
    public static float Compare(Element a_element,Element b_element)
    {
        //类型检测
        float typeCompareResult = 0;
        MaskCoreConfig.TryGetValue(a_element.type, b_element.type, out typeCompareResult);
        
        //位置检测
        float posCompareReslt = 0;
        float sub = Vector2.Distance(a_element.pos, b_element.pos);
        posCompareReslt = 1.5f - sub;

        //旋转角度检测
        float rotCompareResult = 1;
        if (a_element.considerRotFlag)
        {
            float minRotSub = 360;
            float mineRot = a_element.rot;
            float otherRot = b_element.rot;
            float temp = 0;
            foreach (float seg in a_element.rotList)
            {
                temp = Mathf.Abs(mineRot + seg - otherRot);
                minRotSub = Mathf.Min(minRotSub, temp);
            }
            rotCompareResult = 1 - minRotSub / 360;
        }

        return typeCompareResult*posCompareReslt*rotCompareResult;

    }
}

public class MaskCore
{
    List<MaskCoreUnit> maskUnits;

    MaskCoreUnit mainMaskUnit;

    public string LogInfo { get; private set; }

    public void Parse(string info)
    {
        Parse(info, false);
    }

    public void Parse(string info, bool force)
    {
        LogInfo = info;
        string[] stringUnits = info.Split('|');
        maskUnits = new List<MaskCoreUnit>();

        foreach (string stringUnit in stringUnits)
        {
            var unit = new MaskCoreUnit();
            unit.Parse(stringUnit);
            if (mainMaskUnit == null || force)
                mainMaskUnit = unit;
            maskUnits.Add(unit);
        }
    }


    //将自己的多个Mask单元与对方的主Mask进行比较
    public float Compare(MaskCore other)
    {
        return 1;
    }

    public int UnitCount => maskUnits?.Count ?? 0;
    public MaskCoreUnit GetUnit(int index) => maskUnits[index];
}

public class MaskCoreUnit
{
    List<Element> elements;

    string logInfo;

    public void Parse(string info)
    {
        logInfo = info;
        elements = new List<Element>();
        string[] elementStrings = info.Split(';');
        foreach(string str in elementStrings)
        {
            Element ele = new Element(str);
            elements.Add(ele);
        }

    }

    public int ElementCount => elements?.Count ?? 0;
    public Element GetElement(int index) => elements[index];
}


public class Element
{
    public int type;
    public Vector2 pos;
    public float rot = 0 ;

    public bool considerRotFlag = true;

    public List<float> rotList;

    public Element(string elementString)
    {
        string[] strs = elementString.Split('-');
        rotList = new List<float> { 0f, 360f };
        foreach (string propertyStr in strs)
        {
            string[] temps = propertyStr.Split('@');
            if (temps.Length < 2) continue;
            int propertyType = Convert.ToInt32(temps[0]);
            string valueStr = temps[1];
            switch (propertyType)
            {
                case 1:
                    // 类型内容，范围为正整数
                    if (int.TryParse(valueStr, out int t)) type = Mathf.Max(0, t);
                    break;
                case 2:
                    // 位置内容，用^分割x和y轴坐标，坐标范围0到1
                    string[] xy = valueStr.Split('^');
                    if (xy.Length >= 2 && float.TryParse(xy[0], out float x) && float.TryParse(xy[1], out float y))
                        pos = new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
                    break;
                case 3:
                    // 是否考虑旋转角，0或者1
                    considerRotFlag = valueStr == "1";
                    break;
                case 4:
                    // 旋转角内容（可选）0到360的浮点数
                    if (float.TryParse(valueStr, out float r)) rot = Mathf.Clamp(r, 0f, 360f);
                    break;
                case 5:
                    // 旋转角相似列表（可选）用^分割，范围0到360，不包括0和360
                    rotList = new List<float> { 0f, 360f };
                    foreach (string seg in valueStr.Split('^'))
                    {
                        if (float.TryParse(seg, out float v) && v > 0f && v < 360f)
                            rotList.Add(v);
                    }
                    break;
            }
        }
    }

    //检测本元素

    public float Compare(Element  ele)
    {
        return CompareLogicCore.Compare(this,ele);
    }
}




