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
using VultuBot.Modules;

namespace VultuBot.Commands
{
    public class AssignableRoles : ApplicationCommandModule
    {
        public ulong[] AuthorizedUsers = new ulong[] {
            1177672585497034923, // Vultu
            198198681982205953, // Essem,
            146019453291855872, // Emma
            590057924387143702 // Ivory
        };


        [SlashCommand("AddAssignableRole", "Adds a new assignable role")]
        public async Task AddAssignableRoleCommand(InteractionContext ctx, [Option("role", "The Discord Role to add")] DiscordRole role)
        {
            if (ctx.Member is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"This command does not work in direct messages.").AsEphemeral());
                return;
            }

            if (!AuthorizedUsers.Contains(ctx.User.Id))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You are not verified to use this command.").AsEphemeral());
                return;
            }

            if (role is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"There was an error processing your request. ``role`` was ``null`, please notify Vultumast!").AsEphemeral());
                return;
            }

            if (Modules.AssignableRoles.Roles.ContainsKey(role.Id))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Role already added.").AsEphemeral());
                return;
            }

            Modules.AssignableRoles.Roles.Add(role.Id, role.Name);
            Modules.AssignableRoles.Write();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Added ``{role.Name}`` (with ID: ``{role.Id}``)"));
        }

        [SlashCommand("RemoveAssignableRole", "Removes Assignable Roles")]
        public async Task RemoveAssignableRoleCommand(InteractionContext ctx, [Option("role", "The Discord Role to Remove")] DiscordRole role)
        {
            if (ctx.Member is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"This command does not work in direct messages.").AsEphemeral());
                return;
            }

            if (!AuthorizedUsers.Contains(ctx.User.Id))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You are not verified to use this command.").AsEphemeral());
                return;
            }

            if (role is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"There was an error processing your request. ``role`` was ``null`, please notify Vultumast!").AsEphemeral());
                return;
            }

            Modules.AssignableRoles.Roles.Remove(role.Id);
            Modules.AssignableRoles.Write();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Removed ``{role.Name}`` (with ID: ``{role.Id}``)"));

        }

    }
}
