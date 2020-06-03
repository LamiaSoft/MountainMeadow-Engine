using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Collision;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.GameMath;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Tools;
using static MountainMeadowEngine.Collision.Polygon;
using static MountainMeadowEngine.Collision.Rectangle3D;
using static MountainMeadowEngine.Components.MovableEntity;
using static MountainMeadowEngine.GameWorld;
//using static MountainMeadowEngine.Collision.Rectangle3D;

namespace MountainMeadowEngine.Components {

  public class CollisionArea : GameObjectComponent {
    string name;

    bool resolve = true;

    public Rectangle3D prevRectangle = new Rectangle3D();
    public Rectangle3D rectangle = new Rectangle3D();

    List<Tuple<int, Vector3>> _contactPoints1 = new List<Tuple<int, Vector3>>();
    List<Tuple<int, Vector3>> _contactPoints2 = new List<Tuple<int, Vector3>>();

    Texture2D debugCollisionBottom, debugCollisionFull;
    List<Vector2> slopeLine = new List<Vector2>();


    public CollisionArea(GameObject context) : base(context) {
      EventManager.AddEventListener<PositionEvent>(this);
    }

    public CollisionArea SetResolve(bool resolve) {
      this.resolve = resolve;
      return this;
    }

    public bool ShouldResolve() {
      return this.resolve;
    }

    public Texture2D GetDebugTextureBottom(GraphicsDevice graphicsDevice) {
      if (debugCollisionBottom == null) {
        int width = (int)rectangle.Length;
        int height = (int)rectangle.Width;// (getCollisionHeight() > collisionBox.Height) ? getCollisionHeight() : collisionBox.Height;

        debugCollisionBottom = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];

        switch (rectangle.type) {
          case RectangleTypes.REGULAR:
          case RectangleTypes.ZSLOPEX_DOWN:
          case RectangleTypes.ZSLOPEY_DOWN:
          case RectangleTypes.ZSLOPEX_UP:
          case RectangleTypes.ZSLOPEY_UP:
            for (int i = 0; i < data.Length; ++i) {
              if (i < width || i > (width * (height - 1)) - 1 ||
                  // (i > (width * (collisionBox.Height - 1)) - 1 && i < (width * collisionBox.Height)) || 
                  // (i > (width * (height - collisionBox.Height - 1)) - 1 && i < (width * (height - collisionBox.Height))) ||
                  i % width == 0 || (i - width + 1) % width == 0) {
                data[i] = Color.Red;
              } else {
                data[i] = Color.Transparent;
              }
            }
            break;
          case RectangleTypes.XYSLOPE_UP_LEFT:
            for (int i = 0; i < data.Length; ++i) {
              if (i < width ||
                  // (i > (width * (collisionBox.Height - 1)) - 1 && i < (width * collisionBox.Height)) || 
                  // (i > (width * (height - collisionBox.Height - 1)) - 1 && i < (width * (height - collisionBox.Height))) ||
                  i % width == 0) {
                data[i] = Color.Red;
              } else {
                data[i] = Color.Transparent;
              }
            }
            slopeLine = BresenhamLine.Line(0, height, width, 0);
            break;
          case RectangleTypes.XYSLOPE_UP_RIGHT:
            for (int i = 0; i < data.Length; ++i) {
              if (i > (width * (height - 1)) - 1 ||
                  // (i > (width * (collisionBox.Height - 1)) - 1 && i < (width * collisionBox.Height)) || 
                  // (i > (width * (height - collisionBox.Height - 1)) - 1 && i < (width * (height - collisionBox.Height))) ||
                  (i - width + 1) % width == 0) {
                data[i] = Color.Red;
              } else {
                data[i] = Color.Transparent;
              }
            }
            slopeLine = BresenhamLine.Line(0, height, width, 0);
            break;
          case RectangleTypes.XYSLOPE_DOWN_LEFT:
            for (int i = 0; i < data.Length; ++i) {
              if (i > (width * (height - 1)) - 1 ||
                  // (i > (width * (collisionBox.Height - 1)) - 1 && i < (width * collisionBox.Height)) || 
                  // (i > (width * (height - collisionBox.Height - 1)) - 1 && i < (width * (height - collisionBox.Height))) ||
                  i % width == 0) {
                data[i] = Color.Red;
              } else {
                data[i] = Color.Transparent;
              }
            }
            slopeLine = BresenhamLine.Line(0, 0, width, height);
            break;
          case RectangleTypes.XYSLOPE_DOWN_RIGHT:
            for (int i = 0; i < data.Length; ++i) {
              if (i < width ||
                  // (i > (width * (collisionBox.Height - 1)) - 1 && i < (width * collisionBox.Height)) || 
                  // (i > (width * (height - collisionBox.Height - 1)) - 1 && i < (width * (height - collisionBox.Height))) ||
                  (i - width + 1) % width == 0) {
                data[i] = Color.Red;
              } else {
                data[i] = Color.Transparent;
              }
            }
            slopeLine = BresenhamLine.Line(0, 0, width, height);
            break;
        }

        for (int sl = 0; sl < slopeLine.Count; sl++) {
          int _tmpPos = (((int)slopeLine[sl].Y - 1) * width) + (int)slopeLine[sl].X;
          if (_tmpPos >= 0 && _tmpPos < data.Length) {
            data[_tmpPos] = Color.Red;
          }
        }
        debugCollisionBottom.SetData(data);
      }

      return debugCollisionBottom;
    }

