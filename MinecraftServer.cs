using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VultuBot
{
    public static class MinecraftServer
    {
        public static StreamWriter ServerInput = null!;

        public static DiscordClient DiscordClient = null!;
        public static DiscordGuild MinecraftGuild = null!;
        public static DiscordChannel MinecraftChannel = null!;

        public static Process ServerProcess = new Process();

        public static Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs args)
        {
            if (ServerInput is null)
                return Task.CompletedTask;
            if (args.Author.Id == sender.CurrentUser.Id)
                return Task.CompletedTask;
            if (args.Channel.Id != Program.MinecraftChannelID)
                return Task.CompletedTask;

            var displayName = args.Message.Author.Username;

            if (args.Message.Stickers.Count != 0)
            {
                ServerInput.WriteLine($"say {args.Message.Author.Username} sent a sticker: {args.Message.Stickers[0].Name}");
                ServerInput.Flush();
            }
            if (!string.IsNullOrWhiteSpace(args.Message.Content))
            {
                //serverInput.WriteLine($"say <{args.Message.Author.Username}> {args.Message.Content}");
                ServerInput.WriteLine($"/tellraw @a [\"\",{{\"text\":\"<{args.Message.Author.Username}> {args.Message.Content}\",\"color\":\"gray\"}},\"\",\"\"]");
                ServerInput.Flush();
            }
            return Task.CompletedTask;
        }
        
        public static void Init()
        {
            ServerProcess.OutputDataReceived += Process_OutputDataReceived;
            ServerProcess.ErrorDataReceived += Process_OutputDataReceived;
        }

        public static void Run(string fileName, string arguments)
        {
            //ServerProcess.StartInfo.FileName = "C:\\Program Files\\Java\\jdk-17\\bin\\java.exe";
            //ServerProcess.StartInfo.Arguments = "-Xmx8096M -Xms8096M -jar server.jar nogui";
            ServerProcess.StartInfo.FileName = fileName;
            ServerProcess.StartInfo.Arguments = arguments;
            ServerProcess.StartInfo.UseShellExecute = false;
            ServerProcess.StartInfo.RedirectStandardOutput = true;
            ServerProcess.EnableRaisingEvents = true;
            ServerProcess.StartInfo.RedirectStandardError = true;
            ServerProcess.StartInfo.RedirectStandardInput = true;

            ServerProcess.Start();
            ServerInput = ServerProcess.StandardInput;
            ServerProcess.BeginOutputReadLine();
            ServerProcess.BeginErrorReadLine();
        }

        public static void Stop()
        {
            ServerInput.WriteLine("/stop");
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
#if DEBUG_DEV

#else
            Console.WriteLine($"[MC SERVER] {e?.Data}");
            if (DiscordClient is null)
                return;

            if (MinecraftGuild is null)
                MinecraftGuild = Program.DiscordClient.GetGuildAsync(Program.ServerID).Result;
            if (MinecraftChannel is null)
                MinecraftChannel = MinecraftGuild.GetChannel(Program.MinecraftChannelID);

            if (e is not null && e?.Data is not null)
            {
                if (!e.Data.Contains("[CONSOLE]"))
                {
                    if (e.Data.Contains("[INFO] <"))
                        MinecraftChannel.SendMessageAsync($"{e.Data.Substring(e.Data.IndexOf('<'))}");
                    else if (e.Data.Contains("logged in"))
                    {
                        var offset = e.Data.IndexOf("[INFO]") + "INFO".Length + 2;
                        MinecraftChannel.SendMessageAsync($"{e.Data.Substring(offset, (e.Data.LastIndexOf('[') - 1) - offset)} has joined the game.");
                    }
                    else if (e.Data.Contains("lost connection"))
                    {
                        var offset = e.Data.IndexOf("[INFO]") + "INFO".Length + 2;
                        var message = $"{e.Data.Substring(offset, ((e.Data.IndexOf("lost connection") - 1) - offset))} has left the game.";

                        if (message.Trim().First() != '/' && !Regex.IsMatch(message.Trim(), @"((([0-9]{1,3})(\.|\s)){4})"))
                            MinecraftChannel.SendMessageAsync(message);
                    }
                }
            }
#endif
        }

    }
}
