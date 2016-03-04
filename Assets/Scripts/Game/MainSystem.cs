using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum SpriteType {
    Wall, Floor, Target, Box, PlayerN, PlayerS, PlayerW, PlayerE
}

public enum GameState {
    Play,  // プレイ中
    Clear, // クリアメッセージ中
}

public class MainSystem : MonoBehaviour {
    private GameObject _button;
    private Text _buttonText;
    private Stage _stage;
    private GameState _gameState;
    private GameManager _gameManager;

    private Sprite GetSprite(SpriteType spriteType) {
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

    public GameObject MakeSprite(SpriteType spriteType, int row, int col) {
        string name = "";
        string layerName = "";
        switch (spriteType) {
        case SpriteType.Wall:
            name = "Wall";
            layerName = "Stage";
            break;
        case SpriteType.Floor:
            name = "Floor";
            layerName = "Stage";
            break;
        case SpriteType.Target:
            name = "Target";
            layerName = "Stage";
            break;
        case SpriteType.Box:
            name = "Box";
            layerName = "Object";
            break;
        default:
            Assert.IsTrue(false);
            break;
        }

        var obj = new GameObject(name);
        obj.AddComponent<SpriteRenderer>().sprite = GetSprite(spriteType);
        obj.GetComponent<SpriteRenderer>().sortingLayerName = layerName;
        const float size = Stage.SpriteSize;
        obj.transform.position = new Vector3(size * col, -size * row, 0);
        return obj;
    }

	void Start () {
        _gameManager = new GameManager();
        _stage = new Stage(_gameManager.NextStage(), this);
        _gameState = GameState.Play;

        GameObject.Find("TextClear").GetComponent<Text>().enabled = false;

        _buttonText = GameObject.Find("Button/Text").GetComponent<Text>();
        _button = GameObject.Find("Button");
        _button.GetComponent<Button>().onClick.AddListener(() => {
            if (_gameManager.IsFinalStage()) {
                SceneManager.LoadScene("Title");
            }
            else {
                _button.SetActive(false);
                GameObject.Find("TextClear").GetComponent<Text>().enabled = false;
                _stage.DestorySprites();
                _stage = new Stage(_gameManager.NextStage(), this);
                _gameState = GameState.Play;
            }
        });
        _button.SetActive(false);
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

            var text = GameObject.Find("TextClear");
            text.GetComponent<Text>().enabled = true;
            _button.SetActive(true);
            if (_gameManager.IsFinalStage()) {
                text.GetComponent<Text>().text = "All Clear!!";
                _buttonText.text = "Back to Title";
            }
            else {
                text.GetComponent<Text>().text = "Stage Clear!!";
                _buttonText.text = "Next Stage";
            }
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
