﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VultuBot
{
    public class Starboard
    {
        private Dictionary<ulong, ulong> _messages = new Dictionary<ulong, ulong>();

        public const uint StarboardThreshold = 3;

#if DEBUG_DEV
        public const string StarboardFilepath = "Vultu/Dev/Starboard.bin";
#else
        public const string StarboardFilepath = "Vultu/Release/Starboard.bin";
#endif


        public ulong GetStarboardID(ulong messageID)
        {
            if (!_messages.ContainsKey(messageID))
                return 0;
            else
                return _messages[messageID];
        }

        public void Add(ulong messageID, ulong starboardID)
        {
            _messages.Add(messageID, starboardID);
            Write();
        }

        public void Remove(ulong messageID)
        {
            _messages.Remove(messageID);
            Write();
        }

        
        public void Write()
        {
            using (FileStream fs = new FileStream(StarboardFilepath, FileMode.OpenOrCreate))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write((int)_messages.Count);
                    foreach (var message in _messages)
                    {
                        bw.Write((ulong)message.Key);
                        bw.Write((ulong)message.Value);

                    }
                    bw.Close();
                }
                fs.Close();
            }
        }

        public void Read()
        {
            if (!File.Exists(StarboardFilepath))
                return;

            using (FileStream fs = new FileStream(StarboardFilepath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    var count = br.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        _messages.Add(br.ReadUInt64(), br.ReadUInt64());
                    }
                    br.Close();
                }
                fs.Close();
            }
        }
    }
}