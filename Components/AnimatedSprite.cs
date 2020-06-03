using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Managers;
using MountainMeadowEngine.Objects;
using MountainMeadowEngine.TexturePacker;
using static MountainMeadowEngine.Components.Animation;
using static MountainMeadowEngine.Components.MovableEntity;

namespace MountainMeadowEngine.Components {
  public class AnimatedSprite : GameObjectComponent {
    private string name;
    public enum AnimationStatus { ACTIVE, INACTIVE };

    private List<SequenceTypes> cycleTypes = new List<SequenceTypes> { SequenceTypes.REGULAR_CYCLE, SequenceTypes.REVERSE_CYCLE, 
                                                                       SequenceTypes.REGULAR_REPEAT_CYCLE, SequenceTypes.REVERSE_REPEAT_CYCLE };

    private List<SequenceTypes> reverseTypes = new List<SequenceTypes> { SequenceTypes.REVERSE, SequenceTypes.REVERSE_CYCLE,
                                                                         SequenceTypes.REVERSE_REPEAT, SequenceTypes.REVERSE_REPEAT_CYCLE };
    private List<Animation> animations = new List<Animation>();
    private Dictionary<Type, int> animationTypes = new Dictionary<Type, int>();

    private List<AnimationStatus> animationStatuses = new List<AnimationStatus>();
    //private List<int> = currentAnimations new List

    private List<ObjectSprite> currentImageFrames = new List<ObjectSprite>();
    private List<double> accumulators = new List<double>(), alpha = new List<double>();
    private List<bool> running = new List<bool>();
    private List<int> currentFrames = new List<int>();

    private Direction lastKnownDirection;
    private List<CollisionArea> collisionAreas = new List<CollisionArea>();

    private bool updated = false;



