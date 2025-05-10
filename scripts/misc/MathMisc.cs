using System;
using Godot;


public static class MathM
{
    public static int BoolToInt(bool value) => value? 1 : 0;
    public static void Iterate2DArray(int height, int width,Action<int,int> action){
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                action(x,y);
            }
        }
    }
    public static bool IsInRange(Vector2 vecA, Vector2 vecB, float radius){
        return vecA.DistanceTo(vecB) <= radius;
    }

}



/// <summary>
/// Vector2S is just a container of 2 sbyte (x and y), dont worry about the code, it is in compacted mode XD
/// </summary>
public partial class Vector2S : Resource{
    public sbyte x, y;
    public static Vector2S Zero = new Vector2S(0,0);
    public static Vector2S One = new Vector2S(1,1);
    public Vector2S(sbyte x=0, sbyte y=0){ this.x = x;this.y = y;}
    public Vector2S(Vector2 v){ x = (sbyte)v.X ; y = (sbyte)v.Y; }
    public static Vector2S operator +(Vector2S a, Vector2S b) => new Vector2S((sbyte)(a.x + b.x), (sbyte)Math.Abs(a.y + b.y));
    public static Vector2S operator -(Vector2S a, Vector2S b) => new Vector2S((sbyte)(a.x - b.x), (sbyte)Math.Abs(a.y - b.y));
    public static Vector2S operator *(Vector2S a, Vector2S b) => new Vector2S((sbyte)(a.x * b.x), (sbyte)(a.y * b.y));
    public static Vector2S operator /(Vector2S a, Vector2S b) => new Vector2S((sbyte)(a.x / b.x), (sbyte)(a.y / b.y));
    public static bool operator ==(Vector2S a, Vector2S b) => a is null ? b is null : a.x == b.x && a.y == b.y;
    public static bool operator !=(Vector2S a, Vector2S b) => !(a == b);
    public override int GetHashCode() => HashCode.Combine(x, y);
    public Vector2S Clone() => new Vector2S(x,y);
    public static Vector2S operator *(Vector2S a, sbyte b) => new Vector2S((sbyte)(a.x * b), (sbyte)(a.y * b));
    public override string ToString(){return "(x:" + x + " | y: " + y + ")";}
    public override bool Equals(object obj){
        if (obj is null) return false;
        return ReferenceEquals(this, obj) || GetType() != obj.GetType();
    }
    public static implicit operator Vector2(Vector2S v) => new Vector2(v.x, v.y);
    public static implicit operator Vector2S(Vector2 v) => new Vector2S((sbyte)v.X, (sbyte)v.Y);
    public void VecTranslate(Vector2 v){ x = (sbyte)v.X; y = (sbyte)v.Y; }

    public void SetAxis(string left,string right,string up,string down){
        Vector2 dir = Input.GetVector(left, right, up, down);
        x = (sbyte)dir.X;
        y = (sbyte)dir.Y;
    }
    public bool IsEmpty() => x == 0 && y == 0;
}