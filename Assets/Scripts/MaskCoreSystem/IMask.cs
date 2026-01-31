using System.Collections.Generic;
public interface IMaskInfoProvider
{
    public MaskCore GetMaskInfo();
}

public interface IMaskInfoJudger
{
    public float JudgeMaskInfo(MaskCore judgeMask);
}

public interface IMaskInfoAgent:IMaskInfoJudger,IMaskInfoProvider
{

}
