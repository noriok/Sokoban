using UnityEngine;
using UnityEngine.UI;
// using System.Collections;
using System.Linq;

public enum SpriteType {
    Wall, Floor, Target, Box, PlayerN, PlayerS, PlayerW, PlayerE
}

public enum GameState {
    Play, // プレイ中
    Clear,  // クリアメッセージ中
}

public class MainSystem : MonoBehaviour {
    private GameObject _button;
    private Stage _stage;
    private GameState _gameState;
    private GameManager _gameManager;

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
        _gameManager = new GameManager();
        _stage = new Stage(_gameManager.NextStage(), this);

        GameObject.Find("TextClear").GetComponent<Text>().enabled = false;

        _button = GameObject.Find("Button");
        _button.GetComponent<Button>().onClick.AddListener(() => {
            GameObject.Find("TextClear").GetComponent<Text>().enabled = false;
            _button.SetActive(false);
            _stage.DestorySprites();
            _stage = new Stage(_gameManager.NextStage(), this);
            _gameState = GameState.Play;
        });
        _button.SetActive(false);

        _gameState = GameState.Play;
	}

	void Update () {
        if (_gameState != GameState.Play) return;

        bool move = false;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            _stage.MoveW();
            move = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            _stage.MoveE();
            move = true;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            _stage.MoveN();
            move = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            _stage.MoveS();
            move = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space)) {
            _stage.Undo();
        }

        if (move && _stage.IsClear()) { // ゲームクリア
            _gameState = GameState.Clear;

            var o = GameObject.Find("TextClear");
            o.GetComponent<Text>().enabled = true;
            _button.SetActive(true);
        }
	}

    void OnGUI() {
        if (GUILayout.Button("Show Clear Text")) {
            var o = GameObject.Find("TextClear");
            o.GetComponent<Text>().enabled = true;
        }
        else if (GUILayout.Button("Hide Clear Text")) {
            var o = GameObject.Find("TextClear");
            o.GetComponent<Text>().enabled = false;
        }
    }
}
