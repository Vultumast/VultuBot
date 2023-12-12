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
using VultuBot.Commands;

namespace VultuBot
{
    internal class Program
    {



        public const ulong ServerID = 1153122535299362876;
        public const ulong MinecraftChannelID = 1174825407409815683;

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


        public static DiscordClient DiscordClient = null!;
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

            DiscordClient = new DiscordClient(new DiscordConfiguration()
            {
                Token = Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents,
                ReconnectIndefinitely = true,
            });


            string[] prefixes = new string[] { "!" };
            var commands = DiscordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = prefixes,
                //CaseSensitive = false,
                ////IgnoreExtraArguments = true,
                //EnableDms = true,
                EnableMentionPrefix = true,
            });
            var slash = DiscordClient.UseSlashCommands();

            DiscordClient.UseInteractivity();

            //RunExternalExe();
            //process.ErrorDataReceived += Process_OutputDataReceived;
            //commands.RegisterCommands<AdminCommands>();



            //To register them globally, once you're confident that they're ready to be used by everyone
            slash.RegisterCommands<SlashCommands>();

#if DEBUG___DEV
            commands.RegisterCommands<DebugCommands>();
#endif

            DiscordClient.Ready += Discord_Ready;
            DiscordClient.MessageCreated += MinecraftServer.Discord_MessageCreated;
            DiscordClient.ComponentInteractionCreated += Discord_ComponentInteractionCreated;
            DiscordClient.MessageReactionAdded += Discord_MessageReactionAdded;
            DiscordClient.MessageReactionRemoved += Discord_MessageReactionRemoved;
            DiscordClient.MessageReactionRemovedEmoji += Discord_MessageReactionRemovedEmoji;
            DiscordClient.MessageReactionsCleared += Discord_MessageReactionsCleared;

            await DiscordClient.ConnectAsync();
            await Task.Delay(-1);
        }



        #region Attachments for Starboard

        static string GetImageForStarboard(DiscordMessage message)
        {
            if (message is null)
                return string.Empty;

            if (message.Attachments.Count > 0)
                foreach (var attachment in message.Attachments)
                    if (attachment.MediaType.StartsWith("image/"))
                        return attachment.Url;

            if (message.Stickers.Count > 0)
                if (message.Stickers.First().FormatType == StickerFormat.PNG)
                    return message.Stickers.First().StickerUrl;

            if ((message.Content.StartsWith("https://media.discordapp.net/attachments/") && message.Content.Contains(".gif?")))
                return message.Content;
            if ((message.Content.StartsWith("https://media.discordapp.net/attachments/") && message.Content.EndsWith("png")))
                return message.Content;

            return string.Empty;
        }

        static string GetVideoForStarboard(DiscordMessage message)
        {
            if (message is null)
                return string.Empty;

            if (message.Attachments.Count > 0)
                foreach (var attachment in message.Attachments)
                    if (attachment.MediaType.StartsWith("video/"))
                        return attachment.Url;

            return string.Empty;
        }

        #endregion
        static void ProcessStarboardMessage(DiscordMessage message, DiscordEmoji emoji, DiscordGuild guild)
        {
            if (message.Author.IsBot)
                return;

            //if (!(emoji.Id == 0 && emoji.GetDiscordName() == ":star:")) // Is it Discord Star?
              //  return;


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

            var star_reactions = 0;
            var yeah_reactions = 0;
            var rt2_reactions = 0;
            var theyreright_reactions = 0;
            for (int i = 0; i < message.Reactions.Count; i++)
            {
                var reaction = message.Reactions[i];

                if (reaction.Emoji.Id == 0 && reaction.Emoji.GetDiscordName() == ":star:")
                    star_reactions = reaction.Count;

                if (reaction.Emoji.Id == 1178051440367915008)
                    yeah_reactions = reaction.Count;

                if (reaction.Emoji.Id == 1182430478834348113)
                    rt2_reactions = reaction.Count;

                if (reaction.Emoji.Id == 1163903968322265149)
                    theyreright_reactions = reaction.Count;
            }

#if DEBUG_DEV
            if (star_reactions >= 1 || yeah_reactions >= 1 || rt2_reactions >= 1 || theyreright_reactions >= 1)
#else
            if (star_reactions >= Starboard.StarboardThreshold || yeah_reactions >= Starboard.StarboardThreshold || rt2_reactions >= Starboard.StarboardThreshold || theyreright_reactions >= Starboard.StarboardThreshold)
#endif
            {
                var content = (message.Content.Length > 0 ? message.Content : string.Empty);
                var refMessage = message.ReferencedMessage;


                var starboard_image = GetImageForStarboard(message);
                var ref_starboard_image = GetImageForStarboard(refMessage);

                var starboard_video = GetVideoForStarboard(message);
                var ref_starboard_video = GetVideoForStarboard(refMessage);

                if (refMessage is not null)
                {
                    if (!string.IsNullOrWhiteSpace(refMessage.Content))
                        content += $"\n(in response to \"{refMessage.Content}\")";
                    else if (ref_starboard_image != string.Empty)
                        content += $"\n(in response to image)";

                    if (ref_starboard_video != string.Empty)
                        content += $"\n[Click Here for Original Video]({ref_starboard_video})";
                }


                var reactions_text =
                    $"{(star_reactions > 0 ? $":star: {star_reactions}  " : string.Empty)}" +
                    $"{(yeah_reactions > 0 ? $"<:yeah:1178051440367915008> {yeah_reactions}  " : string.Empty)}" +
                    $"{(rt2_reactions > 0 ? $"<:rt2:1182430478834348113> {rt2_reactions}  " : string.Empty)}" +
                    $"{(theyreright_reactions > 0 ? $"<:theyreright:1163903968322265149> {theyreright_reactions}  " : string.Empty)}";


                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Color = new DiscordColor(0xFF, 0xAC, 0x33),
                    /* Footer = new EmbedFooter()
                    {
                        IconUrl = "https://cdn.discordapp.com/emojis/1178395966076882975.webp?size=96&quality=lossless",
                        Text = $"Stars: {reactions.Count}",
                    }, */
                    Footer = new EmbedFooter()
                    {
                        //IconUrl = "https://cdn.discordapp.com/emojis/1178395966076882975.webp?size=96&quality=lossless",
                        Text = $"Total Reactions: {star_reactions + yeah_reactions + rt2_reactions + theyreright_reactions}",
                    },

                    Author = new EmbedAuthor()
                    {
                        IconUrl = message.Author.AvatarUrl,
                        Name = message.Author.Username
                    },
                    Description = $"{content}{(starboard_video == string.Empty ? string.Empty : $"\n[Click Here for Video]({starboard_video})")}" +
                                  $"\n\n[Click Here to Jump To Message](https://discord.com/channels/{guild.Id}/{message.ChannelId}/{message.Id})" +
                                  $"\n\n{reactions_text}",
                    Thumbnail = new EmbedThumbnail()
                    {
                        Url = ref_starboard_image,
                    },
                    ImageUrl = starboard_image
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
            if (Starboard.IsEmoteStarboardEmote(e.Emoji) && (e.User.Id == message.Author.Id) && (e.User.Id != 150745989836308480)) // Disable Self starring
            {
                e.Message.DeleteReactionAsync(e.Emoji, e.User);
                return Task.CompletedTask;
            } 

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

        private static Task Discord_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            MinecraftServer.Init();
            sender.UpdateStatusAsync(new DiscordActivity("Support Vultu!"), UserStatus.Online);
            Console.WriteLine("READY!");
            return Task.CompletedTask;
        }
    }
}