    public AnimatedSprite(GameObject context) : base(context) {
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.OBJECT_PHYSICS);
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.OBJECT_RENDERING);
      EventManager.AddEventListener<PositionEvent>(this);
    }

    public AnimatedSprite SetName(string name) {
      this.name = name;
      return this;
    }

    public string GetName() {
      return this.name;
    }

    public List<Type> GetCurrentAnimations() {
      List<Type> list = new List<Type>();
      for (int i = 0; i < animationStatuses.Count; i++) {
        if (animationStatuses[i] == AnimationStatus.ACTIVE) {
          list.Add(animations[i].GetType());
        }
      }
      return list;
    } 

    public int GetAnimationIndexFromType(Type type) {
      if (animationTypes.ContainsKey(type)) {
        return animationTypes[type];
      } else {
        throw new Exception("Cannot get animation index for \"" + type.Name + "\". Not found.");
      }
    }

    public AnimatedSprite AddAnimation<T>() {
      int id;
      AddAnimation<T>(out id);
      return this;
    }

    public AnimatedSprite AddAnimation<T>(out int id) {
      if (!animationTypes.ContainsKey(typeof(T))) {
        T instance = (T)Activator.CreateInstance(typeof(T), context);
        Animation animation = (Animation)(object)instance;
        animations.Add(animation);
        animationTypes.Add(animation.GetType(), animations.Count - 1);
        id = animations.Count - 1;

        animationStatuses.Add(AnimationStatus.INACTIVE);
        currentImageFrames.Add(new ObjectSprite());
        accumulators.Add(0);
        alpha.Add(0);
        running.Add(false);
        currentFrames.Add(0);
        return this;
      } else {
        throw new Exception("Animation \"" + typeof(T).Name + "\" has already been added.");
      }
    }

    public void Start<T>(bool deactivateOthers = true, bool pushEvent = false) {
      Start(GetAnimationIndexFromType(typeof(T)), deactivateOthers, pushEvent);
    }

    public void Stop<T>(bool deactivate = false, bool pushEvent = false) {
      Stop(GetAnimationIndexFromType(typeof(T)), deactivate, pushEvent);
    }

    public void Reset<T>() {
      Reset(GetAnimationIndexFromType(typeof(T)));
    }

    public void Pause<T>(bool pushEvent = false) {
      Pause(GetAnimationIndexFromType(typeof(T)), pushEvent);
    }

    public void UnPause<T>(bool pushEvent = false) {
      UnPause(GetAnimationIndexFromType(typeof(T)), pushEvent);
    }

    public void IsRunning<T>() {
      IsRunning(GetAnimationIndexFromType(typeof(T)));
    }

    public void Start(int index, bool deactivateOthers = true, bool pushEvent = false) {
      if (deactivateOthers) {
        DeactivateAll();
      }
      animationStatuses[index] = AnimationStatus.ACTIVE;
      running[index] = true;
      Reset(index);
      if (pushEvent)
        context.PushEventToComponents(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.STARTED, this).SetAnimationType(animations[index].GetType()));
    }

    public void Stop(int index, bool deactivate = false, bool pushEvent = false) {
      running[index] = false;
      Reset(index);
      if (deactivate) {
        animationStatuses[index] = AnimationStatus.INACTIVE;
      }

      if (pushEvent)
        context.PushEventToComponents(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.STOPPED, this).SetAnimationType(animations[index].GetType()));
    }

    private void DeactivateAll() {
      for (int i = 0; i < animationStatuses.Count; i++) {
        if (animationStatuses[i] == AnimationStatus.ACTIVE)
          Stop(i, true);
      } 
    }

    private void Reset(int index) {
      updated = false;
      accumulators[index] = 0;
      currentFrames[index] = 0;
      alpha[index] = 0;
    }

    public void Pause(int index, bool pushEvent = false) {
      running[index] = false;
      if (pushEvent)
        context.PushEventToComponents(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.PAUSED, this).SetAnimationType(animations[index].GetType()));
    }

    public void UnPause(int index, bool pushEvent = false) {
      running[index] = true;
      if (pushEvent)
        context.PushEventToComponents(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.UNPAUSED, this).SetAnimationType(animations[index].GetType()));
    }

    public bool IsRunning(int index) {
      return running[index];
    }

    public List<ObjectSprite> GetImageFrames() {
      List<ObjectSprite> list = new List<ObjectSprite>();
      for (int i = 0; i < animationStatuses.Count; i++) {
        if (animationStatuses[i] == AnimationStatus.ACTIVE) {
          list.Add(currentImageFrames[i]);
        }
      }
      return list;
    }  

    public List<CollisionArea> GetFrameCollisionAreas() {
      return collisionAreas;
    }

    public List<CollisionArea> GetAllCollisionAreas() {
      List<CollisionArea> areas = new List<CollisionArea>();

      for (int i = 0; i < animations.Count; i++) {
        areas.AddRange(animations[i].GetCollisionAreas());
      }
      return areas;
    }

    public List<ObjectSprite> GetAllImageFrames() {
      List<string> frameNames;
      List<ObjectSprite> sprites = new List<ObjectSprite>();

      for (int i = 0; i < animations.Count; i++) {
        frameNames = GameContentManager.LoadSpriteSheet(animations[i].GetSpriteSheetFile()).GetFrameNames("*");
        for (int n = 0; n < frameNames.Count; n++) {
          ObjectSprite sprite = new ObjectSprite();
          sprite.SetSprite(GameContentManager.LoadSpriteSheet(animations[i].GetSpriteSheetFile()).Sprite(frameNames[n]), Color.White, 0, animations[i].GetScale(), SpriteEffects.None);
          sprites.Add(sprite);
        }
      }
      return sprites;
    }


    private bool UpdateCurrentFrame<T>(Direction direction, Dictionary<Direction, List<T>> list, int currentAnimation) {
      if (!list.ContainsKey(direction) || list[direction].Count == 0) {
        return false;
      }

      if (animations[currentAnimation].GetDuration() == 0) {
        currentFrames[currentAnimation] = (animations[currentAnimation].GetSequenceType() == SequenceTypes.REVERSE) ? list[direction].Count - 1 : 0;
        return true;
      }

      if (accumulators[currentAnimation] >= (animations[currentAnimation].GetDuration() * 2)) {
        if (animations[currentAnimation].GetSequenceType() == SequenceTypes.REGULAR_REPEAT_CYCLE || animations[currentAnimation].GetSequenceType() == SequenceTypes.REVERSE_REPEAT_CYCLE) {
          accumulators[currentAnimation] -= (animations[currentAnimation].GetDuration() * 2);
        } else {
          accumulators[currentAnimation] = 0;
          currentFrames[currentAnimation] = (animations[currentAnimation].GetSequenceType() == SequenceTypes.REGULAR_CYCLE) ? 0 : list[direction].Count - 1;
          running[currentAnimation] = false;
        }
      }

      if (accumulators[currentAnimation] >= animations[currentAnimation].GetDuration()) {
        if (animations[currentAnimation].GetSequenceType() == SequenceTypes.REGULAR_REPEAT || animations[currentAnimation].GetSequenceType() == SequenceTypes.REVERSE_REPEAT) {
          accumulators[currentAnimation] -= animations[currentAnimation].GetDuration();
        } else if (!cycleTypes.Contains(animations[currentAnimation].GetSequenceType())) {
          accumulators[currentAnimation] = 0;
          currentFrames[currentAnimation] = (animations[currentAnimation].GetSequenceType() == SequenceTypes.REGULAR) ? list[direction].Count - 1 : 0;
          running[currentAnimation] = false;
        }
      }

      if (running[currentAnimation]) {
        alpha[currentAnimation] = accumulators[currentAnimation] / animations[currentAnimation].GetDuration();
        if (alpha[currentAnimation] <= 1) {
          currentFrames[currentAnimation] = (int)Math.Floor(alpha[currentAnimation] * (list[direction].Count));
        } else {
          currentFrames[currentAnimation] = (list[direction].Count - 1) - (int)Math.Floor((alpha[currentAnimation] - 1) * (list[direction].Count));
        }

        if (reverseTypes.Contains(animations[currentAnimation].GetSequenceType())) {
          currentFrames[currentAnimation] = (list[direction].Count - 1) - currentFrames[currentAnimation];
        }
      }

      return true;
    }

    public override void Initialize() {
      
    }

    protected virtual void Update(GameTime gameTime) {
      for (int i = 0; i < animationStatuses.Count; i++) {
        if (animationStatuses[i] == AnimationStatus.ACTIVE) {
          int currentAnimation = i;

          if (running[currentAnimation]) {             if (animations[currentAnimation].GetDuration() > 0) {               accumulators[currentAnimation] += gameTime.ElapsedGameTime.TotalSeconds;             }              Direction direction = Direction.NONE;              if (context.GetMovableEntity() != null) {               direction = MovableEntity.GetNearestDirection(context.GetMovableEntity().GetCurrentDirection(), animations[currentAnimation].GetAvailableDirections());             } 
            int frame = currentFrames[currentAnimation];             bool updateSuccess = (UpdateCurrentFrame(direction, animations[currentAnimation].GetAnimationFrames(), currentAnimation));             if (!updateSuccess) {               if (lastKnownDirection != Direction.NONE) {                 direction = lastKnownDirection;                 updateSuccess = (UpdateCurrentFrame(direction, animations[currentAnimation].GetAnimationFrames(), currentAnimation));               } else {                 direction = Direction.DEFAULT;                 updateSuccess = (UpdateCurrentFrame(direction, animations[currentAnimation].GetAnimationFrames(), currentAnimation));               }

              if (!updateSuccess) {                 if (animations[currentAnimation].GetDuration() > 0) {                   accumulators[currentAnimation] -= gameTime.ElapsedGameTime.TotalSeconds;                 }                 //context.PushEventToComponents(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.STOPPED, this).SetAnimationType(animations[currentAnimation].GetType()));                 return;               }             } else {               if (direction != Direction.NONE) {                 lastKnownDirection = direction;               }             } 
            if (currentFrames[currentAnimation] != frame) {
              context.OnEvent(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.FRAME_CHANGE, this).SetCurrentFrame(currentFrames[currentAnimation]).SetAnimationType(animations[currentAnimation].GetType()));
            }
             if (!animations[currentAnimation].GetDirectionFlips().Contains(direction)) {               currentImageFrames[currentAnimation].SetSprite(GameContentManager.LoadSpriteSheet(animations[currentAnimation].GetSpriteSheetFile()).Sprite(animations[currentAnimation].GetAnimationFrame(direction, currentFrames[currentAnimation])), Color.White, 0, animations[currentAnimation].GetScale(), SpriteEffects.None);             } else {               currentImageFrames[currentAnimation].SetSprite(GameContentManager.LoadSpriteSheet(animations[currentAnimation].GetSpriteSheetFile()).Sprite(animations[currentAnimation].GetAnimationFrame(direction, currentFrames[currentAnimation])), Color.White, 0, animations[currentAnimation].GetScale(), SpriteEffects.FlipHorizontally);             }             collisionAreas = animations[currentAnimation].GetFrameCollisionAreas(direction, currentFrames[currentAnimation]);              if (running[currentAnimation] == false) {               context.PushEventToComponents(GameEvent.Create<AnimationEvent>(AnimationEvent.Values.STOPPED, this).SetAnimationType(animations[currentAnimation].GetType()));             }           }
        }
      }
    } 
    public override GameEvent OnEvent(GameEvent gameEvent) {
      if (gameEvent is UpdateEvent) {
        if ((UpdateEvent.Values)gameEvent.GetValue() == UpdateEvent.Values.OBJECT_PHYSICS) {
          Update(((UpdateEvent)gameEvent).GetGameTime());
          updated = true;
        }

        if ((UpdateEvent.Values)gameEvent.GetValue() == UpdateEvent.Values.OBJECT_RENDERING) {
          if (updated == false) {
            Update(((UpdateEvent)gameEvent).GetGameTime());
          }
          updated = false;
        }
      }

      if (gameEvent is PositionEvent) {
        List<CollisionArea> areas = GetAllCollisionAreas();
        for (int i = 0; i < areas.Count; i++) {
          areas[i].OnEvent(gameEvent);
        }
      }

      return gameEvent;
    }
  }
}
