using System;
using System.Linq;
using AshLib;
using Discord;
using Discord.WebSocket;

class DiscordHandler{
	private Garden gar;
	private ulong channelID;
	private ulong otherBotId;
	private string token;
	
	private static DiscordSocketClient _client;
	
	public DiscordHandler(string t, ulong c, ulong o, Garden g){
		this.token = t;
		this.channelID = c;
		this.otherBotId = o;
		this.gar = g;
	}
	
	public async Task startDiscord(){
		DiscordSocketConfig _config = new DiscordSocketConfig { MessageCacheSize = 100, 
		AlwaysDownloadUsers = true, 
		GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers};
		
        _client = new DiscordSocketClient(_config);
		
		_client.Log += Log;
		_client.Ready += Ready;
		_client.MessageReceived += MessageReceived;
		
		await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
	}
	
	private Task Log(LogMessage arg){
		bool b;
		if(!this.gar.config.CanGetCampAsBool("doDiscordLog", out b) || !b){
			return Task.CompletedTask;
		}
        this.gar.writeToConsole("{$0}[LOG]/[rc] {$1}{$2}", Palette.DiscordLogStart, Palette.DiscordLogContent, arg);
        return Task.CompletedTask;
    }
	
	//Check the mailbox
	private async Task Ready(){
		//get the id of the last seen message
		ulong lastSeenMessage;
		if(!this.gar.config.CanGetCampAsUlong("lastSeenMessage", out lastSeenMessage)){
			log("lastSeenMessage cant be found");
			throw new Exception("lastSeenMessage cant be found");
		}
		
		log("Last seen message id: " + lastSeenMessage);
		
        // Get the channel
        ITextChannel channel = _client.GetChannel(this.channelID) as ITextChannel;
		
		bool foundSeenMessage = false;
		IMessage lastMessage = null;
		IMessage seenMessage = null;
		int mnum = 0;
		
		//search the last seen flower
		while(!foundSeenMessage){
			log2("Downloading messages...");
			IEnumerable<IMessage> messages;
			
			if (lastMessage == null){
				messages = await channel.GetMessagesAsync(30).FlattenAsync();
			} else{
				messages = await channel.GetMessagesAsync(lastMessage.Id, Direction.Before, 30).FlattenAsync();
			}
			
			log2("Downloaded messages length: " + messages.Count());
			if(!messages.Any()){
				log2("Downloaded messages is empty.");
				break;
			}
			
			foreach (IMessage message in messages){
				log2("Found message: " + message.Content);
				if(message.Id == lastSeenMessage){
					log2("Found last seen message!: " + message.Content);
					foundSeenMessage = true;
					seenMessage = message;
					break;
				}
				mnum++;
			}
			
			lastMessage = messages.Last();
		}
		
		if(seenMessage == null){
			log2("The last seen message was not found. Using the last message.");
			seenMessage = lastMessage;
		}
		
		if(seenMessage != null){
			log2("Using as last seen message: " + seenMessage.Content);
		}
		
		if(seenMessage == null){
			log2("Seen message is still null. Skipping.");
			this.gar.finishedReady();
			return;
		}
		
		log2("Entering phase 2");
		log2("Downloading messages: " + mnum);
		//Download all not seen messages
		
		IMessage m = await channel.GetMessageAsync(seenMessage.Id);
		
		IEnumerable<IMessage> dowMessages;
		dowMessages = await channel.GetMessagesAsync(seenMessage.Id, Direction.After, mnum).FlattenAsync();
		dowMessages = dowMessages.Reverse();
		
		List<IMessage> allMessages = new List<IMessage>();
		allMessages.Add(m);
		allMessages.AddRange(dowMessages);
		
		Emoji rose = new Emoji("🌹");
		Emoji paper = new Emoji("📋");
		
		//iterate through them
		ulong flowers = 0; //to count the number
		foreach (IMessage message in allMessages){
			log2("Processing message: " + message.Content);
			
			lastSeenMessage = message.Id;
			
			if (message.Author.Id == _client.CurrentUser.Id){
				continue;
			}
			
			if(!this.gar.discordAllowAll && message.Author.Id != this.otherBotId){
				continue;
			}

			if(this.gar.processMessage(message.Content, 2, false, out short h) && message is IUserMessage userMessage)
			{
				flowers++;
				log2("Found a flower: " + message.Content);
				switch(h){
					case 1:
					userMessage.AddReactionAsync(rose);
					break;
					case 2:
					userMessage.AddReactionAsync(paper);
					break;
				}
			}
		}
		
		this.gar.printEndOfMailbox(flowers);
		
		log("Last seen message id: " + lastSeenMessage);
		
		this.gar.config.SetCamp("lastSeenMessage", lastSeenMessage);
		this.gar.config.Save();
		
		this.gar.saveFlowers();
		
		this.gar.finishedReady();
	}
	
	private async Task MessageReceived(SocketMessage message){	
        if(message.Channel.Id != this.channelID){
			return;
		}
		
		this.gar.config.SetCamp("lastSeenMessage", message.Id);
		this.gar.config.Save();
		
		// Check if the message is from this bot
        if(message.Author.Id == _client.CurrentUser.Id){
            return;
		}

        log("Recieved message: " + message.Content);
		
		if(!this.gar.discordAllowAll && message.Author.Id != this.otherBotId){
			return;
		}
		
		if (this.gar.processMessage(message.Content, 1, true, out short h) && message is IUserMessage userMessage)
		{
			Emoji rose = new Emoji("🌹");
			Emoji paper = new Emoji("📋");
			switch(h){
				case 1:
				userMessage.AddReactionAsync(rose);
				break;
				case 2:
				userMessage.AddReactionAsync(paper);
				break;
			}
		}
    }
	
	public async Task sendMessage(string s){
		ITextChannel channel = _client.GetChannel(channelID) as ITextChannel;

		await channel.SendMessageAsync(s);
	}
	
	//debug log
	internal void log(object o){
		bool b;
		if(!this.gar.config.CanGetCampAsBool("doDebugLog", out b) || !b){
			return;
		}
		
		this.gar.writeToConsole("{$0}[DEBUG]/[rc] {$1}{$2}", Palette.LogStart, Palette.LogContent, o);
	}
	
	internal void log2(object o){
		bool b;
		if(!this.gar.config.CanGetCampAsBool("doDebugLog", out b) || !b){
			return;
		}
		
		this.gar.writeToConsole("{$0}[MAILBOX DEBUG]/[rc] {$1}{$2}", Palette.MailboxLogStart, Palette.MailboxLogContent, o);
	}
}