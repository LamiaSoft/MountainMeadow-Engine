using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Objects;
using static MountainMeadowEngine.Collision.Rectangle3D;
using static MountainMeadowEngine.Components.GameObject;

namespace MountainMeadowEngine
{

    public class ObjectManager : IComparer<Tuple<int, int, Type>> {
    Type sceneContextType;

    Dictionary<int, GameObject> allGameObjects = new Dictionary<int, GameObject>();
    Dictionary<int, Vector3> objectPositions = new Dictionary<int, Vector3>();
    Dictionary<int, Vector2> objectDrawPositions = new Dictionary<int, Vector2>();
    Dictionary<int, List<Tuple<int, int, int>>> objectPhysicsBuckets = new Dictionary<int, List<Tuple<int, int, int>>>();
    Dictionary<int, List<Tuple<int, int>>> objectRenderBuckets = new Dictionary<int, List<Tuple<int, int>>>();
    List<int> objectFixedPosition = new List<int>();
    Dictionary<Type, Tuple<Vector3, Vector3>> typePhysicsOffset = new Dictionary<Type, Tuple<Vector3, Vector3>>();
    Dictionary<Type, Tuple<Vector2, Vector2>> typeRenderOffset = new Dictionary<Type, Tuple<Vector2, Vector2>>();
    Dictionary<Type, List<Tuple<Vector2, Vector2>>> typeUnderlyingSlopeOffset = new Dictionary<Type, List<Tuple<Vector2, Vector2>>>();
    public Dictionary<Tuple<int, int, int>, Dictionary<int, bool>> spatialPhysicsBuckets = new Dictionary<Tuple<int, int, int>, Dictionary<int, bool>>();
    Dictionary<Tuple<int, int>, Dictionary<int, bool>> spatialRenderBuckets = new Dictionary<Tuple<int, int>, Dictionary<int, bool>>();

    GameObject _currentObject, _currentObject2;
    GameObjectComponent _currentComponent, _currentComponent2;
    Vector3 _maxSides1, _minSides1, _maxSides2, _minSides2;


    Vector3 spatialBucketPixelSize;
    //public Dictionary<Tuple<int, int, int>, List<int>> spatialPhysicsBuckets = new Dictionary<Tuple<int, int, int>, List<int>>();
    //Dictionary<Tuple<int, int>, List<int>> spatialRenderBuckets = new Dictionary<Tuple<int, int>, List<int>>();
    public Dictionary<int, Dictionary<int, bool>> spatialRenderBucketList = new Dictionary<int, Dictionary<int, bool>>();
    Dictionary<Tuple<int, int>, int> viewportBuckets = new Dictionary<Tuple<int, int>, int>();

    List<CollisionArea> _collisionAreas = new List<CollisionArea>();
    List<Sprite> _sprites = new List<Sprite>();
    List<AnimatedSprite> _animatedSprites = new List<AnimatedSprite>();

    List<Vector2> _collisionPoints = new List<Vector2>();

    public ObjectManager(Vector3 spatialBucketPixelSize, Type sceneContextType) {
      this.spatialBucketPixelSize = spatialBucketPixelSize;
      this.sceneContextType = sceneContextType;
    }

   
    public Dictionary<int, GameObject> GetAllGameObjects() {
      return allGameObjects;
    }

    void SetViewportBuckets(float viewportX, float viewportY, float viewportWidth, float viewportHeight) {
      viewportBuckets.Clear();

      viewportWidth *= ViewportManager.GAME_VIEWPORT_SCALE;
      viewportHeight *= ViewportManager.GAME_VIEWPORT_SCALE;

      float minX = viewportX;  
      float maxX = minX + viewportWidth;
      float minY = viewportY; 
      float maxY = minY + viewportHeight; 
           
      for (float my = minY; my <= maxY + spatialBucketPixelSize.Y + 1; my += spatialBucketPixelSize.Y) {
        for (float m = minX; m <= maxX + spatialBucketPixelSize.X + 1; m += spatialBucketPixelSize.X) {
          
          float x1 = m;// (m > maxX) ? maxX : m;
          float y1 = my;//(my > maxY) ? maxY : my;

          int xx1 = (int)Math.Ceiling(x1 / spatialBucketPixelSize.X);
          int yy1 = (int)Math.Ceiling(y1 / spatialBucketPixelSize.Y);

          Tuple<int, int> t = Tuple.Create(xx1, yy1);
          if (!viewportBuckets.ContainsKey(t)) {
            viewportBuckets.Add(t, 1);
          }
        }
      }
      //Tools.Debug.Json(ToString() + " VIEWPORT: ", viewportBuckets, GameWorld.DebugModes.WARNING);
    }

