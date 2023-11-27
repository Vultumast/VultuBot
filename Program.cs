using System.Runtime;
using System.Collections;
using System.Collections.ObjectModel;
using DSharpPlus;
using System.Collections.Concurrent;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using DSharpPlus.CommandsNext.Converters;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Reflection.Metadata;
using DSharpPlus.Interactivity.Extensions;
using System.Data;
using System.Net.WebSockets;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using DSharpPlus.SlashCommands;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using System.Diagnostics.Metrics;
using System.Net.Mail;

namespace VultuBot
{
    internal class Program
    {



        const ulong ServerID = 1153122535299362876;
        const ulong MinecraftChannelID = 1174825407409815683;

#if DEBUG_DEV
        const ulong StarboardChannelID = 1178377434702286978;
#else
        const ulong StarboardChannelID = 1178379555774418994;
#endif

        static Starboard Starboard = new Starboard();

        public struct RoleIDs
        {
            public const ulong VC = 1170155106809946122;
            public const ulong Fortnite = 1171071868212629504;
            public const ulong Minecraft = 1176422456613945395;
            public const ulong Movie_Night = 1178426137114849422;

        }



        public static string Token;


        static DiscordClient discord = null!;
        static Process process = new Process();
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Token = File.ReadAllText("Token.txt");

            if (!Directory.Exists("Vultu"))
                Directory.CreateDirectory("Vultu");

            if (!Directory.Exists("Vultu/Dev"))
                Directory.CreateDirectory("Vultu/Dev");

            if (!Directory.Exists("Vultu/Release"))
                Directory.CreateDirectory("Vultu/Release");

            Starboard.Read();

            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents,
                ReconnectIndefinitely = true,
            });


            string[] prefixes = new string[] { "!" };
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = prefixes,
                //CaseSensitive = false,
                ////IgnoreExtraArguments = true,
                //EnableDms = true,
                EnableMentionPrefix = true,
            });
            var slash = discord.UseSlashCommands();

            discord.UseInteractivity();

            //RunExternalExe();
            //process.ErrorDataReceived += Process_OutputDataReceived;
            //commands.RegisterCommands<AdminCommands>();



            //To register them globally, once you're confident that they're ready to be used by everyone
            slash.RegisterCommands<SlashCommands>();

#if DEBUG___DEV
            commands.RegisterCommands<DebugCommands>();
