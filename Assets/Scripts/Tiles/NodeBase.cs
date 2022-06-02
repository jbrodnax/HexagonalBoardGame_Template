using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NodeBase
{
    public NodeBase Connection {get; private set;}
    public float G {get; private set;}
    public float H {get; private set;}
    public float F => G + H;

    public Tile tile {get; set;}
    public float r {get; set;}
    public float q {get; set;}
    public Vector2 Coords {get; set;}

    public NodeBase(Tile t, Vector2 coords){
        tile = t;
        this.Coords = coords;
        this.q = coords.x;
        this.r = coords.y;

        this.G = 0;
        this.H = 0;
    }
    public void SetConnection(NodeBase nodeBase) => Connection = nodeBase;

    public void SetG(float g) => G = g;

    public void SetH(float h) => H = h;

    public float GetDistance(NodeBase targetNode){
        var dcol = Math.Abs(q - targetNode.q);
        var drow = Math.Abs(r - targetNode.r);
        return (dcol + Math.Max(0, ((drow - dcol)/2)));
    }

    public void Reset() => G = H = 0;
}
