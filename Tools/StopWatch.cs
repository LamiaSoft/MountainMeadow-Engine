using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MountainMeadowEngine.Tools {
  
  public static class StopWatch {
    public static Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

    public static void Start(string name) {
      if (!stopwatches.ContainsKey(name)) {
        stopwatches.Add(name, new Stopwatch());
      } else {
        stopwatches[name].Reset();
      }
      stopwatches[name].Start();
    }

    public static void Stop(string name) {
      if (!stopwatches.ContainsKey(name)) {
        stopwatches.Add(name, new Stopwatch());
      } else {
        stopwatches[name].Stop();
      }
    }

    public static void DisplayMilliseconds(string name) {
      if (!stopwatches.ContainsKey(name)) {
        stopwatches.Add(name, new Stopwatch());
      } else {
        Tools.Debug.Output(name + " ELAPSED MILLISECONDS: ", stopwatches[name].ElapsedMilliseconds.ToString());
      }
    }

    public static void DisplayTicks(string name) {
      if (!stopwatches.ContainsKey(name)) {
        stopwatches.Add(name, new Stopwatch());
      } else {
        Tools.Debug.Output(name + " ELAPSED TICKS: ", stopwatches[name].ElapsedTicks.ToString());
      }
    }

  }
}
