using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager {
    private int _count = 1;

    public GameManager() {
    }

    public bool IsFinalStage() {
        return _count == 3;
    }

    public List<string> NextStage() {
        string filename = string.Format("Stage/stage{0}", _count++);
        return ReadStageData(filename);
	}

    private List<string> ReadStageData(string filename) {
        Debug.Log("load stage: " + filename);
        TextAsset t = Resources.Load<TextAsset>(filename);
        string[] ss = t.text.Split(new[] { '\n' });

        int len = ss.Select(e => e.Length).Max();
        var ret = new List<string>();
        foreach (string s in ss) {
            var cs = new char[len];
            for (int i = 0; i < len; i++) {
                cs[i] = ' ';
                if (i < s.Length) {
                    cs[i] = s[i];
                }
            }
            ret.Add(string.Join("", cs.Select(e => e.ToString()).ToArray()));
        }
        return ret;
    }
}
