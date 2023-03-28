# Matrilineal Marriage
Matrilineal marriage mod for player family members in Mount &amp; Blade II: Bannerlord

## Content
This mod inverts the behavior of male and female player clan member marriages, i.e., male player clan members will leave the player clan to join their wives' clan, and female player clan members will stay in the player clan with their husbands. Some restrictions apply, as it is not possible to make a clan leader join your clan via this mod. Your female clan member will leave the clan if she marries another clan leader. The mod applies to NPCs only and does not affect player character marriages in any way.

Disclaimer: The approach of this mod is rather hacky as it tries to intercept all possible marriage offers for the player's clan members and then inverts their marriage outcome by running the respective marriage code again but with swapped parameters. The AI does not know about this mod and will propose marriage offers for your sisters and daughters in hopes of them joining the AI clans. This may make this mod a bit unbalanced.

## Compatibility
This mod should be safe to add or remove at any time as it only uses event-based C# code. The data stored in the savegame will be ignored if the mod is removed mid game and is removed after a successive save.
