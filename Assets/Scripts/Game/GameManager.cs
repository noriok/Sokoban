using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager {

    public GameManager() {
    }

    public List<string> NextStage() {
        string filename = "Stage/stage1";
        return ReadStageData(filename);
	}

    private List<string> ReadStageData(string filename) {
        TextAsset t = Resources.Load<TextAsset>(filename);
        string[] ss = t.text.Split(new[] { '\n' });
        return ss.ToList();
    }
}
