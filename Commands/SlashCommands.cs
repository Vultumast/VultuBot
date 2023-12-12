using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VultuBot.Commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("test", "Test shit")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Success!"));
        }

        /*[SlashCommand("roles", "Toggle Roles")]
        public async Task RoleCommand(InteractionContext ctx, [Choice("VC", 0)] [Choice("Fortnite", 1)][Choice("Minecraft", 1)] [Option("Roles", "Roles to toggle")] long choice = 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Success!"));
        }
        */

        [SlashCommand("github", "Pulls the Github link")]
        public async Task GithubCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("https://github.com/Vultumast/VultuBot"));
        }
        [SlashCommand("roles", "Toggle Roles")]
        public async Task RoleCommand(InteractionContext ctx)
        {
            var dropDownOptions = new List<DiscordSelectComponentOption>()
            {
                new DiscordSelectComponentOption("VC Role", "vc", isDefault: ctx.Member.Roles.Contains(ctx.Guild.Roles[Program.RoleIDs.VC])),
                new DiscordSelectComponentOption("Minecraft Role", "minecraft", isDefault: ctx.Member.Roles.Contains(ctx.Guild.Roles[Program.RoleIDs.Minecraft])),
                new DiscordSelectComponentOption("Fortnite Role", "fortnite",isDefault: ctx.Member.Roles.Contains(ctx.Guild.Roles[Program.RoleIDs.Fortnite])),
                new DiscordSelectComponentOption("Move Night Role", "movie night",isDefault: ctx.Member.Roles.Contains(ctx.Guild.Roles[Program.RoleIDs.Movie_Night])),
            };
            var dropDown = new DiscordSelectComponent("roleDropDown", null, dropDownOptions, false, 0, dropDownOptions.Count);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Select your Roles").AddComponents(dropDown).AsEphemeral());
        }
        [SlashCommand("kofi", "Support Vultu")]
        public async Task KofiCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Support Vultu here: https://ko-fi.com/radixcomet"));
        }
        /* [SlashCommand("color", "Get a role color")]
        public async Task ColorCommand(InteractionContext ctx, [Option("ColorCode", "RGB888 Hexcode in format of 0xFFFFFF")] string hexcode)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"WIP"));
        }*/
    }
}
