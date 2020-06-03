using System;
using System.Collections.Generic;

namespace MountainMeadowEngine.Behavior {
  
  public class Repeater : BehaviorTreeNode {
    int numberOfRepeats;
    int currentLoop = 0;

    public Repeater(int numberOfRepeats = -1) {
      this.numberOfRepeats = numberOfRepeats;
    }

    public override BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData) {
      if (childrenCount != 1) {
        throw new Exception("\"Repeater\" behavior tree node require 1 child node.");
      }

      BehaviorStatutes result = BehaviorStatutes.RUNNING;
      int firstRunning = -1;

      while (numberOfRepeats == -1 || currentLoop < numberOfRepeats) {
        firstRunning = -1;

        for (int i = childIndex; i < childrenCount; i++) {
          result = GetChildNode(i).Run(ref metaData);

          if (result == BehaviorStatutes.RUNNING) {
            if (numberOfRepeats == -1) {
              runningIndex = i;
              return result;
            } else if (firstRunning == -1) {
              firstRunning = i;
            }
          }
        }
      }

      if (result == BehaviorStatutes.RUNNING) {
        runningIndex = firstRunning;
        return result;
      }

      return BehaviorStatutes.SUCCESS;
    }
  }
}
