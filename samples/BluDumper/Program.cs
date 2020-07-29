﻿using Blu4Net;
using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace BluDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            using (BluEnvironment.ResolveEndpoints()
                .Select(async endPoint => await DumpEndpoint(endPoint))
                .Subscribe())
            {
                Console.ReadLine();
            }
        }


        private static async Task DumpEndpoint(Uri endpoint)
        {
            Console.WriteLine($"Endpoint: {endpoint}");

            var player = await BluPlayer.Connect(endpoint);
            DumpPlayer(player);
        }

        private static void DumpPlayer(BluPlayer player)
        {
            Console.WriteLine($"Player: {player.Name}");
            Console.WriteLine(new string('=', 80));

            Console.WriteLine($"State: {player.State}");
            Console.WriteLine($"Mode: {player.Mode}");
            Console.WriteLine($"Volume: {player.Volume}%");
            
            DumpMedia(player.Media);

            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            player.StateChanges.Subscribe(state =>
            {
                Console.WriteLine($"State: {state}");
            });

            player.ModeChanges.Subscribe(mode =>
            {
                Console.WriteLine($"Mode: {mode}");
            });

            player.VolumeChanges.Subscribe(volume =>
            {
                Console.WriteLine($"Volume: {volume}%");
            });

            player.MediaChanges.Subscribe(media =>
            {
                DumpMedia(media);
            });
        }

        private static void DumpMedia(PlayerMedia media)
        {
            Console.WriteLine($"Media:");
            for (var i = 0; i < media.Titles.Length; i++)
            {
                Console.WriteLine($"\tTitle{i + 1}: {media.Titles[i]}");
            }
            Console.WriteLine($"\tImageUri: {media.ImageUri}");
            Console.WriteLine($"\tServiceIconUri: {media.ServiceIconUri}");
        }
    }
}