    public Texture2D GetDebugTextureFull(GraphicsDevice graphicsDevice) {
      if (debugCollisionFull == null) {
        int width = (int)rectangle.Length;
        int height = (int)rectangle.Width;// (getCollisionHeight() > collisionBox.Height) ? getCollisionHeight() : collisionBox.Height;

        int textureHeight = (int)rectangle.Height; //height + (int)(originZTo - originZFrom);

        debugCollisionFull = new Texture2D(graphicsDevice, width, textureHeight);
        Color[] data = new Color[width * textureHeight];

        for (int i = 0; i < data.Length; ++i) {
          if (i < width || i > (width * (textureHeight - 1)) - 1 ||
              (i > (width * (height - 1)) - 1 && i < (width * height)) ||
              (i > (width * (textureHeight - height - 1)) - 1 && i < (width * (textureHeight - height))) ||
              i % width == 0 || (i - width + 1) % width == 0) {
            data[i] = Color.Blue;
          } else {
            data[i] = Color.Transparent;
          }
        }
        debugCollisionFull.SetData(data);
      }

      return debugCollisionFull;
    }

    public CollisionArea SetOffset(float x, float y, float z) {
      prevRectangle.SetOffset(x, y, z);
      rectangle.SetOffset(x, y, z);
      return this;
    }

    public CollisionArea SetLength(float length) {
      prevRectangle.SetLength(length);
      rectangle.SetLength(length);
      return this;
    }

    public CollisionArea SetWidth(float width) {
      prevRectangle.SetWidth(width);
      rectangle.SetWidth(width);
      return this;
    }

    public CollisionArea SetHeight(float height) {
      prevRectangle.SetHeight(height);
      rectangle.SetHeight(height);
      return this;
    }

    public CollisionArea SetRectangleType(RectangleTypes type) {
      prevRectangle.SetRectangleType(type);
      rectangle.SetRectangleType(type);
      return this;
    }

    public CollisionArea SetName(string name) {
      this.name = name;
      return this;
    }

    public string GetName() {
      return this.name;
    }


    public Vector3 GetOffset() {
      return rectangle.Offset;
    }

    public float GetLength() {
      return rectangle.Length;
    }

    public float GetWidth() {
      return rectangle.Width;
    }

    public float GetHeight() {
      return rectangle.Height;
    }

    protected virtual void Update() {
      prevRectangle.SetPosition(context.GetPrevPosition().X, context.GetPrevPosition().Y, context.GetPrevPosition().Z);
      rectangle.SetPosition(context.GetPosition().X, context.GetPosition().Y, context.GetPosition().Z);
    }

    public bool CheckCollision(CollisionArea object2CollisionArea, out List<Tuple<int, Vector3>> contactPoints1, out List<Tuple<int, Vector3>> contactPoints2) {
      contactPoints1 = _contactPoints1;
      contactPoints2 = _contactPoints2;

      if (context.GetStatus() != GameObject.ObjectStatuses.ACTIVE)
        return false;
      
      if (Rectangle3D.Intersects(rectangle, object2CollisionArea.rectangle, out contactPoints1, out contactPoints2)) {
        return true;
      }
      return false;
    }

    public Vector3 SolveCollision(CollisionArea object2CollisionArea, List<Tuple<int, Vector3>> contactPoints1, List<Tuple<int, Vector3>> contactPoints2) {
      if (prevRectangle.Bottom < object2CollisionArea.rectangle.Top) {
        context.SetPosition(null, null, object2CollisionArea.rectangle.Top - 1);
        context.GetMovableEntity().Stop(false, false, true);
        return new Vector3();
      }

      if (prevRectangle.Top > object2CollisionArea.rectangle.Bottom) {
        context.SetPosition(null, null, object2CollisionArea.rectangle.Bottom + 1);
        context.GetMovableEntity().Stop(false, false, true);
        return new Vector3();
      }

      Collision c = new Collision(contactPoints1, contactPoints2, this, object2CollisionArea);
      return c.GetImpact();
    }

    private float? GetLeftCollision(List<Tuple<int, Vector3>> contactPoints) {
      foreach (Tuple<int, Vector3> contact in contactPoints) {
        switch (contact.Item1) {
          case 0:
          case 3:
          case 4:
          case 7:
            return contact.Item2.X;
            break;
        }
      }
      return null;

    }

    private float? GetRightCollision(List<Tuple<int, Vector3>> contactPoints) {
      foreach (Tuple<int, Vector3> contact in contactPoints) {
        switch (contact.Item1) {
          case 1:
          case 2:
          case 5:
          case 6:
            return contact.Item2.X;
            break;
        }
      }
      return null;

    }

    private float? GetBackCollision(List<Tuple<int, Vector3>> contactPoints) {
      foreach (Tuple<int, Vector3> contact in contactPoints) {
        switch (contact.Item1) {
          case 0:
          case 1:
          case 4:
          case 5:
            return contact.Item2.Y;
            break;
        }
      }
      return null;

    }

    private float? GetFrontCollision(List<Tuple<int, Vector3>> contactPoints) {
      foreach (Tuple<int, Vector3> contact in contactPoints) {
        switch (contact.Item1) {
          case 2:
          case 3:
          case 6:
          case 7:
            return contact.Item2.Y;
            break;
        }
      }
      return null;

    }

    private float? GetTopCollision(List<Tuple<int, Vector3>> contactPoints) {
      foreach (Tuple<int, Vector3> contact in contactPoints) {
        switch (contact.Item1) {
          case 0:
          case 1:
          case 2:
          case 3:
            return contact.Item2.Z;
            break;
        }
      }
      return null;
    }

    private float? GetBottomCollision(List<Tuple<int, Vector3>> contactPoints) {
      foreach (Tuple<int, Vector3> contact in contactPoints) {
        switch (contact.Item1) {
          case 4:
          case 5:
          case 6:
          case 7:
            return contact.Item2.Z;
            break;
        }
      }
      return null;


    }

    public override void Initialize() {
      
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      Update();
      return gameEvent;
    }
  }
}
