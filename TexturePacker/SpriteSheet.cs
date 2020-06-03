namespace MountainMeadowEngine.TexturePacker {
  using System;
  using System.Collections.Generic;

  using Microsoft.Xna.Framework.Graphics;

  public class SpriteSheet {
    private string name;
    private readonly IDictionary<string, SpriteFrame> spriteList;

    public List<string> GetFrameNames(string name) {
      List<string> frameNames = new List<string>();

      int index = name.IndexOf("*", System.StringComparison.OrdinalIgnoreCase);
      if (index == -1) {
        index = name.Length;
      }

      bool getAll = (index == 0);

      foreach (var frameName in this.spriteList) {
        if (frameName.Key.Length >= index) {
          if (getAll || frameName.Key.Substring(0, index) == name.Substring(0, index)) {
            frameNames.Add(frameName.Key);
          }
        }
      }
      return frameNames;
    }

    public SpriteSheet(string name) {
      this.name = name;
      spriteList = new Dictionary<string, SpriteFrame>();
    }

    public string GetName() {
      return name;
    }

    public void Add(string name, SpriteFrame sprite) {
      spriteList.Add(name, sprite);
    }

    public void Add(SpriteSheet otherSheet) {
      foreach (var sprite in otherSheet.spriteList) {
        if (!spriteList.ContainsKey(sprite.Key))
          spriteList.Add(sprite);
      }
    }

    public SpriteFrame Sprite(string sprite) {
      return this.spriteList[sprite];
    }

  }
}