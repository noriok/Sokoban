
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

class Player {
    private Dictionary<SpriteType, GameObject> _spriteMap = new Dictionary<SpriteType, GameObject>();

    public int Row { get; private set; }
    public int Col { get; private set; }
    public GameObject Sprite { get; private set; }

    public Player(int row, int col, MainSystem sys, GameObject root) {
        const float size = Stage.SpriteSize;
        var spriteTypes = new[] { SpriteType.PlayerN,
                                  SpriteType.PlayerS,
                                  SpriteType.PlayerE,
                                  SpriteType.PlayerW };
        foreach (var stype in spriteTypes) {
            var obj = sys.Bless("Player", stype);
            obj.GetComponent<SpriteRenderer>().sortingLayerName = "Player";
            obj.transform.SetParent(root.transform);
            obj.transform.position = new Vector3(size * col, -size * row, 0);
            obj.SetActive(false);

            _spriteMap[stype] = obj;
        }
        Row = row;
        Col = col;
        Sprite = _spriteMap[SpriteType.PlayerN];
        UpdateDirection(-1, 0);
    }

    public void UpdatePosition(int drow, int dcol) {
        UpdateDirection(drow, dcol);
        Row += drow;
        Col += dcol;

        const float size = Stage.SpriteSize;
        var pos = new Vector3(size * Col, -size * Row, 0);
        foreach (var sprite in _spriteMap.Values) {
            sprite.transform.localPosition = pos;
        }
    }

    public void UpdateDirection(int drow, int dcol) {
        Sprite.SetActive(false);

        if (drow == -1) {
            Sprite = _spriteMap[SpriteType.PlayerN];
        }
        else if (drow == 1) {
            Sprite = _spriteMap[SpriteType.PlayerS];
        }
        else if (dcol == -1) {
            Sprite = _spriteMap[SpriteType.PlayerW];
        }
        else if (dcol == 1) {
            Sprite = _spriteMap[SpriteType.PlayerE];
        }
        else {
            Assert.IsTrue(false);
        }
        Sprite.SetActive(true);
    }
}