    public void DrawUpdate(GameTime gameTime, float viewportX, float viewportY, float viewportWidth, float viewportHeight) {
      SetViewportBuckets(viewportX, viewportY, viewportWidth, viewportHeight);

      spatialRenderBucketList.Clear();
      spatialRenderBucketList.Add(0, new Dictionary<int, bool>());



      foreach (Tuple<int, int> bucket in viewportBuckets.Keys) {
        if (spatialRenderBuckets.ContainsKey(bucket) && spatialRenderBuckets[bucket].Count > 0) {
          List<int> objectIds = new List<int>(spatialRenderBuckets[bucket].Keys);
          for (int o = 0; o < objectIds.Count; o++) {
            if (!spatialRenderBucketList[0].ContainsKey(objectIds[o]))
              spatialRenderBucketList[0].Add(objectIds[o], true);
            continue;
          }
        }
      }
    }

    public void UpdateObjectRendering(int objectId) {
      if (allGameObjects[objectId].GetComponents<FixedPositionObject>().Count > 0)
        return;

      if (!objectDrawPositions.ContainsKey(objectId)) {
        objectDrawPositions.Add(objectId, allGameObjects[objectId].GetDrawPosition());
        objectRenderBuckets.Add(objectId, new List<Tuple<int, int>>());
      } else {
        if (allGameObjects[objectId].GetDrawPosition() == objectDrawPositions[objectId]) {
          return;
        }
        objectDrawPositions[objectId] = allGameObjects[objectId].GetDrawPosition();
        for (int i = 0; i < objectRenderBuckets[objectId].Count; i++) {
          spatialRenderBuckets[objectRenderBuckets[objectId][i]].Remove(objectId);
        }
        objectRenderBuckets[objectId].Clear();
      }

      Vector2 min = allGameObjects[objectId].GetDrawPosition() + typeRenderOffset[allGameObjects[objectId].GetType()].Item1;
      Vector2 max = allGameObjects[objectId].GetDrawPosition() + typeRenderOffset[allGameObjects[objectId].GetType()].Item2;

      for (float my = (float)Math.Floor(min.Y); my <= (float)Math.Ceiling(max.Y) + spatialBucketPixelSize.Y; my += spatialBucketPixelSize.Y) {
        for (float m = (float)Math.Floor(min.X); m <= (float)Math.Ceiling(max.X) + spatialBucketPixelSize.X; m += spatialBucketPixelSize.X) {
          float x1 = (m > (float)Math.Ceiling(max.X)) ? (float)Math.Ceiling(max.X) : m;
          float y1 = (my > (float)Math.Ceiling(max.Y)) ? (float)Math.Ceiling(max.Y) : my;

          int xx1 = (int)Math.Ceiling(x1 / spatialBucketPixelSize.X);
          int yy1 = (int)Math.Ceiling(y1 / spatialBucketPixelSize.Y);

          Tuple<int, int> t = Tuple.Create(xx1, yy1);
          //Tools.Debug.Json(ToString() + " DRWTUPLE " + objectId + ": ", t, GameWorld.DebugModes.WARNING);

          if (!spatialRenderBuckets.ContainsKey(t)) {
            spatialRenderBuckets.Add(t, new Dictionary<int, bool>());
          }
          if (!spatialRenderBuckets[t].ContainsKey(objectId)) {
            spatialRenderBuckets[t].Add(objectId, true);
            objectRenderBuckets[objectId].Add(t);
          }
        }
      }

    }

