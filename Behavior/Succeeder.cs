using System;
using System.Collections.Generic;

namespace MountainMeadowEngine.Behavior {

  public class Succeeder : BehaviorTreeNode {
    
    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (childrenCount == 0) {
        throw new Exception("\"Succeeder\" behavior tree node requires at least 1 child node.");
      }

      BehaviorStatutes result;

      for (int i = childIndex; i < childrenCount; i++) {
        result = GetChildNode(i).Run(ref metaData);
        if (result == BehaviorStatutes.FAILURE) {
          return BehaviorStatutes.SUCCESS;
        }
        if (result == BehaviorStatutes.RUNNING) {
          runningIndex = i;
          return result;
        }
      }

      return BehaviorStatutes.SUCCESS;
    }

  }
}