#endif
            //commands.RegisterCommands<PlusShackCommands>();

            discord.Ready += Discord_Ready;
            discord.MessageCreated += Discord_MessageCreated;
            discord.ComponentInteractionCreated += Discord_ComponentInteractionCreated;
            discord.MessageReactionAdded += Discord_MessageReactionAdded;
            discord.MessageReactionRemoved += Discord_MessageReactionRemoved;
            discord.MessageReactionRemovedEmoji += Discord_MessageReactionRemovedEmoji;
            discord.MessageReactionsCleared += Discord_MessageReactionsCleared;

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }


        static void ProcessStarboardMessage(DiscordMessage message, DiscordEmoji emoji, DiscordGuild guild)
        {
            if (message.Author.IsBot)
                return;

            if (emoji.Id == 0 && emoji.GetDiscordName() == ":star:") // Is it Discord Star?
            {

                //Console.WriteLine("discord star detected!");
                var starboardID = Starboard.GetStarboardID(message.Id);
                var starboardChannel = guild.GetChannel(StarboardChannelID);

                if (message.Reactions.Count == 0)
                {
                    if (starboardID != 0)
                    {
                        starboardChannel.DeleteMessageAsync(starboardChannel.GetMessageAsync(Starboard.GetStarboardID(message.Id)).Result);
                        Starboard.Remove(message.Id);
                    }
                    return;
                }

                var reactions = message.Reactions.Where(x => emoji.Id == 0 && emoji.GetDiscordName() == ":star:").First();
#if DEBUG_DEV
                if (reactions.Count >= 1)
#else
                if (reactions.Count >= Starboard.StarboardThreshold)
#endif
                {
                    var imageURL = string.Empty;
                   
                    if (message.Attachments.Count > 0)
                    {
                        foreach (var attachment in message.Attachments)
                        {
                            if (attachment.MediaType.StartsWith("image/"))
                            {
                                Console.WriteLine($"{attachment.MediaType}");
                                imageURL = attachment.Url;
                                break;
                            }

                        }
                    }


                    
                    // Kind of a band-aid fix, but it will do for now.
                    if (message.Content.StartsWith("https://media.discordapp.net/attachments/") && message.Content.Contains(".gif?"))
                        imageURL = message.Content;


                    if (imageURL == string.Empty && message.Stickers.Count > 0)
                    {
                        if (message.Stickers.First().FormatType == StickerFormat.PNG)
                            imageURL = message.Stickers.First().StickerUrl;
                    }

                    var content = (message.Content.Length > 0 ? message.Content : string.Empty);


                    if (message.ReferencedMessage is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(message.ReferencedMessage.Content))
                            content += $"\n(in response to {message.ReferencedMessage.Content})";

                    }

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    {
                        Color = new DiscordColor(0xFF, 0xAC, 0x33),
                        Footer = new EmbedFooter()
                        {
                            IconUrl = "https://cdn.discordapp.com/emojis/1178395966076882975.webp?size=96&quality=lossless",
                            Text = $"Stars: {reactions.Count}",
                        },
                        Author = new EmbedAuthor()
                        {
                            IconUrl = message.Author.AvatarUrl,
                            Name = message.Author.Username
                        },
                        Description = $"{content}" +
                                      $"\n\n[Click Here to Jump To Message](https://discord.com/channels/{guild.Id}/{message.ChannelId}/{message.Id})",
                        ImageUrl = imageURL
                    };


                    if (starboardID == 0)
                    {
                        var id = starboardChannel.SendMessageAsync(builder.Build()).Result.Id;

                        Starboard.Add(message.Id, id);
                    }
                    else
                    {
                        starboardChannel.GetMessageAsync(Starboard.GetStarboardID(message.Id)).Result.ModifyAsync(builder.Build());
                    }
                }
                else
                {
                    if (starboardID != 0)
                    {
                        starboardChannel.DeleteMessageAsync(starboardChannel.GetMessageAsync(Starboard.GetStarboardID(message.Id)).Result);
                        Starboard.Remove(message.Id);
                    }

                }
            }
        }

        #region Reactions
        private static Task Discord_MessageReactionsCleared(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionsClearEventArgs e)
        {
            var starboardID = Starboard.GetStarboardID(e.Message.Id);
            var starboardChannel = e.Guild.GetChannel(StarboardChannelID);

            if (starboardID != 0)
            {
                starboardChannel.DeleteMessageAsync(starboardChannel.GetMessageAsync(Starboard.GetStarboardID(e.Message.Id)).Result);
                Starboard.Remove(e.Message.Id);
            }

            return Task.CompletedTask;
        }

        private static Task Discord_MessageReactionRemovedEmoji(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEmojiEventArgs e)
        {
            ProcessStarboardMessage(e.Channel.GetMessageAsync(e.Message.Id).Result, e.Emoji, e.Guild);
            return Task.CompletedTask;
        }

        private static Task Discord_MessageReactionRemoved(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            ProcessStarboardMessage(e.Channel.GetMessageAsync(e.Message.Id).Result, e.Emoji, e.Guild);
            return Task.CompletedTask;
        }

        private static Task Discord_MessageReactionAdded(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            var message = e.Channel.GetMessageAsync(e.Message.Id).Result;
#if DEBUG
            if ((e.Emoji.Id == 0 && e.Emoji.GetDiscordName() == ":star:") && (e.User.Id == message.Author.Id) && (e.User.Id != 150745989836308480)) // Disable Self starring
            {
                e.Message.DeleteReactionAsync(e.Emoji, e.User);
                return Task.CompletedTask;
            }
#endif   
                
            

            ProcessStarboardMessage(message, e.Emoji, e.Guild);
            return Task.CompletedTask;
        }