    public void UpdateObject(int objectId) {
      if (allGameObjects[objectId].GetComponents<FixedPositionObject>().Count > 0)
        return;

      if (!objectPositions.ContainsKey(objectId)) {
        objectPositions.Add(objectId, allGameObjects[objectId].GetPosition());
        objectPhysicsBuckets.Add(objectId, new List<Tuple<int, int, int>>());
      } else {
        if (allGameObjects[objectId].GetPosition() == objectPositions[objectId]) {
          return;
        }
        objectPositions[objectId] = allGameObjects[objectId].GetPosition();
        for (int i = 0; i < objectPhysicsBuckets[objectId].Count; i++) {
          spatialPhysicsBuckets[objectPhysicsBuckets[objectId][i]].Remove(objectId);
        }
        objectPhysicsBuckets[objectId].Clear();
      }

      Vector3 min = allGameObjects[objectId].GetPosition() + typePhysicsOffset[allGameObjects[objectId].GetType()].Item1;
      Vector3 max = allGameObjects[objectId].GetPosition() + typePhysicsOffset[allGameObjects[objectId].GetType()].Item2;

      for (float my = min.Y; my <= max.Y + spatialBucketPixelSize.Y; my += spatialBucketPixelSize.Y) {
        for (float m = min.X; m <= max.X + spatialBucketPixelSize.X; m += spatialBucketPixelSize.X) {
          for (float mz = max.Z; mz <= min.Z + spatialBucketPixelSize.Z; mz += spatialBucketPixelSize.Z) {
      
            float x1 = (m > max.X) ? max.X : m;
            float y1 = (my > max.Y) ? max.Y : my;
            float z1 = (mz > min.Z) ? min.Z : mz;

            int xx1 = (int)Math.Ceiling(x1 / spatialBucketPixelSize.X);
            int yy1 = (int)Math.Ceiling(y1 / spatialBucketPixelSize.Y);
            int zz1 = (int)Math.Ceiling(z1 / spatialBucketPixelSize.Z);

            Tuple<int, int, int> t = Tuple.Create(xx1, yy1, zz1);

            if (!spatialPhysicsBuckets.ContainsKey(t)) {
              spatialPhysicsBuckets.Add(t, new Dictionary<int, bool>());
            }
            if (!spatialPhysicsBuckets[t].ContainsKey(objectId)) {
              spatialPhysicsBuckets[t].Add(objectId, true);
              objectPhysicsBuckets[objectId].Add(t);
            }
          }
        }
      }
    }

    public bool RenderInFrontSlope(int gameObjectId, int gameObjectId2) {
      if (typeUnderlyingSlopeOffset.ContainsKey(allGameObjects[gameObjectId].GetType())) {
        Vector3 minSides = GetObjectMinSides(gameObjectId2);
        Vector3 maxSides = GetObjectMaxSides(gameObjectId2);
        Vector3 slopePosMin, slopePosMax;

        foreach (Tuple<Vector2, Vector2> slope in typeUnderlyingSlopeOffset[allGameObjects[gameObjectId].GetType()]) {
          slopePosMin.X = allGameObjects[gameObjectId].GetPosition().X + slope.Item1.X;
          slopePosMin.Y = allGameObjects[gameObjectId].GetPosition().Y + slope.Item1.Y;
          slopePosMax.X = allGameObjects[gameObjectId].GetPosition().X + slope.Item2.X;
          slopePosMax.Y = allGameObjects[gameObjectId].GetPosition().Y + slope.Item2.Y;

          if (minSides.X <= slopePosMax.X && maxSides.X >= slopePosMin.X)  {
            if (minSides.Y <= slopePosMax.Y && maxSides.Y >= slopePosMin.Y) {
              return true;
            }
          }
        }
      }
      return false;
    }

    public Vector3 GetObjectMaxSides(int gameObjectId) {
      return allGameObjects[gameObjectId].GetPosition() + typePhysicsOffset[allGameObjects[gameObjectId].GetType()].Item2;
    }

    public Vector3 GetObjectMinSides(int gameObjectId) {
      return allGameObjects[gameObjectId].GetPosition() + typePhysicsOffset[allGameObjects[gameObjectId].GetType()].Item1;
    }


