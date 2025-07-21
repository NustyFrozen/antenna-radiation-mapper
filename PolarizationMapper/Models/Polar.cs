using System;
using System.Numerics;

namespace PolarizationMapper.Models;

public struct polar
{
    public double magnitude;
    public double angle;
    private double highResolution;
    public polar(Vector2 cartisian)
    {
        magnitude = Math.Sqrt(Math.Pow(cartisian.X, 2) + Math.Pow(cartisian.Y, 2));
        angle = Math.Atan2(cartisian.Y, cartisian.X);
        angle = (angle * 180.0 / Math.PI);
        angle += 180;
        highResolution = angle;
        angle = Math.Round(angle);
    }
    public double getHighResolutionAngle() { return highResolution; }
            
           
    public Vector2 toCartisian()
    {
        var tempAngle = (angle / 180.0 * Math.PI);
        float x, y;
        x = (float)(magnitude * Math.Cos(tempAngle));
        y = (float)(magnitude * Math.Sin(tempAngle));
        return new Vector2(x,y);
    }
    public override string ToString()
    {
        return $"{angle}:{magnitude}";
    }
    public int average;
}