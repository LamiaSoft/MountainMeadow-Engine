using System;
using System.Collections.Generic;
using MountainMeadowEngine.Components;

namespace MountainMeadowEngine.Behavior {
  
  public abstract class BehaviorTreeNode : IDisposable {
    public enum BehaviorStatutes { FAILURE, SUCCESS, RUNNING }

    protected int childrenCount;
    protected int runningIndex = -1;

    bool keepRunningIndex;
    protected GameObject gameObject;
    List<BehaviorTreeNode> childrenNodes = new List<BehaviorTreeNode>();

    public BehaviorTreeNode(bool keepRunningIndex = true) {
      this.keepRunningIndex = keepRunningIndex;
    }

    public GameObject GetGameObject() {
      return gameObject;  
    }

    public void SetGameObject(GameObject gameObject) {
      this.gameObject = gameObject;

      for (int i = 0; i < childrenCount; i++) {
        if (childrenNodes[i].GetGameObject() == null) {
          childrenNodes[i].SetGameObject(this.gameObject);
        } else {
          childrenNodes[i].SetGameObject(childrenNodes[i].GetGameObject());
        }
      }
    }

    public BehaviorTreeNode AddChildNode(BehaviorTreeNode node) {
      childrenNodes.Add(node);
      childrenCount++;
      return this;
    }

    public BehaviorTreeNode GetChildNode(int index) {
      return childrenNodes[index];
    }

    public BehaviorStatutes Run(ref Dictionary<string, object> metaData) {
      int childIndex = ((runningIndex == -1 || keepRunningIndex == false) ? 0 : runningIndex);
      runningIndex = -1;
      BehaviorStatutes status = Run(childIndex, ref metaData);
      return status;
    }

    public abstract BehaviorStatutes Run(int childIndex, ref Dictionary<string, object> metaData);

    public void Erase() {
      runningIndex = -1;
      for (int i = 0; i < childrenCount; i++) {
        childrenNodes[i].Erase();
      }
    }

    public virtual void Dispose() {
      for (int i = 0; i < childrenCount; i++) {
        childrenNodes[i].Dispose();
      }
      gameObject = null;
      childrenNodes.Clear();
    }
  }
}
