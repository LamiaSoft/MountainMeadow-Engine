using System;
using System.Collections.Generic;

namespace MountainMeadowEngine.Behavior {
  
  public class UntilFail : BehaviorTreeNode {

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (childrenCount != 1) {
        throw new Exception("\"UntilFail\" behavior tree node requires 1 child node.");
      }

      BehaviorStatutes result = BehaviorStatutes.RUNNING;

      while (result != BehaviorStatutes.FAILURE) {
        for (int i = childIndex; i < childrenCount; i++) {
          result = GetChildNode(i).Run(ref metaData);

          if (result == BehaviorStatutes.RUNNING) {
            runningIndex = i;
            return result;
          }
        }
      }

      return result;
    }
  }
}
