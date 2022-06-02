using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainTile : Tile
{
   public override void Init(Vector2 coords, bool altColor)
   {
      base.Init(coords, altColor);

      _highlighted = Color.Lerp(_renderer.color, Color.white, _highBrightness);
      _original = _renderer.color;
   }
}