    Tuple<Vector2, Vector2> GetRenderOffset(GameObject gameObject) {
      Vector2 minRenderOffset = new Vector2(0, 0);
      Vector2 maxRenderOffset = new Vector2(0, 0);

      if (gameObject.GetComponents<FixedPositionObject>().Count == 0) {
        float minX = 0, minY = 0, maxX = 0, maxY = 0, x, y;

        _sprites = gameObject.GetComponents<Sprite>();
        _animatedSprites = gameObject.GetComponents<AnimatedSprite>();

        if (_sprites.Count > 0 || _animatedSprites.Count > 0) {
          for (int c = 0; c < _sprites.Count; c++) {
            x = gameObject.GetDrawPosition().X + _sprites[c].GetOffset().X;
            y = gameObject.GetDrawPosition().Y + _sprites[c].GetOffset().Y;

            if (gameObject.GetId() == 1) {
              //Tools.Debug.Json(ToString() + " X-Y " + gameObject.GetId() + ": ", x + ", " + y);
             // Tools.Debug.Json(ToString() + " offset " + gameObject.GetId() + ": ", _sprites[c].GetOffset());
              //Tools.Debug.Json(ToString() + " source rect " + gameObject.GetId() + ": ", ((Rectangle)_sprites[c].GetSourceRectangle()));
            }

            if (c == 0) {
              minX = x;
              minY = y;
            } else {
              minX = (x < minX) ? x : minX;
              minY = (y < minY) ? y : minY;
            }
            if (!_sprites[c].IsSpriteSheet()) {
              maxX = (x + _sprites[c].GetTexture().Width > maxX) ? x + _sprites[c].GetTexture().Width : maxX;
              maxY = (y + _sprites[c].GetTexture().Height > maxY) ? y + _sprites[c].GetTexture().Height : maxY;
            } else {
              maxX = (x + ((Rectangle)_sprites[c].GetSourceRectangle()).Width > maxX) ? x + ((Rectangle)_sprites[c].GetSourceRectangle()).Width : maxX;
              maxY = (y + ((Rectangle)_sprites[c].GetSourceRectangle()).Height > maxY) ? y + ((Rectangle)_sprites[c].GetSourceRectangle()).Height : maxY;
            }           }

          for (int c = 0; c < _animatedSprites.Count; c++) {
            List<ObjectSprite> objectSprites = _animatedSprites[c].GetAllImageFrames();

            for (int os = 0; os < objectSprites.Count; os++) {
              x = gameObject.GetDrawPosition().X - (objectSprites[os].pivotPoint.X * objectSprites[os].scale.X);
              y = gameObject.GetDrawPosition().Y - (objectSprites[os].pivotPoint.Y * objectSprites[os].scale.Y);

              if (c == 0 && _sprites.Count == 0) {
                minX = x;
                minY = y;
              } else {
                minX = (x < minX) ? x : minX;
                minY = (y < minY) ? y : minY;
              
              }
              maxX = (x + objectSprites[os].sourceRectangle.Width * objectSprites[os].scale.X > maxX) ? x + objectSprites[os].sourceRectangle.Width * objectSprites[os].scale.X : maxX;
              maxY = (y + objectSprites[os].sourceRectangle.Height * objectSprites[os].scale.Y > maxY) ? y + objectSprites[os].sourceRectangle.Height * objectSprites[os].scale.Y : maxY;

              //maxX = (x + _animatedSprites[c].GetImageFrame().sourceRectangle.Width * _animatedSprites[c].GetImageFrame().scale.X > maxX) ? x + _animatedSprites[c].GetImageFrame().sourceRectangle.Width * _animatedSprites[c].GetImageFrame().scale.X : maxX;
              //maxY = (y + _animatedSprites[c].GetImageFrame().sourceRectangle.Height * _animatedSprites[c].GetImageFrame().scale.Y > maxY) ? y + _animatedSprites[c].GetImageFrame().sourceRectangle.Height * _animatedSprites[c].GetImageFrame().scale.Y : maxY;
            }
          }

          maxRenderOffset = new Vector2(maxX - gameObject.GetDrawPosition().X, maxY - gameObject.GetDrawPosition().Y);
          minRenderOffset = new Vector2(minX - gameObject.GetDrawPosition().X, minY - gameObject.GetDrawPosition().Y);

          if (gameObject.GetId() == 1) {
            //Tools.Debug.Json(ToString() + " MAX BLA " + gameObject.GetId() + ": ", maxRenderOffset);
            //Tools.Debug.Json(ToString() + " MIN BLA " + gameObject.GetId() + ": ", minRenderOffset);
            //Tools.Debug.Json(ToString() + " BLA1 " + gameObject.GetId() + ": ", minX + ", " + maxX);
           // Tools.Debug.Json(ToString() + " BLA2 " + gameObject.GetId() + ": ", minY + ", " + maxY);
          }
        }
      }

      return Tuple.Create(minRenderOffset, maxRenderOffset);
    }

