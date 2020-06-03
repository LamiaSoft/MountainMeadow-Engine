using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Managers
{

    public class PhysicsManager {
    List<Tuple<int, int>> checkedPairs = new List<Tuple<int, int>>();
    List<Tuple<int, int, CollisionArea, CollisionArea>> collisions = new List<Tuple<int, int, CollisionArea, CollisionArea>>();
    Dictionary<Type, List<Tuple<int, int, CollisionArea, CollisionArea>>> storedCollisions = new Dictionary<Type, List<Tuple<int, int, CollisionArea, CollisionArea>>>();

    List<Tuple<int, Vector3>> contactPoints1, contactPoints2;
    List<CollisionArea> _collisionAreas1 = new List<CollisionArea>();
    List<CollisionArea> _collisionAreas2 = new List<CollisionArea>();
    Dictionary<int, bool> _passedIds = new Dictionary<int, bool>();


    public PhysicsManager() {

    }

    public void Update(GameTime gameTime, List<GameScene> gameScenes) {

      for (int i = 0; i < gameScenes.Count; i++) {
        if (gameScenes[i].GetStatus() == GameScene.SceneStatuses.INACTIVE || gameScenes[i].IsPaused()) {
          continue;
        }

        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.PRE_PHYSICS, this, gameScenes[i].GetType()).SetGameTime(gameTime));
        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.OBJECT_MOVEMENT, this, gameScenes[i].GetType()).SetGameTime(gameTime).SetObjectManager(gameScenes[i].GetObjectManager()));
        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.OBJECT_PHYSICS, this, gameScenes[i].GetType()).SetGameTime(gameTime).SetObjectManager(gameScenes[i].GetObjectManager()));
        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.PHYSICS, this, gameScenes[i].GetType()).SetGameTime(gameTime).SetObjectManager(gameScenes[i].GetObjectManager()));
        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.POST_PHYSICS, this, gameScenes[i].GetType()).SetGameTime(gameTime).SetObjectManager(gameScenes[i].GetObjectManager()));

        checkedPairs.Clear();
               
        foreach (Dictionary<int, bool> objectIdsDict in gameScenes[i].GetObjectManager().spatialPhysicsBuckets.Values) {
          _passedIds.Clear();

          foreach (int o in objectIdsDict.Keys) {
            if (gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetMovableEntity() == null || 
                (gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetStatus() != GameObject.ObjectStatuses.ACTIVE && 
                 gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetStatus() != GameObject.ObjectStatuses.IDLE)) {
              continue;
            }

            _passedIds.Add(o, true);

            foreach (int o2 in objectIdsDict.Keys) {
              if (!_passedIds.ContainsKey(o2) && !PairChecked(o, o2)) {
                
                _collisionAreas1 = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents<CollisionArea>();
                if (gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents<DirectionalCollisionArea>().Count > 0)
                  _collisionAreas1.AddRange(gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents<DirectionalCollisionArea>()[0].GetCollisionAreas());
                if (gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents<AnimatedSprite>().Count > 0)
                  _collisionAreas1.AddRange(gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents<AnimatedSprite>()[0].GetFrameCollisionAreas());
                
                _collisionAreas2 = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2).GetComponents<CollisionArea>();
                if (gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2).GetComponents<DirectionalCollisionArea>().Count > 0)
                  _collisionAreas2.AddRange(gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2).GetComponents<DirectionalCollisionArea>()[0].GetCollisionAreas());
                if (gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2).GetComponents<AnimatedSprite>().Count > 0)
                  _collisionAreas2.AddRange(gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2).GetComponents<AnimatedSprite>()[0].GetFrameCollisionAreas());

                bool firstLeading = (_collisionAreas1.Count >= _collisionAreas2.Count);

                for (int ca1 = 0; ca1 < ((firstLeading) ? _collisionAreas1.Count : _collisionAreas2.Count); ca1++) {
                  for (int ca2 = 0; ca2 < ((firstLeading) ? _collisionAreas2.Count : _collisionAreas1.Count); ca2++) {
                    if (firstLeading) {
                      if (_collisionAreas1[ca1].CheckCollision(_collisionAreas2[ca2], out this.contactPoints1, out this.contactPoints2)) {
                        EventManager.PushEvent(GameEvent.Create<CollisionEvent>(CollisionEvent.Values.ENTER, this).SetObjects(
                          gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o),
                          gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2)
                        ).SetContactPoints(this.contactPoints1, this.contactPoints2).SetCollisionAreas(_collisionAreas1[ca1], _collisionAreas2[ca2]));

                        AddCollision(o, o2, _collisionAreas1[ca1], _collisionAreas2[ca2], gameScenes[i].GetType(), true);
                        //ca2 = ((firstLeading) ? _collisionAreas2.Count : _collisionAreas1.Count);
                        //ca1 = ((firstLeading) ? _collisionAreas1.Count : _collisionAreas2.Count);
                      } 
                    } else {
                      if (_collisionAreas2[ca1].CheckCollision(_collisionAreas1[ca2], out this.contactPoints1, out this.contactPoints2)) {
                        EventManager.PushEvent(GameEvent.Create<CollisionEvent>(CollisionEvent.Values.ENTER, this).SetObjects(
                          gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o2),
                          gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o)
                        ).SetContactPoints(this.contactPoints1, this.contactPoints2).SetCollisionAreas(_collisionAreas2[ca1], _collisionAreas1[ca2]));

                        AddCollision(o, o2, _collisionAreas2[ca1], _collisionAreas1[ca2], gameScenes[i].GetType(), false);
                        //ca2 = ((firstLeading) ? _collisionAreas2.Count : _collisionAreas1.Count);
                        //ca1 = ((firstLeading) ? _collisionAreas1.Count : _collisionAreas2.Count);
                      }
                    }
                  }
                }

                StorePair(o, o2);
              }
            }
          }
        }

        if (storedCollisions.ContainsKey(gameScenes[i].GetType())) {
          for (int c = 0; c < storedCollisions[gameScenes[i].GetType()].Count; c++) {
            EventManager.PushEvent(GameEvent.Create<CollisionEvent>(CollisionEvent.Values.EXIT, this).SetObjects(
              gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(storedCollisions[gameScenes[i].GetType()][c].Item1),
                gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(storedCollisions[gameScenes[i].GetType()][c].Item2)).
              SetCollisionAreas(storedCollisions[gameScenes[i].GetType()][c].Item3, storedCollisions[gameScenes[i].GetType()][c].Item4));
          }
        }

        storedCollisions.Clear();
        storedCollisions.Add(gameScenes[i].GetType(), new List<Tuple<int, int, CollisionArea, CollisionArea>>());
        for (int c = 0; c < collisions.Count; c++) {
          storedCollisions[gameScenes[i].GetType()].Add(collisions[c]);
        }
        collisions.Clear();
      
        for (int camNum = 0; camNum < gameScenes[i].GetCameras().Count; camNum++) {
          gameScenes[i].GetCameras()[camNum].Update(gameTime);
        }

        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.POST_COLLISION, this, gameScenes[i].GetType()).SetGameTime(gameTime).SetObjectManager(gameScenes[i].GetObjectManager()));

      }

    }

    private void AddCollision(int object1, int object2, CollisionArea collisionArea1, CollisionArea collisionArea2, Type sceneType, bool firstLeading = true) {
      Tuple<int, int, CollisionArea, CollisionArea> t;

      if (object1 < object2) {
        if (firstLeading) {
          t = Tuple.Create(object1, object2, collisionArea1, collisionArea2);
        } else {
          t = Tuple.Create(object1, object2, collisionArea2, collisionArea1);
        }
      } else {
        if (firstLeading) {
          t = Tuple.Create(object2, object1, collisionArea2, collisionArea1);
        } else {
          t = Tuple.Create(object2, object1, collisionArea1, collisionArea2);
        }
      }

      collisions.Add(t);
      if (storedCollisions.ContainsKey(sceneType) && storedCollisions[sceneType].Contains(t)) {
        storedCollisions[sceneType].Remove(t);    
      }
    }

    //private bool IsStoredCollision(int object1, int object2) {
    //  Tuple<int, int> t = (object1 < object2) ? Tuple.Create(object1, object2) : Tuple.Create(object2, object1);
    //  return (storedCollisions.Contains(t));
    //}

    private bool PairChecked(int object1, int object2) {
      Tuple<int, int> t = (object1 < object2) ? Tuple.Create(object1, object2) : Tuple.Create(object2, object1);
      return (checkedPairs.Contains(t));
    }

    private void StorePair(int object1, int object2) {
      Tuple<int, int> t = (object1 < object2) ? Tuple.Create(object1, object2) : Tuple.Create(object2, object1);
      checkedPairs.Add(t);
    }

  }
}
