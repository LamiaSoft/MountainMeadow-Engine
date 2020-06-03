using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Behavior {

  public class WalkToObject : BehaviorTreeNode {
    GameObject targetObject;
    Vector2 location;
    float lastKnownX, lastKnownY;

    public WalkToObject(GameObject targetObject) {
      this.targetObject = targetObject;
    }

    public WalkToObject(GameObject gameObject, GameObject targetObject) {
      this.gameObject = gameObject;
      this.targetObject = targetObject;
    }

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      int dirX = 0, dirY = 0;

      if (gameObject.GetPosition().X == lastKnownX && gameObject.GetPosition().Y == lastKnownY) {
        return BehaviorStatutes.FAILURE;
      }

      lastKnownX = gameObject.GetPosition().X;
      lastKnownY = gameObject.GetPosition().Y;

      location.X = targetObject.GetPosition().X;
      location.Y = targetObject.GetPosition().Y;

      if (gameObject.GetPosition().X != location.X) {
        if (gameObject.GetPrevPosition().X < location.X && gameObject.GetPosition().X < location.X ||
            gameObject.GetPrevPosition().X > location.X && gameObject.GetPosition().X > location.X) {
          dirX = (gameObject.GetPosition().X < location.X) ? 1 : -1;
        }
      }

      if (gameObject.GetPosition().Y != location.Y) {
        if (gameObject.GetPrevPosition().Y < location.Y && gameObject.GetPosition().Y < location.Y ||
            gameObject.GetPrevPosition().Y > location.Y && gameObject.GetPosition().Y > location.Y) {
          dirY = (gameObject.GetPosition().Y < location.Y) ? 1 : -1;
        }
      }

      if (dirX == 0 || dirY == 0) {
        float? x = null, y = null;
        if (dirX == 0)
          x = location.X;
        if (dirY == 0)
          y = location.Y;

        gameObject.SetPosition(x, y, null);
      }

      gameObject.PushEventToComponents(GameEvent.Create<ActionEvent>(ActionEvent.Values.MOVE, this).SetMoveParams(dirX, dirY, null));
      gameObject.GetMovableEntity().SetDirection(dirX, dirY, null);

      if ((dirX == 0 && dirY == 0)) {
        return BehaviorStatutes.SUCCESS;
      }

      return BehaviorStatutes.RUNNING;
    }

  }
}

