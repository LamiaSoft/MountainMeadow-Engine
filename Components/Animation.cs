using System;
using System.Collections.Generic;
using MountainMeadowEngine.Managers;
using MountainMeadowEngine.TexturePacker;
using static MountainMeadowEngine.Components.MovableEntity;

namespace MountainMeadowEngine.Components {
  
  public abstract class Animation {

    public enum SequenceTypes { REGULAR, REGULAR_CYCLE, REGULAR_REPEAT, REGULAR_REPEAT_CYCLE, REVERSE, REVERSE_CYCLE, REVERSE_REPEAT, REVERSE_REPEAT_CYCLE };

    Dictionary<Direction, List<string>> frames = new Dictionary<Direction, List<string>>();
    Dictionary<Direction, Dictionary<int, List<CollisionArea>>> collisionAreas = new Dictionary<Direction, Dictionary<int, List<CollisionArea>>>();
    SpriteSheet spriteSheet;
    string spriteSheetFile;
    SequenceTypes sequenceType = SequenceTypes.REGULAR;
    float scale = 1;
    List<Direction> directionFlips = new List<Direction>();
    List<Direction> availableDirections = new List<Direction>();
    double duration;

    List<CollisionArea> _emptyList = new List<CollisionArea>();

    public Animation(GameObject context) { }

    protected void AddCollisionArea(Direction direction, CollisionArea collisionArea, params int[] indices) {
      List<int> newIndices = new List<int>();
      for (int i = 0; i < ((indices.Length > 0) ? indices.Length : frames[direction].Count); i++) {
        newIndices.Add(((indices.Length > 0) ? indices[i] : i));
      }
                                    
      if (!collisionAreas.ContainsKey(direction)) {
        collisionAreas.Add(direction, new Dictionary<int, List<CollisionArea>>());
      }

      foreach (int index in newIndices) {
        if (!collisionAreas[direction].ContainsKey(index)) {
          collisionAreas[direction].Add(index, new List<CollisionArea>());
        }
        collisionAreas[direction][index].Add(collisionArea);
      }
    }

    protected void SetDuration(double duration) {
      this.duration = duration;
    }

    protected void SetSequenceType(SequenceTypes sequenceType) {
      this.sequenceType = sequenceType;
    }

    public SequenceTypes GetSequenceType() {
      return sequenceType;
    }

    public Dictionary<Direction, List<string>> GetAnimationFrames() {
      return frames;
    }

    public string GetAnimationFrame(Direction direction, int index) {
      return frames[direction][index];
    }

    public List<CollisionArea> GetFrameCollisionAreas(Direction direction, int index) {
      if (collisionAreas.ContainsKey(direction) && collisionAreas[direction].ContainsKey(index)) {
        return collisionAreas[direction][index];
      }
      return _emptyList;
    }

    public List<CollisionArea> GetCollisionAreas() {
      List<CollisionArea> areas = new List<CollisionArea>();
      foreach (Direction dir in collisionAreas.Keys) {
        foreach (int index in collisionAreas[dir].Keys) {
          foreach (CollisionArea ca in collisionAreas[dir][index]) {
            areas.Add(ca);
          }
        }
      }
      return areas;
    }

    public string GetSpriteSheetFile() {
      return spriteSheetFile;
    }

    protected void SetSpriteSheetFile(string spriteSheetFile) {
      this.spriteSheet = GameContentManager.LoadSpriteSheet(spriteSheetFile);
      this.spriteSheetFile = spriteSheetFile;
    }

    public List<Direction> GetDirectionFlips() {
      return directionFlips;
    }

    public List<Direction> GetAvailableDirections() {
      return availableDirections;
    }

    public double GetDuration() {
      return duration;
    }

    public float GetScale() {
      return scale;  
    }

    // Should not get spriteframe here
    //public SpriteFrame GetSpriteFrame(Direction direction, int frameNumber) {
    //  return spriteSheet.Sprite(frames[direction][frameNumber]);
    //}

    protected void SetAnimationFrames(Direction direction, string animationName, float spriteScale = 1, bool flipHorizontally = false) {
      frames[direction] = spriteSheet.GetFrameNames(animationName);

      if (frames[direction].Count == 0)
        throw new ArgumentException("No animation found for: \"" + animationName + "\"");

      this.scale = spriteScale;
      if (flipHorizontally && !directionFlips.Contains(direction)) {
        directionFlips.Add(direction);
      }
      if (!availableDirections.Contains(direction)) {
        availableDirections.Add(direction);
      }
    }



  }
}
