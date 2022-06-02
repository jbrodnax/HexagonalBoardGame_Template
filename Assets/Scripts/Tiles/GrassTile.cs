using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTile : Tile
{
    [SerializeField] private Color _baseColor, _offsetColor;

    public override void Init(Vector2 coords, bool altColor)
    {
        base.Init(coords, altColor);

        _renderer.color = altColor ? _offsetColor : _baseColor;
        _highlighted = Color.Lerp(_renderer.color, Color.white, _highBrightness);
        _original = _renderer.color;
    }

}
