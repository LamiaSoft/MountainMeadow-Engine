using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Behavior {
  public class ObstructionIsJumpable : BehaviorTreeNode {
    CollisionArea collidedArea;

    public ObstructionIsJumpable(CollisionArea collidedArea = null) {
      this.collidedArea = collidedArea;
    }

    public ObstructionIsJumpable(GameObject gameObject, CollisionArea collidedArea = null) {
      this.gameObject = gameObject;
      this.collidedArea = collidedArea;
    }

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      CollisionArea col;

      if (collidedArea != null) {
        col = collidedArea;
      } else if (metaData.ContainsKey("IsObstructed.collidedArea")) {
        col = (CollisionArea)metaData["IsObstructed.collidedArea"];
      } else {
        return BehaviorStatutes.FAILURE;
      }

      if (col.rectangle.Top > gameObject.GetPosition().Z - gameObject.GetMovableEntity().GetJumpingHeight()) {
        Console.WriteLine("OBSTRUCTION IS JUMPABLE");
        return BehaviorStatutes.SUCCESS;
      }

      return BehaviorStatutes.FAILURE;
    }
  }
}
