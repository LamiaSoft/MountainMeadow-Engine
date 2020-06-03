

// Author: Jason Morley (Source: http://www.morleydev.co.uk/blog/2010/11/18/generic-bresenhams-line-algorithm-in-visual-basic-net/)
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MountainMeadowEngine.GameMath {
  /// <summary>
  /// The Bresenham algorithm collection
  /// </summary>
  public static class BresenhamLine {
    private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

    /// <summary>
    /// The plot function delegate
    /// </summary>
    /// <param name="x">The x co-ord being plotted</param>
    /// <param name="y">The y co-ord being plotted</param>
    /// <returns>True to continue, false to stop the algorithm</returns>
    public delegate bool PlotFunction(int x, int y);

    /// <summary>
    /// Plot the line from (x0, y0) to (x1, y10
    /// </summary>
    /// <param name="x0">The start x</param>
    /// <param name="y0">The start y</param>
    /// <param name="x1">The end x</param>
    /// <param name="y1">The end y</param>
    /// <param name="plot">The plotting function (if this returns false, the algorithm stops early)</param>
    public static List<Vector2> Line(int x0, int y0, int x1, int y1) {
      bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
      if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
      if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
      int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;
      //bool result;

      int lastY = y;
      List<Vector2> coords = new List<Vector2>();

      for (int x = x0; x <= x1; ++x) {
        /*
        if (steep) {
          coords.Add(new Vector2(y, x));
          result = plot(y, x);
        } else {
          result = plot(x, y);
        }
        if (lastY != y) {
          result = plot(x, y-1);
          lastY = y;
        }
        if (!result) return; */

        if (steep) {
          coords.Add(new Vector2(y, x));
          if (lastY != x) {
            coords.Add(new Vector2(y, x-1));
            lastY = x;
          }
        } else {
          coords.Add(new Vector2(x, y));
          if (lastY != y) {
            coords.Add(new Vector2(x, y-1));
            lastY = y;
          }
        } 
        if (lastY != y) {
          //coords.Add(new Vector2(x, y - 1));
          //lastY = y;
        }
         
        //if (!(steep ? plot(y, x); plot(y+1,x) : plot(x, y))) return;
        err = err - dY;
        if (err < 0) { y += ystep; err += dX; }
      }

      return coords;
    }
  }
}

