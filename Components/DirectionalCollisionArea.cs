using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using static MountainMeadowEngine.Components.MovableEntity;

namespace MountainMeadowEngine.Components {
  
  public class DirectionalCollisionArea : GameObjectComponent {
    Dictionary<Direction, List<CollisionArea>> collisionAreas = new Dictionary<Direction, List<CollisionArea>>();
    List<Direction> availableDirections = new List<Direction>();
    Direction _currentDirection = Direction.NONE;

    public DirectionalCollisionArea(GameObject context) : base(context) {
      EventManager.AddEventListener<PositionEvent>(this);
    }

    public DirectionalCollisionArea AddCollisionArea(Direction direction, CollisionArea collisionArea) {
      if (!collisionAreas.ContainsKey(direction)) {
        collisionAreas.Add(direction, new List<CollisionArea>());
        availableDirections.Add(direction);
      }
      //EventManager.RemoveEventListener<UpdateEvent>(collisionArea, UpdateEvent.Values.OBJECT_PHYSICS);
      collisionAreas[direction].Add(collisionArea);
      return this;
    }

    public List<CollisionArea> GetCollisionAreas() {
      Direction direction = Direction.NONE;
      if (context.GetMovableEntity() != null) {
        direction = MovableEntity.GetNearestDirection(context.GetMovableEntity().GetCurrentDirection(), availableDirections);
      }
      if (collisionAreas.ContainsKey(direction)) {
        return collisionAreas[direction];
      }
      return new List<CollisionArea>();
    }

    public List<CollisionArea> GetAllCollisionAreas() {
      List<CollisionArea> returnValue = new List<CollisionArea>();
      foreach (List<CollisionArea> list in collisionAreas.Values) {
        returnValue.AddRange(list);
      }
      return returnValue;
    }

    public override void Initialize() {
      
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      foreach (Direction dir in collisionAreas.Keys) {
        for (int i = 0; i < collisionAreas[dir].Count; i++) {
          collisionAreas[dir][i].OnEvent(gameEvent);
        }
      }

      return gameEvent;
    }
  }
}
