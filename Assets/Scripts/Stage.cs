using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

class Box {
    public int Row { get; set; }
    public int Col { get; set; }
    public GameObject GameObj { get; set; }

    public void UpdateSpritePosition() {
        const float size = Stage.SPRITE_SIZE;
        GameObj.transform.position = new Vector3(size * Col, -size * Row, 0);
    }
}

class UndoData {
    public int DeltaRow { get; private set; }
    public int DeltaCol { get; private set; }
    public int BoxIndex { get; private set; } // 移動させた箱のインデックス。箱を移動させていないなら -1

    public UndoData(int deltaRow, int deltaCol, int boxIndex) {
        DeltaRow = deltaRow;
        DeltaCol = deltaCol;
        BoxIndex = boxIndex;
    }
}

public class Stage {
    private List<Box> _boxes = new List<Box>();
    private List<bool[]> _targetTable = new List<bool[]>();
    private Player _player;
    private List<string> _stage;
    private Stack<UndoData> _undo = new Stack<UndoData>();

    private GameObject _root; // 全てのスプライトの親オブジェクト

    private readonly int _rows;
    private readonly int _cols;

    private const char CHAR_WALL = '#';
    private const char CHAR_FLOOR = '.';
    private const char CHAR_TARGET = 'x';
    private const char CHAR_BOX = 'o';
    private const char CHAR_PLAYER = 'p';

    public const float SPRITE_SIZE = 0.8f;

    public void DestorySprites() {
        GameObject.Destroy(_root);
    }

    public Stage(List<string> stage, MainSystem sys) {
        _root = new GameObject("SpriteRoot");
        _stage = stage;

        _rows = _stage.Count;
        _cols = _stage[0].Length;
        for (int i = 0; i < _rows; i++) {
            _targetTable.Add(new bool[_cols]);
        }
        for (int i = 0; i < _stage.Count; i++) {
            for (int j = 0; j < _stage[i].Length; j++) {
                switch (_stage[i][j]) {
                case CHAR_WALL:
                    sys.MakeSprite(SpriteType.Wall, i, j).transform.SetParent(_root.transform);
                    break;

                case CHAR_FLOOR:
                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);
                    break;

                case CHAR_TARGET:
                    sys.MakeSprite(SpriteType.Target, i, j).transform.SetParent(_root.transform);
                    _targetTable[i][j] = true;
                    break;

                case CHAR_BOX:
                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);

                    var box = sys.MakeSprite(SpriteType.Box, i, j);
                    box.transform.SetParent(_root.transform);
                    _boxes.Add(new Box { Row = i, Col = j, GameObj = box });
                    break;

                case CHAR_PLAYER:
                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);
                    _player = new Player(i, j, sys, _root);
                    break;
                }
            }
        }

        Assert.IsNotNull(_player);
    }

    public bool IsClear() {
        foreach (var box in _boxes) {
            if (!_targetTable[box.Row][box.Col]) {
                return false;
            }
        }
        return true;
    }

    private bool IsWall(int row, int col) {
        if (0 <= row && row < _rows && 0 <= col && col < _cols) {
            return _stage[row][col] == CHAR_WALL;
        }
        return true;
    }

    private bool ExistsBox(int row, int col) {
        return _boxes.Exists(e => e.Row == row && e.Col == col);
    }

    // (row, col) にある箱を、(row+drow, col+dcol) に移動できたなら true
    private bool TryMoveBox(int row, int col, int drow, int dcol) {
        Assert.IsTrue(ExistsBox(row, col));
        Assert.IsTrue(Math.Abs(drow) + Math.Abs(dcol) == 1);

        int r = row + drow;
        int c = col + dcol;
        if (IsWall(r, c) || ExistsBox(r, c)) return false;

        var box = _boxes.Find(e => e.Row == row && e.Col == col);
        box.Row = r;
        box.Col = c;
        box.UpdateSpritePosition();
        return true;
    }

    private void Move(int drow, int dcol) {
        int row = _player.Row + drow;
        int col = _player.Col + dcol;
        if (IsWall(row, col)) return;

        int boxIndex = -1;
        if (ExistsBox(row, col)) {
            for (int i = 0; i < _boxes.Count; i++) {
                if (_boxes[i].Row == row && _boxes[i].Col == col) {
                    boxIndex = i;
                    break;
                }
            }

            if (!TryMoveBox(row, col, drow, dcol)) return;
        }
        _player.UpdatePosition(drow, dcol);
        _undo.Push(new UndoData(drow, dcol, boxIndex));
    }

    public void MoveN() {
        Move(-1, 0);
    }

    public void MoveS() {
        Move(1, 0);
    }

    public void MoveE() {
        Move(0, 1);
    }

    public void MoveW() {
        Move(0, -1);
    }

    public void Undo() {
        if (_undo.Count == 0) return;

        var undo = _undo.Pop();
        _player.UpdatePosition(undo.DeltaRow * -1, undo.DeltaCol * -1);
        _player.UpdateDirection(undo.DeltaRow, undo.DeltaCol);
        if (undo.BoxIndex != -1) {
            var box = _boxes[undo.BoxIndex];
            box.Row += undo.DeltaRow * -1;
            box.Col += undo.DeltaCol * -1;
            box.UpdateSpritePosition();
        }
    }
}
