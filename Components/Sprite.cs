using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Managers;
using MountainMeadowEngine.Objects;
using static MountainMeadowEngine.Components.MovableEntity;

namespace MountainMeadowEngine.Components {
  
  public class Sprite : GameObjectComponent {
    string name, spriteName, spriteSheetFile, currentSprite;
    bool flipSprite = false;

    Dictionary<Direction, string> spriteNames = new Dictionary<Direction, string>();
    List<Direction> availableDirections = new List<Direction>();
    List<Direction> flippedDirections = new List<Direction>();
    ObjectSprite imageFrame = new ObjectSprite();
    Vector2 offset = new Vector2();
    float scale = 1;

    public Sprite(GameObject context) : base(context) {
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.OBJECT_RENDERING);
    }


    public Texture2D GetTexture() {
      if (IsSpriteSheet()) {
        if (imageFrame.texture != null || currentSprite != null)
          return imageFrame.texture;

        imageFrame.SetSprite(GameContentManager.LoadSpriteSheet(spriteSheetFile).Sprite(spriteName), Color.White);
        return imageFrame.texture;
      }
      return GameContentManager.LoadTexture(currentSprite);
    }

    public Rectangle? GetSourceRectangle() {
      if (IsSpriteSheet()) {
        if (imageFrame.texture != null || currentSprite != null)
          return imageFrame.sourceRectangle;

        imageFrame.SetSprite(GameContentManager.LoadSpriteSheet(spriteSheetFile).Sprite(spriteName), Color.White);
        return imageFrame.sourceRectangle;
      }
      return null;
    }

    public Color GetColor() {
      if (IsSpriteSheet()) {
        return imageFrame.color;
      }
      return Color.White;
    }

    public float GetRotation() {
      if (IsSpriteSheet()) {
        return imageFrame.rotation;
      }
      return 0;
    }

    public Vector2 GetOffset() {
      return offset;
    }

    public Vector2 GetOrigin() {
      if (IsSpriteSheet()) {
        return imageFrame.origin;
      }
      return Vector2.Zero;
    }

    public float GetScale() {
      return scale;
    }

    public Sprite SetScale(float scale) {
      this.scale = scale;
      return this;
    }

    public SpriteEffects GetSpriteEffects() {
      if (IsSpriteSheet()) {
        return imageFrame.spriteEffects;
      }
      return (!flipSprite) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    }

    public Sprite SetSprite(Direction direction, string spriteName, bool flipped = false) {
      if (this.spriteName == null) {
        this.spriteName = spriteName;
        this.currentSprite = spriteName;
      }
      if (!spriteNames.ContainsKey(direction)) {
        spriteNames.Add(direction, spriteName);
        availableDirections.Add(direction);
        if (flipped && !flippedDirections.Contains(direction)) {
          flippedDirections.Add(direction);
        }
      }
      return this;
    }

    public List<Direction> GetFlippedDirections() {
      return flippedDirections;  
    }

    public bool FlipSprite() {
      return flipSprite;  
    }

    public Sprite SetName(string name) {
      this.name = name;
      return this;
    }

    public string GetName() {
      return this.name;
    }

    public Sprite SetSprite(string spriteName) {
      this.spriteName = spriteName;
      this.currentSprite = spriteName;
      return this;
    }

    public Sprite SetOffset(float x, float y) {
      this.offset.X = x;
      this.offset.Y = y;
      return this;
    }

    public Sprite SetSpriteSheetFile(string file) {
      spriteSheetFile = file;
      return this;
    }

    public string GetCurrentSpriteName() {
      return currentSprite;
    }

    public string GetSpriteSheetFile() {
      return spriteSheetFile;
    }

    public bool IsSpriteSheet() {
      return (spriteSheetFile != null);
    }

    public override void Initialize() { }
       
    protected virtual void DrawUpdate() {
      if (currentSprite == null) {
        currentSprite = spriteName;
      }

      if (spriteNames.Count > 0) {
        Direction direction = Direction.NONE;

        if (context.GetMovableEntity() != null) {
          direction = MovableEntity.GetNearestDirection(context.GetMovableEntity().GetCurrentDirection(), availableDirections);
        }
        if (spriteNames.ContainsKey(direction)) {
          currentSprite = spriteNames[direction];
          flipSprite = (flippedDirections.Contains(direction));
        }
      }

      if (IsSpriteSheet()) {
        if (!flipSprite) {
          imageFrame.SetSprite(GameContentManager.LoadSpriteSheet(spriteSheetFile).Sprite(currentSprite), Color.White, 0, scale);
        } else {
          imageFrame.SetSprite(GameContentManager.LoadSpriteSheet(spriteSheetFile).Sprite(currentSprite), Color.White, 0, scale, SpriteEffects.FlipHorizontally);
        }
      }
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      DrawUpdate();
      return gameEvent;
    }
  }
}
