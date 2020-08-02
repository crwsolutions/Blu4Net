﻿using Blu4Net.Channel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blu4Net
{
    public class PlayPosition
    {
        public TimeSpan Elapsed { get; private set; }
        public TimeSpan? Length { get; private set; }

        private PlayPosition()
        {
        }

        public static PlayPosition Create(StatusResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new PlayPosition()
            {
                Elapsed = TimeSpan.FromSeconds(response.Seconds),
                Length = response.TotalLength != 0 ? TimeSpan.FromSeconds(response.TotalLength) : default(TimeSpan?)
            };
        }

        public override string ToString()
        {
            return Length != null ? $"{Elapsed} - {Length}" : $"{Elapsed}";
        }
    }
}