    Tuple<Vector3, Vector3> GetPhysicsOffset(GameObject gameObject, out List<Tuple<Vector2, Vector2>> underlyingSlopeOffset) {
      Vector3 minPhysicsOffset = new Vector3(0, 0, 0);
      Vector3 maxPhysicsOffset = new Vector3(0, 0, 0);
      underlyingSlopeOffset = new List<Tuple<Vector2, Vector2>>();

      if (gameObject.GetComponents<FixedPositionObject>().Count == 0) {
        float minX = 0, minY = 0, minZ = 0, maxX = 0, maxY = 0, maxZ = 0, z = 0;

        _collisionAreas = gameObject.GetComponents<CollisionArea>();
        if (gameObject.GetComponents<DirectionalCollisionArea>().Count > 0)
          _collisionAreas.AddRange(gameObject.GetComponents<DirectionalCollisionArea>()[0].GetAllCollisionAreas());

        if (gameObject.GetComponents<AnimatedSprite>().Count > 0)
          _collisionAreas.AddRange(gameObject.GetComponents<AnimatedSprite>()[0].GetAllCollisionAreas());
        
        if (_collisionAreas.Count > 0) {
          for (int c = 0; c < _collisionAreas.Count; c++) {
            if (_collisionAreas[c].rectangle.type == RectangleTypes.XYSLOPE_UP_LEFT || _collisionAreas[c].rectangle.type == RectangleTypes.XYSLOPE_DOWN_RIGHT || _collisionAreas[c].rectangle.HasZSlope()) {
              Tuple<Vector2, Vector2> slopeData = Tuple.Create(new Vector2(_collisionAreas[c].rectangle.X - gameObject.GetPosition().X, _collisionAreas[c].rectangle.Y - gameObject.GetPosition().Y),
                                                               new Vector2(_collisionAreas[c].rectangle.Right - gameObject.GetPosition().X, _collisionAreas[c].rectangle.Front - gameObject.GetPosition().Y));
              underlyingSlopeOffset.Add(slopeData);
            }

            float x = gameObject.GetPosition().X + _collisionAreas[c].GetOffset().X;
            float y = gameObject.GetPosition().Y + _collisionAreas[c].GetOffset().Y;
            z = gameObject.GetPosition().Z + _collisionAreas[c].GetOffset().Z - _collisionAreas[c].GetHeight();

            if (c == 0) {
              minX = x;
              minY = y;
              minZ = z;
              maxZ = z + _collisionAreas[c].GetHeight();
            } else {
              minX = (x < minX) ? x : minX;
              minY = (y < minY) ? y : minY;
              minY = (y < minY) ? y : minY;
              minZ = (z < minZ) ? z : minZ;
              maxZ = (z + _collisionAreas[c].GetHeight() > maxZ) ? z + _collisionAreas[c].GetHeight() : maxZ;
            }
            maxX = (x + _collisionAreas[c].GetLength() > maxX) ? x + _collisionAreas[c].GetLength() : maxX;
            maxY = (y + _collisionAreas[c].GetWidth() > maxY) ? y + _collisionAreas[c].GetWidth() : maxY;
          }

          maxPhysicsOffset = new Vector3(maxX - gameObject.GetPosition().X, maxY - gameObject.GetPosition().Y, minZ - gameObject.GetPosition().Z);
          minPhysicsOffset = new Vector3(minX - gameObject.GetPosition().X, minY - gameObject.GetPosition().Y, maxZ - gameObject.GetPosition().Z);
        }
      }

      return Tuple.Create(minPhysicsOffset, maxPhysicsOffset);
    }

    public List<int> GetFixedPositionObjects() {
      return objectFixedPosition;
    }
      
    public int Create<T>(float x, float y, float z, ObjectStatuses status = ObjectStatuses.ACTIVE) {
      int id = allGameObjects.Count;
      GameObject gameObject = (GameObject)(object)GameObject.Create<T>(id, x, y, z, sceneContextType, status);
      allGameObjects.Add(id, gameObject);

      if (!typePhysicsOffset.ContainsKey(gameObject.GetType())) {
        List<Tuple<Vector2, Vector2>> slopeData;
        typePhysicsOffset.Add(gameObject.GetType(), GetPhysicsOffset(gameObject, out slopeData));
        if (slopeData.Count > 0) {
          typeUnderlyingSlopeOffset.Add(gameObject.GetType(), slopeData);
        }
        typeRenderOffset.Add(gameObject.GetType(), GetRenderOffset(gameObject));
      }

      if (gameObject.GetComponents<FixedPositionObject>().Count > 0) {
        objectFixedPosition.Add(id);
      }

      if (status == ObjectStatuses.ACTIVE || status == ObjectStatuses.IDLE) {
        UpdateObject(id);
        UpdateObjectRendering(id);
      }
      return id;
    }

