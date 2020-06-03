using System;
using System.Collections.Generic;

namespace MountainMeadowEngine.Behavior {

  public class Inverter : BehaviorTreeNode {
    
    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (childrenCount != 1) {
        throw new Exception("\"Inverter\" behavior tree node requires 1 child node.");
      }

      BehaviorStatutes result = GetChildNode(0).Run(ref metaData);

      if (result == BehaviorStatutes.RUNNING) {
        runningIndex = 0;
        return result;
      }

      return ((result == BehaviorStatutes.SUCCESS) ? BehaviorStatutes.FAILURE : BehaviorStatutes.SUCCESS);
    }

  }
}
