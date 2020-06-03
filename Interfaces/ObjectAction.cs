using System;
using System.Collections.Generic;
using System.Reflection;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Interfaces {
  
  public abstract class ObjectAction : IDisposable {
    int animationIndex = -1;
    Type animationType;
    List<AnimatedSprite> _list;
    protected GameObject gameObject;

    public ObjectAction(GameObject context, Type animationType = null) {
      gameObject = context;
      if (animationType != null) {
        _list = gameObject.GetComponents<AnimatedSprite>();
        if (_list.Count > 0) {
          animationIndex = _list[0].GetAnimationIndexFromType(animationType);
          this.animationType = animationType;
        }
      }
    }

    public virtual void Dispose() { }

    public abstract bool Run(ActionEvent.Values currentActionName, ActionEvent.Values? previousActionName, bool currentAnimationFinished, GameEvent gameEvent);
    public abstract ActionEvent AnimationStopped(ActionEvent.Values? previousActionName);

    public Type GetAnimationType() {
      return animationType;
    }

    protected virtual void StartAnimation(bool deactivateOtherAnimations = true) {
      if (animationIndex > -1) {
        _list = gameObject.GetComponents<AnimatedSprite>();
        if (_list.Count > 0) {
          _list[0].Start(animationIndex, deactivateOtherAnimations);
        }
      }
    }

    protected virtual void StopAnimation(bool deactivateAnimation = false) {
      if (animationIndex > -1) {
        _list = gameObject.GetComponents<AnimatedSprite>();
        if (_list.Count > 0) {
          _list[0].Start(animationIndex, deactivateAnimation);
        }
      }
    }

    protected virtual void PauseAnimation() {
      if (animationIndex > -1) {
        _list = gameObject.GetComponents<AnimatedSprite>();
        if (_list.Count > 0) {
          _list[0].Pause(animationIndex);
        }
      }
    }

    protected virtual void UnPauseAnimation() {
      if (animationIndex > -1) {
        _list = gameObject.GetComponents<AnimatedSprite>();
        if (_list.Count > 0) {
          _list[0].UnPause(animationIndex);
        }
      }
    }

  }
}