#endregion

        private static Task Discord_ComponentInteractionCreated(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            var memberAuthor = e.Guild.GetMemberAsync(e.User.Id).Result;
            switch (e.Interaction.Data.CustomId)
            {
                case "roleDropDown":
                    if (e.Interaction.Data.Values is not null)
                    {
                        if (e.Interaction.Data.Values.Contains("vc"))
                            memberAuthor.GrantRoleAsync(e.Guild.Roles[RoleIDs.VC]);
                        else
                            memberAuthor.RevokeRoleAsync(e.Guild.Roles[RoleIDs.VC]);

                        if (e.Interaction.Data.Values.Contains("fortnite"))
                            memberAuthor.GrantRoleAsync(e.Guild.Roles[RoleIDs.Fortnite]);
                        else
                            memberAuthor.RevokeRoleAsync(e.Guild.Roles[RoleIDs.Fortnite]);

                        if (e.Interaction.Data.Values.Contains("minecraft"))
                            memberAuthor.GrantRoleAsync(e.Guild.Roles[RoleIDs.Minecraft]);
                        else
                            memberAuthor.RevokeRoleAsync(e.Guild.Roles[RoleIDs.Minecraft]);
                        if (e.Interaction.Data.Values.Contains("movie night"))
                            memberAuthor.GrantRoleAsync(e.Guild.Roles[RoleIDs.Movie_Night]);
                        else
                            memberAuthor.RevokeRoleAsync(e.Guild.Roles[RoleIDs.Movie_Night]);
                    }
                    
                    e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    break;
            }
            return Task.CompletedTask;
        }

        static StreamWriter serverInput = null;

        public static async Task RunExternalExe()
        {
            process.StartInfo.FileName = "C:\\Program Files\\Java\\jre-1.8\\bin\\java.exe";
            process.StartInfo.Arguments = "-Xmx8096M -Xms8096M -jar server.jar nogui";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;

            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_OutputDataReceived;
            process.Start();
            serverInput = process.StandardInput;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        static DiscordClient client = null!;
        static DiscordGuild minecraftGuild = null!;
        static DiscordChannel minecraftChannel = null!;
        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
#if DEBUG_DEV

#else
            Console.WriteLine($"[MC SERVER] {e?.Data}");
            if (client is null)
                return;

            if (minecraftGuild is null)
                minecraftGuild = client.GetGuildAsync(ServerID).Result;
            if (minecraftChannel is null)
                minecraftChannel = minecraftGuild.GetChannel(MinecraftChannelID);

            if (e is not null && e?.Data is not null)
            {
                if (!e.Data.Contains("[CONSOLE]"))
                {
                    if (e.Data.Contains("<"))
                        minecraftChannel.SendMessageAsync($"{e.Data.Substring(e.Data.IndexOf('<'))}");
                    else if (e.Data.Contains("logged in"))
                    {
                        var offset = e.Data.IndexOf("[INFO]") + "INFO".Length + 2;
                        minecraftChannel.SendMessageAsync($"{e.Data.Substring(offset, (e.Data.LastIndexOf('[')  - 1) - offset)} has joined the game.");
                    }
                    else if (e.Data.Contains("lost connection"))
                    {
                        var offset = e.Data.IndexOf("[INFO]") + "INFO".Length + 2;
                        var message = $"{e.Data.Substring(offset, ((e.Data.IndexOf("lost connection") - 1) - offset))} has left the game.";

                        if (message.Trim().First() != '/' && !Regex.IsMatch(message.Trim(), @"((([0-9]{1,3})(\.|\s)){4})"))
                            minecraftChannel.SendMessageAsync(message);
                    }
                }
            }
#endif
        }

        private static Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs args)
        {
            if (serverInput is null)
                return Task.CompletedTask;
            if (args.Author.Id == sender.CurrentUser.Id)
                return Task.CompletedTask;
            if (args.Channel.Id != MinecraftChannelID)
                return Task.CompletedTask;

            var displayName = args.Message.Author.Username;

            if (args.Message.Stickers.Count != 0)
            {
                serverInput.WriteLine($"say {args.Message.Author.Username} sent a sticker: {args.Message.Stickers[0].Name}");
                serverInput.Flush();
            }
            if (!string.IsNullOrWhiteSpace(args.Message.Content))
            {
                serverInput.WriteLine($"say <{args.Message.Author.Username}> {args.Message.Content}");
                serverInput.Flush();
            }
            return Task.CompletedTask;
        }

        private static Task Discord_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            client = sender;
            RunExternalExe();
            Console.WriteLine("READY!");
            return Task.CompletedTask;
        }
    }
}