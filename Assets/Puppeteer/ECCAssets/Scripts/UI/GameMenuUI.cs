using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : UIBase
{
    public Button btnGo;
    public Image imgTest;
    
    public override void PreAddAtlasAsset()
    {
        Debug.Log("GameMenuUI PreAddAtlasAsset");
        AddAtlasAsset("atlas/common.spriteatlas");
    }

    protected override void Show()
    {
        Debug.Log("GameMenuUI show");
        btnGo.onClick.AddListener(StartGame);
        SetImageSprite(imgTest, "Star");
    }

    public void StartGame()
    {
        Debug.Log("StartGame");
    }
}
