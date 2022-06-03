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

    public Cube GetCubeFromAxial(Vector2 v){
        return (new Cube(v.x, v.y, (-v.x - v.y)));
    }

    public Cube GetCubeFromThis(){
        return (GetCubeFromAxial(Coords));
    }

    public void Reset() => G = H = 0;
}

public class Cube
{
    public float q;
    public float r;
    public float s;

    public List<Vector3> Cube_Directions;

    public Cube(float _q, float _r, float _s){
        q = _q;
        r = _r;
        s = _s;

        Cube_Directions = new List<Vector3>();
        Cube_Directions.Add(new Vector3(1, 0, -1));
        Cube_Directions.Add(new Vector3(1, -1, 0));
        Cube_Directions.Add(new Vector3(0, -1, 1));
        Cube_Directions.Add(new Vector3(-1, 0, 1));
        Cube_Directions.Add(new Vector3(-1, 1, -0));
        Cube_Directions.Add(new Vector3(0, 1, -1));
    }

    public Vector2 GetAxial(){
        return (new Vector2(q,r));
    }

    public Cube Round(bool b){
        q = Mathf.Round(this.q);
        r = Mathf.Round(this.r);
        s = Mathf.Round(this.s);

        return this;
    }

    public Cube Round(){
        var _q = Mathf.Round(this.q);
        var _r = Mathf.Round(this.r);
        var _s = Mathf.Round(this.s);

        var q_diff = Math.Abs(_q - q);
        var r_diff = Math.Abs(_r - r);
        var s_diff = Math.Abs(_s - s);

        if (q_diff > r_diff && q_diff > s_diff)
            q = -r-s;
        else if (r_diff > s_diff)
            r = -q-s;
        else
            s = -q-r;
        
        return this;
    }

    public float GetDistanceTo(Cube b){
        return ((Math.Abs(this.q - b.q) + Math.Abs(this.r - b.r) + Math.Abs(this.s - b.s)) / 2);
    }

    public Cube Add(Cube a, Cube b = null){
        if (b == null)
            b = this;
        return new Cube(
            a.q + b.q,
            a.r + b.r,
            a.s + b.s
        );
    }
    public Cube Scale(Vector3 v, int factor){
        return new Cube(
            v.x * factor,
            v.y * factor,
            v.z * factor
        );
    }

    public Cube Neighbor(Cube c, Vector3 v){
        return new Cube(
            c.q + v.x,
            c.r + v.y,
            c.s + v.z
        );
    }
    public List<Cube> Ring(Cube center, int radius){
        var results = new List<Cube>();
        var nextCube = Add(center, Scale(Cube_Directions[4], radius));

        for (int i = 0; i < 6; i++){
            for (int j = 0; j < radius; j++){
                results.Add(nextCube);
                nextCube = Add(nextCube, Neighbor(nextCube, Cube_Directions[i]));
            }
        }

        return results;
    }
}