    public void Delete(int id) {
      for (int i = 0; i < objectPhysicsBuckets[id].Count; i++) {
        spatialPhysicsBuckets[objectPhysicsBuckets[id][i]].Remove(id);
      }
      for (int i = 0; i < objectRenderBuckets[id].Count; i++) {
        spatialRenderBuckets[objectRenderBuckets[id][i]].Remove(id);
      }

      objectPositions.Remove(id);
      objectDrawPositions.Remove(id);
      objectPhysicsBuckets.Remove(id);
      objectRenderBuckets.Remove(id);
      objectFixedPosition.Remove(id);

      allGameObjects[id].Dispose();
      allGameObjects.Remove(id);
    }

    public T GetGameObjectById<T>(int id) {
      if (allGameObjects.ContainsKey(id))
        return (T)(object)allGameObjects[id];
      return default(T);
    }

    public int SortBySlope(Tuple<int, int, Type> x, Tuple<int, int, Type> y) {
      bool _renderSlopeFront1 = RenderInFrontSlope(y.Item1, x.Item1);
      bool _renderSlopeFront2 = RenderInFrontSlope(x.Item1, y.Item1);

      int _result = 0;

      if (_renderSlopeFront1 && !_renderSlopeFront2) {
        _result = 1;
      } else if (_renderSlopeFront2 && !_renderSlopeFront1) {
        _result = -1;
      } 
      return _result;
    }


    public int Compare(Tuple<int, int, Type> x, Tuple<int, int, Type> y) {
      _currentObject = allGameObjects[x.Item1];
      _currentObject2 = allGameObjects[y.Item1];

      //Tools.Debug.Json("COMPARE " + _currentObject.GetType().Name + " TO " + _currentObject2.GetType().Name);

      _currentComponent = _currentObject.GetComponents()[x.Item2];
      _currentComponent2 = _currentObject2.GetComponents()[y.Item2];

      string name1 = (_currentComponent is Sprite) ? ((Sprite)_currentComponent).GetName() : ((AnimatedSprite)_currentComponent).GetName();
      string name2 = (_currentComponent2 is Sprite) ? ((Sprite)_currentComponent2).GetName() : ((AnimatedSprite)_currentComponent2).GetName();

      if (_currentObject.GetComponents<SpriteCollisionRegister>().Count > 0) {
        if (!_currentObject.GetComponents<SpriteCollisionRegister>()[0].GetObjectMinMaxSides(name1, out _minSides1, out _maxSides1)) {
          _maxSides1 = GetObjectMaxSides(x.Item1);
          _minSides1 = GetObjectMinSides(x.Item1);
        }
      } else {
        _maxSides1 = GetObjectMaxSides(x.Item1);
        _minSides1 = GetObjectMinSides(x.Item1);
      }

      if (_currentObject2.GetComponents<SpriteCollisionRegister>().Count > 0) {
        if (!_currentObject2.GetComponents<SpriteCollisionRegister>()[0].GetObjectMinMaxSides(name2, out _minSides2, out _maxSides2)) {
          _maxSides2 = GetObjectMaxSides(y.Item1);
          _minSides2 = GetObjectMinSides(y.Item1);
        }
      } else {
        _maxSides2 = GetObjectMaxSides(y.Item1);
        _minSides2 = GetObjectMinSides(y.Item1);
      }

      bool _renderSlopeFront1 = RenderInFrontSlope(y.Item1, x.Item1);
      bool _renderSlopeFront2 = RenderInFrontSlope(x.Item1, y.Item1);

      if (x.Item1 == 1 || y.Item1 == 1) {
       // Tools.Debug.Output("SLOPE FRONT1 - " + x.Item1, _renderSlopeFront1.ToString());
       // Tools.Debug.Output("SLOPE FRONT2 - " + y.Item1, _renderSlopeFront2.ToString());
      }

      int _result = 0;

      if (_minSides1.Z < _maxSides2.Z) {
        _result = 1;
      } else if (_minSides2.Z < _maxSides1.Z) {
        _result = -1;
      } else {
        if (_renderSlopeFront1 && !_renderSlopeFront2) {
          _result = 1;
        } else if (_renderSlopeFront2 && !_renderSlopeFront1) {
          _result = -1;
        } else if (_maxSides1.Y > _maxSides2.Y) {
          _result = 1;
        } else if (_maxSides2.Y > _maxSides1.Y) {
          _result = -1;
        } else if (_maxSides1.Z > _maxSides2.Z) {
          _result = -1;
        } else {
          _result = 1;
        }
      }

      //Tools.Debug.Json("RESULT", _result);

      return _result;

    }
  }
}
