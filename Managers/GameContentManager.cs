
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.TexturePacker;

namespace MountainMeadowEngine.Managers {

  public class GameContentManager : IDisposable {
    public enum ContentType { TEXTURE, SOUND, SONG, FONT, SPRITESHEET }
    static GameWorld gameWorld;

    static Dictionary<string, Microsoft.Xna.Framework.Content.ContentManager> spriteSheetManagers = new Dictionary<string, Microsoft.Xna.Framework.Content.ContentManager>();
    static Dictionary<string, SpriteSheet> spriteSheets = new Dictionary<string, SpriteSheet>();

    private static Microsoft.Xna.Framework.Content.ContentManager fontManager;
    static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

    private static Microsoft.Xna.Framework.Content.ContentManager songManager;
    static Dictionary<string, Song> songs = new Dictionary<string, Song>();

    private static Microsoft.Xna.Framework.Content.ContentManager soundManager;
    static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

    private static Microsoft.Xna.Framework.Content.ContentManager textureManager;
    static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();


    static void DisposeAll<T>(Dictionary<string, T> disposables) {
      foreach (T item in disposables.Values) {
        ((IDisposable)item).Dispose();
      }
      disposables.Clear();
    }

    public static void RemoveSpriteSheet(string file) {
      if (spriteSheets.ContainsKey(file)) {
        spriteSheetManagers[file].Dispose();
        spriteSheetManagers.Remove(file);
        spriteSheets.Remove(file);
      }
    }

    public static void Reload(ContentType contentType) {
      switch (contentType) {
        case ContentType.TEXTURE:
          if (textureManager != null) {
            textureManager.Dispose();
            textureManager = null;
          }
          textureManager = new Microsoft.Xna.Framework.Content.ContentManager(gameWorld.Services) {
            RootDirectory = "Content"
          };
          DisposeAll(textures);
          break;
        case ContentType.SOUND:
          if (soundManager != null) {
            soundManager.Dispose();
            soundManager = null;
          }
          soundManager = new Microsoft.Xna.Framework.Content.ContentManager(gameWorld.Services) {
            RootDirectory = "Content"
          };
          DisposeAll(sounds);
          break;
        case ContentType.SONG:
          if (songManager != null) {
            songManager.Dispose();
            songManager = null;
          }
          songManager = new Microsoft.Xna.Framework.Content.ContentManager(gameWorld.Services) {
            RootDirectory = "Content"
          };
          DisposeAll(songs);
          break;
        case ContentType.FONT:
          if (fontManager != null) {
            fontManager.Dispose();
            fontManager = null;
          }
          fontManager = new Microsoft.Xna.Framework.Content.ContentManager(gameWorld.Services) {
            RootDirectory = "Content"
          };
          DisposeAll(fonts);
          break;
        case ContentType.SPRITESHEET:
          DisposeAll(spriteSheetManagers);
          spriteSheets.Clear();
          break;
      }
    }

    public void Initialize(GameWorld gameWorld) {
      GameContentManager.gameWorld = gameWorld;
      Reload(ContentType.FONT);
      Reload(ContentType.SONG);
      Reload(ContentType.SOUND);
      Reload(ContentType.TEXTURE);
    }

    public static SpriteFont LoadFont(string file) {
      if (!fonts.ContainsKey(file)) {
        fonts.Add(file, fontManager.Load<SpriteFont>("Fonts/" + file));
      }
      return fonts[file];
    }

    public static Song LoadSong(string file) {
      if (!songs.ContainsKey(file)) {
        songs.Add(file, songManager.Load<Song>("Songs/" + file));
      }
      return songs[file];
    }

    public static SoundEffect LoadSound(string file) {
      if (!sounds.ContainsKey(file)) {
        sounds.Add(file, soundManager.Load<SoundEffect>("Sounds/" + file));
      }
      return sounds[file];
    }

    public static Texture2D LoadTexture(string file) {
      if (!textures.ContainsKey(file)) {
                textures.Add(file, textureManager.Load<Texture2D>("Textures/" + file));
      }
      return textures[file];
    }

    public static SpriteSheet LoadSpriteSheet(string file) {
      if (!spriteSheets.ContainsKey(file)) {
        spriteSheetManagers.Add(file, new Microsoft.Xna.Framework.Content.ContentManager(gameWorld.Services) {
          RootDirectory = "Content"
        });

        SpriteSheetLoader loader = new SpriteSheetLoader(spriteSheetManagers[file]);
        spriteSheets.Add(file, loader.MultiLoad("SpriteSheets/" + file, 999));
      }
      return spriteSheets[file];
    }

    public void Dispose() {
      DisposeAll(textures);
      DisposeAll(sounds);
      DisposeAll(songs);
      DisposeAll(fonts);
      DisposeAll(spriteSheets);
      DisposeAll(spriteSheetManagers);

      textureManager.Dispose();
      textureManager = null;
      soundManager.Dispose();
      soundManager = null;
      songManager.Dispose();
      songManager = null;
      fontManager.Dispose();
      fontManager = null;
    }

  }
}
