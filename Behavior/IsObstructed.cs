using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Behavior {
  public class IsObstructed : BehaviorTreeNode, IEventListener {
    Vector3 currentImpact = new Vector3();
    CollisionArea collidedArea;
    Vector3 checkSides = new Vector3();

    public IsObstructed(bool checkX, bool checkY, bool checkZ) {
      checkSides.X = (checkX) ? 1 : 0;
      checkSides.Y = (checkY) ? 1 : 0;
      checkSides.Z = (checkZ) ? 1 : 0;
      EventManager.AddEventListener<CollisionEvent>(this);
    }

    public IsObstructed(GameObject gameObject, bool checkX, bool checkY, bool checkZ) {
      this.gameObject = gameObject;
      checkSides.X = (checkX) ? 1 : 0;
      checkSides.Y = (checkY) ? 1 : 0;
      checkSides.Z = (checkZ) ? 1 : 0;
      EventManager.AddEventListener<CollisionEvent>(this);
    }

    public GameEvent OnEvent(GameEvent gameEvent) {
      if ((CollisionEvent.Values)gameEvent.GetValue() == CollisionEvent.Values.ENTER) {
        if (((CollisionEvent)gameEvent).GetObject1() == gameObject) {
          currentImpact = ((CollisionEvent)gameEvent).GetImpact();
          if (Collided(currentImpact))   
            collidedArea = ((CollisionEvent)gameEvent).GetCollisionArea2();
        } else if (((CollisionEvent)gameEvent).GetObject2() == gameObject) {
          currentImpact = ((CollisionEvent)gameEvent).GetImpact();
          if (Collided(currentImpact))
            collidedArea = ((CollisionEvent)gameEvent).GetCollisionArea1();
        }
      } 
      return gameEvent;
    }

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (collidedArea == null) {
        return BehaviorStatutes.FAILURE;
      }

      if (metaData.ContainsKey("IsObstructed.collidedArea"))
          metaData.Remove("IsObstructed.collidedArea");
        
      metaData.Add("IsObstructed.collidedArea", collidedArea);

      collidedArea = null;
      return BehaviorStatutes.SUCCESS;
    }

    private bool Collided(Vector3 currentImpact) {
      return ((checkSides.X == 1 && Math.Abs(currentImpact.X) > 0) ||
              (checkSides.Y == 1 && Math.Abs(currentImpact.Y) > 0) ||
              (checkSides.Z == 1 && Math.Abs(currentImpact.Z) > 0));
    }
  }
}
