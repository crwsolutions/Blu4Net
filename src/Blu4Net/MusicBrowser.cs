﻿using Blu4Net.Channel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blu4Net
{
    public class MusicBrowser : MusicContentNode
    {

        private readonly BluChannel _channel;

        public MusicBrowser(BluChannel channel, BrowseContentResponse response)
            : base(channel, null, response)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public Task PlayURL(string playURL)
        {
            return _channel.PlayURL(playURL);
        }

    }
}
