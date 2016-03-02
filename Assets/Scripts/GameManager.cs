using UnityEngine;
using System.Collections.Generic;

public class GameManager {

    public GameManager() {

    }

    public List<string> NextStage() {
        var stage = new List<string>();
        stage.Add("#######");
        stage.Add("#.....#");
        stage.Add("#.xo..#");
        stage.Add("#.p...#");
        stage.Add("#.....#");
        stage.Add("#######");
        return stage;
	}
}
