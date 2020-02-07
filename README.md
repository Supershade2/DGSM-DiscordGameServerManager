# DGSM-DiscordGameServerManager
C# discord bot that can be utilized to manage game servers through discord<br />
<em>Libraries/Sources used</em>
<ul>
  <li>
    DSharpplus: https://github.com/DSharpPlus/DSharpPlus
  </li>
  <li>
    NewtonsoftJSON: https://www.newtonsoft.com/json
  </li>
  <li>
    SourceRcon: https://github.com/aiusepsi/SourceRcon
  </li>
  <li>
	MSBUILD:<br /> https://www.nuget.org/packages/Microsoft.Build.Runtime/ <br /> https://www.nuget.org/packages/Microsoft.Build.Tasks.Core/ <br /> https://www.nuget.org/packages/Microsoft.Build/ <br /> https://www.nuget.org/packages/Microsoft.Build.Framework/ <br /> https://www.nuget.org/packages/Microsoft.Build.Utilities.Core/
  </li>
  </ul><br />
<em>Configuration explanation: Config.json</em>
<ul>
<li>
	token: discord bots token gets put in here
</li>
<li>
	server_guild_id: ID of the server or guild goes here
</li>
<li>
	discord_channel: discord channel id for the designated management channel goes here
</li>
<li>
	message_channel: discord channel id for special date messages goes here
</li>
<li>
	steamcmd_dir: path to steamcmd goes here
</li>
<li>
	wsapikey: steam workshop api key goes here
</li>
<li>
	wscollectionid: id of the steam workshop collection goes here
</li>
<li>
	rcon_address: address of the server to use RCON
</li>
<li>
	rcon_port: rcon port number for the server goes here
</li>
<li>
	rcon_pass: rcon password goes here
</li>
<li>
	admin_pass: used for ARK: Survival Evolved admin commands, the password for allowing admin commands goes here
</li>
<li>
	game_launch_args: Steam game launch arguments, specific to Ark:Survival Evolved go here
</li>
<li>
	prefix: prefix for denoting commands goes here e.g: ark, minecraft, etc
</li>
<li>
	backup_dir: path to directory where backups are stored goes here
</li>
<li>
	game: when specified creates a gameprofile.json for that game
</li>
<li>
	ID: Bots ID goes here
</li>
<li>
	gametracking_url: url for tracking game status using an external service goes here
</li>
<li>
	_messages: list of special messages is found and can be modified here, follow example messages for how to define messages with exception to the first three being used for other purposes
</li>
<li>
	useHeuristics: change to true to enable the use of basic heuristics on defined special messages, default is false
</li>
<li>
	registration_key: specify a registration key to enable users to register to receive an invite link to share with others
</li>
<li>
	invite: invitation link that will be handed out to users on successful registration
</li>
</ul>
