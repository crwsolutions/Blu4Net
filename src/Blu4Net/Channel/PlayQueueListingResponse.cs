﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Blu4Net.Channel
{
    [XmlRoot("playlist")]
    public class PlayQueueListingResponse
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("length")]
        public int Length;

        [XmlElement("song")]
        public PlayQueueSong[] Songs = new PlayQueueSong[0];

        public override string ToString()
        {
            return Name;
        }
    }

    [XmlRoot("playlist")]
    public class PlayQueueSong
    {
        [XmlElement("title")]
        public string Title;

        [XmlElement("art")]
        public string Artist;

        [XmlElement("alb")]
        public string Album;

        public override string ToString()
        {
            return Title;
        }
    }
}