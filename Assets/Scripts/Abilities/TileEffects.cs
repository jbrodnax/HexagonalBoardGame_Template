using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEffects : MonoBehaviour
{
    private Color lighten;
    private Color darken;
    private float blendlight;
    private float blenddark;
    private TileEvent initialState;
    public delegate void MouseDelegate(Tile t1);
    public MouseDelegate OnMouseEnter;
    public MouseDelegate OnMouseExit;
    public MouseDelegate OnMouseDown;

    public TileEffects Init(
            Color h, 
            Color d, 
            float bl, 
            float bd,
            TileEvent state, 
            MouseDelegate onEnter = null,
            MouseDelegate onExit = null,
            MouseDelegate onDown = null
        ){
        lighten = h;
        darken = d;
        blendlight = bl;
        blenddark = bd;
        initialState = state;

        if (onEnter == null)
            OnMouseEnter = MouseEnterDelegateStub;
        else
            OnMouseEnter = onEnter;

        if (onExit == null)
            OnMouseExit = MouseExitDelegateStub;
        else
            OnMouseExit = onExit;

        if (onDown == null)
            OnMouseDown = MouseDownDelegateStub;
        else
            OnMouseDown = onDown;

        return this;
    }

    public void MouseEnterDelegateStub(Tile t){
        t.Highlight(TileEvent.Highlight);
    }

    public void MouseExitDelegateStub(Tile t){
        t.Highlight(TileEvent.UnHighlight);
    }

    public void MouseDownDelegateStub(Tile t){
        Debug.LogError("Ability does not implement an OnMouseDown Delegate!");
    }

    public Color InitPreview(Tile tile){
        var original = tile.OriginalColor;
        return TileEffectsHighlight(tile, initialState);
    }

    public Color TileEffectsHighlight(Tile tile, TileEvent tevent){
        var original = tile.OriginalColor;

        switch(tevent){
            case TileEvent.Highlight:
                return Color.Lerp(original, lighten, blendlight);
            case TileEvent.UnHighlight:
                return Color.Lerp(original, darken, blenddark);
            default:
                return original;
        }
    }

    public Color Unset(Tile tile){
        return tile.OriginalColor;
    }
}
