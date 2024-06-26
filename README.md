![](https://github.com/roblans/Blu4Net/blob/master/assets/BluMiniPlayer.PNG)

*[BluMiniPlayer](https://github.com/roblans/Blu4Net/tree/master/samples/BluMiniPlayer)* sample app


# Blu4Net
Blu4Net is a .NET library that interfaces with BluOS players (Bluesound, NAD). It uses an event-driven (RX), non-blocking (async/await) model that makes it lightweight and efficient.

Supported Targets:

- .NET Standard: 2.0
- .NET Standard: 2.1

Requires C# 8.0 (due to the use of IAsyncEnumerable\<T\>)

![](https://dev.azure.com/roblans/Blu4Net/_apis/build/status/Blu4Net%20Release?branchName=master) ![](https://img.shields.io/nuget/v/blu4net.svg)

## Features

### Discovery

Player endpoint discovery (mDNS)

### Basic operations

- Play
- Seek (offset)
- Pause
- Stop
- Skip
- Back
- Shuffle (on | off)
- Repeat (off | one | all)
- Volume (percentage)
- Position (elapsed time + length of current track) 

### Media information

- Titles
- Image (artwork)
- Service icon

### Presets

- Fetch presets
- Load preset (by number)

### Queue

- Info (name + length)
- Fetch songs (paged)
- Clear
- Remove (song)
- Save (name)

### Music sources (Library, TuneIn, ...)

- Browse hierarchy (note: partially implemented)
- Search (full-text)

### Notifications (Observable - ReactiveX)
- State changes (playing, paused, streaming, ...)
- Mode changes (shuffle, repeat)
- Volume changes
- Position changes (note: not realtime)
- Media changes
- Preset changes
- Queue changes

## Installation

Install the NuGet package.

> Install-Package [Blu4Net](http://www.nuget.org/packages/blu4net)

## Getting started

```cs
using Blu4Net;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GettingStarted
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // find endpoint of player (first one)
            var endpoint = await BluEnvironment.ResolveEndpoints().FirstOrDefaultAsync();
            Console.WriteLine($"Endpoint: {endpoint}");

            // success?
            if (endpoint != null)
            {
                // yes, so create and connect the player
                var player = await BluPlayer.Connect(endpoint);
                Console.WriteLine($"Player: {player}");

                // get the state
                var state = await player.GetState();
                Console.WriteLine($"State: {state}");

                // get the volume
                var volume = await player.GetVolume();
                Console.WriteLine($"Volume: {volume}");

                // get the current playing media
                var media = await player.GetMedia();
                Console.WriteLine($"Media: {media.Titles.FirstOrDefault()}");

                // subscribe to volume changes
                using (player.VolumeChanges.Subscribe(volume => Console.WriteLine($"Volume: {volume}")))
                {
                    Console.WriteLine();
                    Console.WriteLine($"Waiting for volume changes...");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("No endpoint found!");
                Console.ReadLine();
            }
        }
    }
}
```
See the *[BluDumper](https://github.com/roblans/Blu4Net/tree/master/samples/BluDumper)* sample app for a complete example
