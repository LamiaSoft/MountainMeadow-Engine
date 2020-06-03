using System;
using System.Collections.Generic;

namespace MountainMeadowEngine.Behavior {
  
  public class Sequence : BehaviorTreeNode {
    
    public Sequence(bool keepRunningIndex = true) : base(keepRunningIndex) {
    }

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (childrenCount < 2) {
        throw new Exception("\"Sequence\" behavior tree node requires at least 2 child nodes.");
      }

      BehaviorStatutes result;

      for (int i = childIndex; i < childrenCount; i++) {
        result = GetChildNode(i).Run(ref metaData);
        if (result == BehaviorStatutes.FAILURE) {
          return result;
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
