﻿using Blu4Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BluDumper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var endpoint = await BluEnvironment.ResolveEndpoints().FirstOrDefaultAsync();
            if (endpoint != null)
            {
                Console.WriteLine($"Endpoint: {endpoint}");

                var player = await BluPlayer.Connect(endpoint);
                //player.Log = Console.Out;
                
                Console.WriteLine($"Player: {player.Name}");
                Console.WriteLine(new string('=', 80));

                await DumpPlayer(player);

                while (true)
                {
                    var key = Console.ReadKey(true);

                    if (key.KeyChar == 'q')
                        break;

                    if (key.KeyChar == 'p')
                    {
                        await DumpQueuedSongs(player.PlayQueue);
                    }

                    if (key.KeyChar == 'l')
                    {
                        var entry = player.MusicBrowser.Entries.SingleOrDefault(element => element.Name == "Library");
                        var node = await entry.Resolve();
                        await DumpMusicContentNode(node, 3);
                    }

                    if (key.KeyChar == 's')
                    {
                        Console.Write("Enter search term: ");
                        var term = Console.ReadLine();

                        var entry = player.MusicBrowser.Entries.SingleOrDefault(element => element.Name == "Library");
                        var node = await entry.Resolve();
                        var search = await node.Search(term);
                        await DumpMusicContentNode(search, 3);
                    }

                    if (Char.IsDigit(key.KeyChar))
                    {
                        var number = (int)Char.GetNumericValue(key.KeyChar);
                        if (number != 0)
                        {
                            await player.PresetList.LoadPreset(number);
                        }
                    }

                    WriteInstructions();
                }
            }
            else 
            {
                Console.WriteLine("No player found!");
                Console.WriteLine("Press 'q' to quit");
                while (Console.ReadKey().KeyChar != 'q') { }
            }
        }

        private static void WriteInstructions()
        {
            Console.WriteLine();
            Console.WriteLine($"Press 'q' to quit");
            Console.WriteLine($"Press 'p' to dump the PlayQueue");
            Console.WriteLine($"Press 'l' to dump the Library (3 levels)");
            Console.WriteLine($"Press 's' to search the library");
            Console.WriteLine($"Press '1..9' to load a Preset");
        }

        private static async Task DumpPlayer(BluPlayer player)
        {
            Console.WriteLine($"State: {await player.GetState()}");
            Console.WriteLine($"Shuffle: {await player.GetShuffleMode()}");
            Console.WriteLine($"Repeat: {await player.GetRepeatMode()}");
            Console.WriteLine($"Volume: {await player.GetVolume()}");
            Console.WriteLine($"Position: {await player.GetPosition()}");

            DumpPresets(await player.PresetList.GetPresets());
            DumpMedia(await player.GetMedia());
            DumpQueueInfo(await player.PlayQueue.GetInfo());
            await DumpMusicBrowser(player.MusicBrowser);

            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            Console.WriteLine("Waiting for changes...");
            WriteInstructions();

            player.StateChanges.Subscribe(state =>
            {
                Console.WriteLine($"State: {state}");
            });

            player.ShuffleModeChanges.Subscribe(mode =>
            {
                Console.WriteLine($"Shuffle: {mode}");
            });

            player.RepeatModeChanges.Subscribe(mode =>
            {
                Console.WriteLine($"Repeat: {mode}");
            });

            player.VolumeChanges.Subscribe(volume =>
            {
                Console.WriteLine($"Volume: {volume}");
            });

            player.PositionChanges.Subscribe(position =>
            {
                Console.WriteLine($"Position: {position}");
            });

            player.MediaChanges.Subscribe(media =>
            {
                DumpMedia(media);
            });

            player.PresetList.Changes.Subscribe(presets =>
            {
                DumpPresets(presets);
            });

            player.PlayQueue.Changes.Subscribe(info =>
            {
                DumpQueueInfo(info);
            });

        }

        private static void DumpMedia(PlayerMedia media)
        {
            Console.WriteLine($"Media:");
            for (var i = 0; i < media.Titles.Count; i++)
            {
                Console.WriteLine($"\tTitle{i + 1}: {media.Titles[i]}");
            }
            Console.WriteLine($"\tImageUri: {media.ImageUri}");
            Console.WriteLine($"\tServiceIconUri: {media.ServiceIconUri}");
        }

        private static void DumpPresets(IReadOnlyCollection<PlayerPreset> presets)
        {
            Console.WriteLine($"Presets:");
            foreach (var preset in presets)
            {
                Console.WriteLine($"\tNumber: {preset.Number}");
                Console.WriteLine($"\tName: {preset.Name}");
                Console.WriteLine($"\tImageUri: {preset.ImageUri}");
                Console.WriteLine();
            }
        }

        private static void DumpQueueInfo(PlayQueueInfo info)
        {
            Console.WriteLine($"Queue:");
            Console.WriteLine($"\tName: {info.Name}");
            Console.WriteLine($"\tLength: {info.Length}");
        }

        private static async Task DumpQueuedSongs(PlayQueue queue)
        {
            Console.WriteLine($"\tSongs:");
            await foreach (var page in queue.GetSongs(500))
            {
                foreach (var song in page)
                {
                    Console.WriteLine($"\t\t{song}");
                }
            }
            Console.WriteLine($"Done.");
        }


        private static async Task DumpMusicBrowser(MusicBrowser browser)
        {
            Console.WriteLine($"Sources:");
            await DumpMusicContentNode(browser, 2);
        }

        private static async Task DumpMusicContentNode(MusicContentNode node, int maxLevels, int level = 0)
        {
            await foreach (var page in GetAllEntries(node))
            {
                foreach (var entry in page)
                {
                    Console.WriteLine($"{new string('\t', level + 1)}{entry}");

                    if (entry.IsResolvable && level < maxLevels - 1)
                    {
                        var child = await entry.Resolve();
                        await DumpMusicContentNode(child, maxLevels, level + 1);
                    }
                }
            }
        }

        private static async IAsyncEnumerable<IReadOnlyCollection<MusicContentEntry>> GetAllEntries(MusicContentNode node)
        {
            yield return node.Entries;

            var current = node;
            while (current.HasNext)
            {
                current = await current.ResolveNext();
                yield return current.Entries;
            }
        }
    }
}
