using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Behavior;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using static MountainMeadowEngine.Behavior.BehaviorTreeNode;

namespace MountainMeadowEngine.Components {
  
  public class BehaviorTree : GameObjectComponent {

    double runFrequency;
    double accumulator = 0;

    BehaviorTreeNode mainNode;
    BehaviorStatutes status = BehaviorStatutes.RUNNING;
    Dictionary<string, object> metaData = new Dictionary<string, object>();

    public BehaviorTree(GameObject context) : base(context) {
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.PRE_PHYSICS);
    }

    public void Restart() {
      status = BehaviorStatutes.RUNNING;
      metaData.Clear();
      accumulator = 0;
      if (mainNode != null)
        mainNode.Erase();
    }

    public BehaviorTree SetMainNode(BehaviorTreeNode mainNode) {
      if (mainNode.GetGameObject() == null) {
        mainNode.SetGameObject(context);
      }
      this.mainNode = mainNode;
      return this;
    }

    public BehaviorTree SetRunFrequency(double frequency) {
      this.runFrequency = frequency;
      return this;
    }

    public BehaviorStatutes GetStatus() {
      return status;
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      if (gameEvent is UpdateEvent && (UpdateEvent.Values)gameEvent.GetValue() == UpdateEvent.Values.PRE_PHYSICS) {
        Run(((UpdateEvent)gameEvent).GetGameTime());
      }
      return gameEvent;
    }

    public void Run(GameTime gameTime) {
      if (mainNode != null && status == BehaviorStatutes.RUNNING) {
        if (runFrequency == 0) {
          status = mainNode.Run(ref metaData);
          return;
        }

        accumulator += gameTime.ElapsedGameTime.TotalSeconds;

        while (accumulator >= runFrequency) {
          status = this.mainNode.Run(ref metaData);

          if (status != BehaviorStatutes.RUNNING) {
            accumulator = 0;
            return;
          }

          accumulator -= runFrequency;
        }
      }
    }

    public override void Dispose() {
      base.Dispose();
      mainNode.Dispose();
      mainNode = null;
    }

    public override void Initialize() {
      
    }

  }
}
