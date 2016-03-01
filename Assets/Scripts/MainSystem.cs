using UnityEngine;
// using System.Collections;
using System.Linq;

public enum SpriteType {
    Wall, Floor, Target, Box, PlayerN, PlayerS, PlayerW, PlayerE
}

public class MainSystem : MonoBehaviour {
    private Stage _stage;

    Sprite GetSprite(SpriteType spriteType) {
        string name = "";
        switch (spriteType) {
        case SpriteType.Wall:    name = "spWall"; break;
        case SpriteType.Floor:   name = "spFloor"; break;
        case SpriteType.Target:  name = "spTarget"; break;
        case SpriteType.Box:     name = "spBox"; break;
        case SpriteType.PlayerN: name = "spPlayerN"; break;
        case SpriteType.PlayerS: name = "spPlayerS"; break;
        case SpriteType.PlayerW: name = "spPlayerW"; break;
        case SpriteType.PlayerE: name = "spPlayerE"; break;
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>("sokoban");
        return sprites.Where(e => e.name == name).First();
    }

    public GameObject Bless(string name, SpriteType spriteType) {
        var obj = new GameObject(name);
        obj.AddComponent<SpriteRenderer>().sprite = GetSprite(spriteType);
        return obj;
    }

	void Start () {
        _stage = new Stage(this);
	}

	void Update () {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            _stage.MoveW();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            _stage.MoveE();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            _stage.MoveN();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            _stage.MoveS();
        }
	}
}
