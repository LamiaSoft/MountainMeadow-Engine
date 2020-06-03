using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using static MountainMeadowEngine.GameWorld;

namespace MountainMeadowEngine.Tools {
  public static class Debug {


    public static void Output(String str, DebugModes mode = DebugModes.NOTICE) {
      Output("", str, mode);
    }

    public static void Output(bool str, DebugModes mode = DebugModes.NOTICE) {
      Output("", str, mode);
    }

    public static void Output(Vector2 str, DebugModes mode = DebugModes.NOTICE) {
      Output("", str, mode);
    }

    public static void Output(Vector3 str, DebugModes mode = DebugModes.NOTICE) {
      Output("", str, mode);
    }

    public static void Output<T>(List<T> str, DebugModes mode = DebugModes.NOTICE) {
      Output("", str, mode);
    }

    public static void Output<T, T2>(Dictionary<T, T2> str, DebugModes mode = DebugModes.NOTICE) {
      Output("", str, mode);
    }

    public static void Json(object str, DebugModes mode = DebugModes.NOTICE) {
      Json("", str, mode);
    }


    public static void Output(String tag, String str, DebugModes mode = DebugModes.NOTICE) {
      switch (GameWorld.DEBUG_MODE) {
        case DebugModes.CRITICAL:
          if (mode != DebugModes.CRITICAL) {
            return;
          }
          break;
        case DebugModes.WARNING:
          if (mode != DebugModes.WARNING && mode != DebugModes.CRITICAL) {
            return;
          }
          break;
        case DebugModes.NOTICE:
          if (mode == DebugModes.NONE) {
            return;
          }
          break;
        case DebugModes.NONE:
          if (mode != DebugModes.NONE) {
            return;
          }
          break;
      }

      tag = (tag.Length > 0) ? "[" + tag + ":" + mode.ToString() + "] " : "[" + mode.ToString() + "] ";
      Console.WriteLine(tag + str);
    }

    public static void Output(String tag, bool str, DebugModes mode = DebugModes.NOTICE) {
      Output(tag, str.ToString(), mode);
    }

    public static void Output(string tag, Vector2 str, DebugModes mode = DebugModes.NOTICE) {
      Output(tag, "X: " + str.X + ", Y: " + str.Y, mode);
    }

    public static void Output(string tag, Vector3 str, DebugModes mode = DebugModes.NOTICE) {
      Output(tag, "X: " + str.X + ", Y: " + str.Y + ", Z: " + str.Z, mode);
    }

    public static void Output<T>(String tag, List<T> str, DebugModes mode = DebugModes.NOTICE) {
      string output = "";
      if (str.Count > 0) {
        foreach (T val in str) {
          output += val.ToString() + ", ";
        }
        output = output.Substring(0, output.Length - 2);
      }
      Output(tag, output, mode);
    }

    public static void Output<T, T2>(String tag, Dictionary<T, T2> str, DebugModes mode = DebugModes.NOTICE) {
      string output = "";
      if (str.Count > 0) {
        foreach (KeyValuePair<T, T2> entry in str) {
          output += entry.Key + ": " + entry.Value + ", ";
        }
        output = output.Substring(0, output.Length - 2);
      }
      Output(tag, output, mode);
    }

    public static void Json(String tag, object str, DebugModes mode = DebugModes.NOTICE) {
     // Output(tag, Newtonsoft.Json.JsonConvert.SerializeObject(str), mode);
    }

  }
}
