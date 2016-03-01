using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

struct Player {
    public int row;
    public int col;
    public GameObject sp;

    public GameObject spN, spS, spE, spW;
}

public class Stage {
    private List<string> _stage = new List<string>();
    // private List<GameObject[]> _spStage = new List<GameObject[]>(); // sprite
    private List<GameObject[]> _boxTable = new List<GameObject[]>();
    private Player _player;

    private int _rows;
    private int _cols;

    private const char CHAR_WALL = '#';
    private const char CHAR_FLOOR = '.';
    private const char CHAR_TARGET = 'x';
    private const char CHAR_BOX = 'o';
    private const char CHAR_PLAYER = 'p';

    private const float SPRITE_SIZE = 0.8f;

	void Init() {
        Debug.Log("Stage : start");
        _stage.Add("#######");
        _stage.Add("#.....#");
        _stage.Add("#.xo..#");
        _stage.Add("#.p...#");
        _stage.Add("#.....#");
        _stage.Add("#######");
	}

    private void SetupPlayer(MainSystem sys) {
        var spN = sys.Bless("Player", SpriteType.PlayerN);
        spN.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        var spS = sys.Bless("Player", SpriteType.PlayerS);
        spS.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        var spE = sys.Bless("Player", SpriteType.PlayerE);
        spE.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        var spW = sys.Bless("Player", SpriteType.PlayerW);
        spW.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        _player.spN = spN;
        _player.spS = spS;
        _player.spE = spE;
        _player.spW = spW;

        spN.SetActive(true);
        spS.SetActive(false);
        spE.SetActive(false);
        spW.SetActive(false);
        _player.sp = spN;
    }

    public Stage(MainSystem sys) {
        Init();
        SetupPlayer(sys);

        _rows = _stage.Count;
        _cols = _stage[0].Length;
        for (int i = 0; i < _rows; i++) {
            _boxTable.Add(new GameObject[_cols]);
        }
        const float size = SPRITE_SIZE;
        for (int i = 0; i < _stage.Count; i++) {
            for (int j = 0; j < _stage[i].Length; j++) {
                switch (_stage[i][j]) {
                case CHAR_WALL:
                    var wall = sys.Bless("Wall", SpriteType.Wall);
                    wall.transform.position = new Vector3(size * j, -size * i, 0);
                    wall.GetComponent<SpriteRenderer>().sortingLayerName = "Stage";
                    break;

                case CHAR_FLOOR:
                    var floor = sys.Bless("Floor", SpriteType.Floor);
                    floor.transform.position = new Vector3(size * j, -size * i, 0);
                    floor.GetComponent<SpriteRenderer>().sortingLayerName = "Stage";
                    break;

                case CHAR_TARGET:
                    var target = sys.Bless("Target", SpriteType.Target);
                    target.transform.position = new Vector3(size * j, -size * i, 0);
                    target.GetComponent<SpriteRenderer>().sortingLayerName = "Stage";
                    break;

                case CHAR_BOX:
                    var floor2 = sys.Bless("Floor", SpriteType.Floor);
                    floor2.transform.position = new Vector3(size * j, -size * i, 0);
                    floor2.GetComponent<SpriteRenderer>().sortingLayerName = "Stage";

                    var box = sys.Bless("Box", SpriteType.Box);
                    box.transform.position = new Vector3(size * j, -size * i, 0);
                    box.GetComponent<SpriteRenderer>().sortingLayerName = "Object";

                    _boxTable[i][j] = box;
                    break;

                case CHAR_PLAYER:
                    var floor3 = sys.Bless("Floor", SpriteType.Floor);
                    floor3.transform.position = new Vector3(size * j, -size * i, 0);
                    floor3.GetComponent<SpriteRenderer>().sortingLayerName = "Stage";

                    _player.row = i;
                    _player.col = j;
                    _player.sp.transform.position = new Vector3(size * j, -size * i, 0);
                    break;
                }
            }
        }
    }

    private bool isWall(int row, int col) {
        if (0 <= row && row < _rows && 0 <= col && col < _cols) {
            if (_stage[row][col] == CHAR_WALL) {
                return true;
            }
            return false;
        }
        return true;
    }

    private bool existsBox(int row, int col) {
        if (0 <= row && row < _rows && 0 <= col && col < _cols) {
            return _boxTable[row][col] != null;
        }
        return false;
    }

    // (row, col) にある箱を、(row+drow, col+dcol) に移動できたなら true
    private bool tryMoveBox(int row, int col, int drow, int dcol) {
        Assert.IsTrue(existsBox(row, col));
        Assert.IsTrue(Math.Abs(drow) + Math.Abs(dcol) == 1);

        int r = row + drow;
        int c = col + dcol;
        if (isWall(r, c) || existsBox(r, c)) return false;

        _boxTable[r][c] = _boxTable[row][col];
        _boxTable[row][col] = null;

        _boxTable[r][c].transform.position = new Vector3(SPRITE_SIZE * c, -SPRITE_SIZE * r, 0);
        return true;
    }

    private void UpdatePlayerPosition(int drow, int dcol) {
        _player.row += drow;
        _player.col += dcol;

        if (drow == -1) {
            _player.sp.SetActive(false);
            _player.sp = _player.spN;
        }
        else if (drow == 1) {
            _player.sp.SetActive(false);
            _player.sp = _player.spS;
            Debug.Log(_player.sp);
        }
        else if (dcol == -1) {
            _player.sp.SetActive(false);
            _player.sp = _player.spW;
        }
        else if (dcol == 1) {
            _player.sp.SetActive(false);
            _player.sp = _player.spE;
        }

        _player.sp.SetActive(true);
        _player.sp.transform.position = new Vector3(SPRITE_SIZE * _player.col, -SPRITE_SIZE * _player.row, 0);
    }

    public void MoveN() {
        int row = _player.row - 1;
        int col = _player.col;
        if (isWall(row, col)) return;

        if (existsBox(row, col)) {
            if (!tryMoveBox(row, col, -1, 0)) return;
        }
        UpdatePlayerPosition(-1, 0);
    }

    public void MoveS() {
        int row = _player.row + 1;
        int col = _player.col;
        if (isWall(row, col)) return;

        if (existsBox(row, col)) {
            if (!tryMoveBox(row, col, 1, 0)) return;
        }
        UpdatePlayerPosition(1, 0);
    }

    public void MoveE() {
        int row = _player.row;
        int col = _player.col + 1;
        if (isWall(row, col)) return;

        if (existsBox(row, col)) {
            if (!tryMoveBox(row, col, 0, 1)) return;
        }
        UpdatePlayerPosition(0, 1);
    }

    public void MoveW() {
        int row = _player.row;
        int col = _player.col - 1;
        if (isWall(row, col)) return;

        if (existsBox(row, col)) {
            if (!tryMoveBox(row, col, 0, -1)) return;
        }
        UpdatePlayerPosition(0, -1);
    }
}
