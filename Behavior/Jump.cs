using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Behavior {
  public class Jump : BehaviorTreeNode {
    bool jumped = false;

    public Jump() { }

    public Jump(GameObject gameObject) {
      this.gameObject = gameObject;
    }

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (gameObject.groundedOn == null && jumped == false)
        return BehaviorStatutes.FAILURE;

      if (jumped == false) {
        gameObject.GetMovableEntity().Jump();
        Console.WriteLine("JUMP!!");
        jumped = true;
      } else if (gameObject.groundedOn != null) {
        jumped = false;
        Console.WriteLine("JUMP FINISHED");
        return BehaviorStatutes.SUCCESS;
      }

      return BehaviorStatutes.RUNNING;
    }
  }
}
