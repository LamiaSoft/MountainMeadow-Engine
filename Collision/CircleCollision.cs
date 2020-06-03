using System;
using Microsoft.Xna.Framework;

namespace MountainMeadowEngine.Collision {

  public class CircleCollision {
    public Vector3 sourceCoords;
    public int diameter;
    public int height;

    public CircleCollision(Vector3 sourceCoords, int diameter, int height) {
      this.sourceCoords = sourceCoords;
      this.diameter = diameter;
      this.height = height;
    }
  }
}
