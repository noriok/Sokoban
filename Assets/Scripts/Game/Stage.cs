using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

class Box {
    public int Row { get; private set; }
    public int Col { get; private set; }
    public GameObject Sprite { get; private set; }
    public bool IsMoving { get; private set; }

    public Box(int row, int col, GameObject sprite) {
        IsMoving = false;
        Row = row;
        Col = col;
        Sprite = sprite;
    }

    public void UpdatePositionImmediately(int drow, int dcol) {
        Row += drow;
        Col += dcol;
        const float size = Stage.SpriteSize;
        Sprite.transform.localPosition = new Vector3(size * Col, -size * Row, 0);
    }

    public IEnumerator UpdatePosition(int drow, int dcol) {
        Assert.IsFalse(IsMoving);
        IsMoving = true;

        const float size = Stage.SpriteSize;
        const float duration = 0.2f;
        float elapsedTime = 0;
        var start = Sprite.transform.localPosition;
        float dx = size * dcol;
        float dy = -size * drow;
        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float x = Mathf.Lerp(start.x, start.x + dx, elapsedTime / duration);
            float y = Mathf.Lerp(start.y, start.y + dy, elapsedTime / duration);
            Sprite.transform.localPosition = new Vector3(x, y, 0);
            yield return null;
        }
        UpdatePositionImmediately(drow, dcol);
        IsMoving = false;
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

    private Text _stepCountText;
    private int _stepCount;

    private GameObject _root; // 全てのスプライトの親オブジェクト

    private readonly int _rows;
    private readonly int _cols;

    public const float SpriteSize = 0.8f;

    private MainSystem _sys;

    public void DestorySprites() {
        GameObject.Destroy(_root);
    }

    public Stage(List<string> stage, MainSystem sys) {
        _sys = sys;
        _root = new GameObject("SpriteRoot");
        _stage = stage;

        _stepCountText = GameObject.Find("StepCountText").GetComponent<Text>();
        _stepCountText.text = "Step: 0";

        _rows = _stage.Count;
        _cols = _stage[0].Length;
        for (int i = 0; i < _rows; i++) {
            _targetTable.Add(new bool[_cols]);
        }
        for (int i = 0; i < _stage.Count; i++) {
            for (int j = 0; j < _stage[i].Length; j++) {
                switch (_stage[i][j]) {
                case StageChar.Wall:
                    sys.MakeSprite(SpriteType.Wall, i, j).transform.SetParent(_root.transform);
                    break;

                case StageChar.Floor:
                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);
                    break;

                case StageChar.Target:
                    sys.MakeSprite(SpriteType.Target, i, j).transform.SetParent(_root.transform);
                    _targetTable[i][j] = true;
                    break;

                case StageChar.Box:
                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);

                    var box = sys.MakeSprite(SpriteType.Box, i, j);
                    box.transform.SetParent(_root.transform);
                    _boxes.Add(new Box(i, j, box));
                    break;

                case StageChar.TargetAndBox:
                    sys.MakeSprite(SpriteType.Target, i, j).transform.SetParent(_root.transform);
                    _targetTable[i][j] = true;

                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);

                    var box2 = sys.MakeSprite(SpriteType.Box, i, j);
                    box2.transform.SetParent(_root.transform);
                    _boxes.Add(new Box(i, j, box2));
                    break;

                case StageChar.Player:
                    sys.MakeSprite(SpriteType.Floor, i, j).transform.SetParent(_root.transform);
                    _player = new Player(i, j, sys, _root);
                    break;

                case StageChar.None:
                    break;

                default:
                    Assert.IsTrue(false);
                    break;
                }
            }
        }
        Assert.IsNotNull(_player);

        // ステージを画面中央に移動させる
        var pos = new Vector3(-Stage.SpriteSize * _cols / 2 + Stage.SpriteSize/2,
                              Stage.SpriteSize * _rows / 2 - Stage.SpriteSize/2,
                              0);
        _root.transform.position = pos;
    }

    public bool IsMoving() {
        if (_player.IsMoving) return true;
        foreach (var box in _boxes) {
            if (box.IsMoving) return true;
        }
        return false;
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
            return _stage[row][col] == StageChar.Wall;
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
        _sys.StartCoroutine(box.UpdatePosition(drow, dcol));
        return true;
    }

    private bool Move(int drow, int dcol) {
        int row = _player.Row + drow;
        int col = _player.Col + dcol;
        if (IsWall(row, col)) return false;

        int boxIndex = -1;
        if (ExistsBox(row, col)) {
            for (int i = 0; i < _boxes.Count; i++) {
                if (_boxes[i].Row == row && _boxes[i].Col == col) {
                    boxIndex = i;
                    break;
                }
            }

            if (!TryMoveBox(row, col, drow, dcol)) return false;
        }
        _sys.StartCoroutine(_player.UpdatePosition(drow, dcol));
        _undo.Push(new UndoData(drow, dcol, boxIndex));

        _stepCount++;
        _stepCountText.text = string.Format("Step: {0}", _stepCount);
        return true;
    }

    public bool MoveN() {
        return Move(-1, 0);
    }

    public bool MoveS() {
        return Move(1, 0);
    }

    public bool MoveE() {
        return Move(0, 1);
    }

    public bool MoveW() {
        return Move(0, -1);
    }

    public void Undo() {
        if (_undo.Count == 0) return;

        var undo = _undo.Pop();
        _player.UpdatePositionImmediately(undo.DeltaRow * -1, undo.DeltaCol * -1);
        _player.UpdateDirection(undo.DeltaRow, undo.DeltaCol);
        if (undo.BoxIndex != -1) {
            var box = _boxes[undo.BoxIndex];
            box.UpdatePositionImmediately(undo.DeltaRow * -1, undo.DeltaCol * -1);
        }

        _stepCount--;
        _stepCountText.text = string.Format("Step: {0}", _stepCount);
    }
}
