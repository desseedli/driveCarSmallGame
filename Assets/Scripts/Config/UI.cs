
//-----------------------------------------------
//              生成代码不要修改
//-----------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum UIType
{
    None = 0,
    ComFull = 1,
    ComPopup = 2,
    Tips = 3,
    SystemPopup = 4,
}

public enum UIMaskType
{
    None = 0,
    OnlyMask = 1,
    MaskClickClose = 2,
    TransparentMask = 3,
    TransparentClickMask = 4,
}

public class UICfg
{
    public readonly int ID;    //		主键
    public readonly string loadPath;    //		加载路径
    public readonly UIType uiType;    //		类型
    public readonly UIMaskType uiMaskType;    //		遮罩类型

    public UICfg(DynamicPacket packet)
    {
        ID = packet.PackReadInt32();
        loadPath = packet.PackReadString();
        uiType = (UIType)packet.PackReadInt32();
        uiMaskType = (UIMaskType)packet.PackReadInt32();
    }
}

public class UICfgMgr
{
    private static UICfgMgr mInstance;
    
    public static UICfgMgr Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new UICfgMgr();
            }
            
            return mInstance;
        }
    }

    private Dictionary<int, UICfg> mDict = new Dictionary<int, UICfg>();
    
    public Dictionary<int, UICfg> Dict
    {
        get {return mDict;}
    }

    public void Deserialize (DynamicPacket packet)
    {
        int num = (int)packet.PackReadInt32();
        for (int i = 0; i < num; i++)
        {
            UICfg item = new UICfg(packet);
            if (mDict.ContainsKey(item.ID))
            {
                mDict[item.ID] = item;
            }
            else
            {
                mDict.Add(item.ID, item);
            }
        }
    }
    
    public UICfg GetTemplateByID(int id)
    {
        if(mDict.ContainsKey(id))
        {
            return mDict[id];
        }
        
        return null;
    }
}
