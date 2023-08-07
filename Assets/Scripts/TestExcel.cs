using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestExcel : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        string configpath = Application.dataPath + "/BinaryData.data";
        ConfigManager.LoadConfig(configpath);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
