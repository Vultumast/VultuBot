using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VultuBot.Modules
{
    public class AssignableRoles : ApplicationCommandModule
    {
        /* [SlashCommand("AddAssignableRole", "Adds a new assignable role")]
        public async Task AddAssignableRoleCommand(InteractionContext ctx, [Option("role", "The Discord Role to add")] DiscordRole role)
        {
            if (role is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"There was an error processing your request. ``role`` was ``null`, please notify Vultumast!"));
                return;
            }


        }

        [SlashCommand("RemoveAssignableRole", "Removes Assignable Roles")]
        public async Task RemoveAssignableRoleCommand(InteractionContext ctx, [Option("role", "The Discord Role to Remove")] DiscordRole role)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"WIP"));
        }
        */
    }
}
