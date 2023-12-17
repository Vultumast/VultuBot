using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VultuBot.Program;

namespace VultuBot.Modules
{
    public class AssignableRoles
    {
        public class AssignableRole
        {
            public string RoleIdentifer = "";
            public ulong ID = 0;
        }

#if DEBUG_DEV
        public const string Filepath = "Vultu/Dev/AssignableRoles.bin";
#else
        public const string Filepath = "Vultu/Release/AssignableRoles.bin";
#endif

        public static Dictionary<ulong, string> Roles = new Dictionary<ulong, string>();

        public static void Write()
        {
            using (FileStream fs = new FileStream(Filepath, FileMode.OpenOrCreate))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(Roles.Count);
                    foreach (var role in Roles)
                    {
                        bw.Write(role.Key);
                        bw.Write(role.Value);
                    }
                    bw.Close();
                }
                fs.Close();
            }
        }
        public static void Read()
        {
            if (!File.Exists(Filepath))
                return;

            using (FileStream fs = new FileStream(Filepath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    var count = br.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        var key = br.ReadUInt64();
                        var value = br.ReadString();
                        Roles.Add(key, value);
                    }

                    br.Close();
                }
                fs.Close();
            }
        }
    }
}
