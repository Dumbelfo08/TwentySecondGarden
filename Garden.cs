using System;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using AshLib;

class Garden{
	
	private Dependencies dep;
	private AshFile flowerList;
	private AshFile noteList;
	public AshFile config;
	
	private DiscordHandler dc;
	private ConsoleHandler ch;
	private NotificationHandler nh;
	
	private Counter cou;
	
	private List<Flower> flowers;
	private List<Note> notes;
	
	private string partnerName;
	
	public bool discordAllowAll = false;
	
	static void Main(string[] args){
		if(args.Length > 1 && args[0] == "-ToastActivated"){
			Console.WriteLine("The notification was clicked, but the app wasn't open.");
			return;
		}
		
		Garden gar = new Garden();
	}
	
	public Garden(){
		//search for debug test file
		string s = "/ashproject/twentysecondgarden";
		if(File.Exists("debug.ash")){
			AshFile debug = new AshFile("debug.ash");
			
			if(!debug.CanGetCampAsString("debugPath", out s)){
				s = "/ashproject/twentysecondgarden";
			}
			
			if(debug.CanGetCampAsBool("dcAll", out bool p) && p){
				discordAllowAll = true;
			}
		}else{
			s = "/ashproject/twentysecondgarden";
		}
		
		//Initialize dependencies
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		this.dep = new Dependencies(appDataPath + s, true, null, null);
		
		this.initializeConfig();
		
		this.initilizeFlowers();
		
		this.initilizeNotes();
		
		this.cou = new Counter(this.dep, this);
		
		//initialize the console
		this.ch = new ConsoleHandler(this);
		
		this.nh = new NotificationHandler(this);
		
		//Do setup if needed
		if(this.config.CanGetCampAsBool("setupCompleted", out bool b) && !b){
			doSetup();
			config.Save();
		}
		
		//Extract needed values
		if(!this.config.CanGetCampAsString("partnerName", out partnerName)){
			throw new Exception("Cant get the partner name");
		}
		
		string token;
		ulong channelID;
		ulong otherBotId;
		
		if(!this.config.CanGetCampAsString("botToken", out token)){
			throw new Exception("Cant get the bot token");
		}
		
		if(!this.config.CanGetCampAsUlong("botChannel", out channelID)){
			throw new Exception("Cant get the channel id");
		}
		
		if(!this.config.CanGetCampAsUlong("otherBotId", out otherBotId)){
			throw new Exception("Cant get the other bot id");
		}
		
		writeToConsole("/[0]Welcome to the {$0}Twenty Second Garden/[rc]. First, lets check your {$1}mailbox/[rc]!", Palette.GardenName, Palette.Mailbox);
		writeToConsole("");
		
		//initialize the discord
		this.dc = new DiscordHandler(token, channelID, otherBotId, this);
		Task.Run(() => this.dc.startDiscord());
	}
	
	private void initializeConfig(){
		//Initialize config
		this.config = this.dep.config;
		
		this.config.InitializeCamp("setupCompleted", false);
		this.config.InitializeCamp("doDiscordLog", false);
		this.config.InitializeCamp("doDebugLog", false);
		this.config.InitializeCamp("botToken", "");
		this.config.InitializeCamp("botChannel", (ulong) 0);
		this.config.InitializeCamp("otherBotId", (ulong) 0);
		this.config.InitializeCamp("partnerName", "");
		this.config.InitializeCamp("myName", "");
		this.config.InitializeCamp("lastSeenMessage", (ulong) 0);
		this.config.InitializeCamp("sendNotifications", true);
		this.config.InitializeCamp("sendSound", true);
		this.config.InitializeCamp("soundOnlyWhenFocused", false);
		this.config.InitializeCamp("instantNotes", false);
		this.config.Save();
	}
	
	public void initilizeFlowers(){
		//Initialize flower list
		this.flowerList = this.dep.ReadAshFile("flowerlist.ash");
		
		this.flowerList.InitializeCamp("numberOfFlowers", (ulong) 0);
		this.flowerList.Save();
		
		//Load flowerlist from ashfile
		generateFlowerlist();
	}
	
	public void initilizeNotes(){
		//Initialize note list
		this.noteList = this.dep.ReadAshFile("notelist.ash");
		
		this.noteList.InitializeCamp("numberOfNotes", (ulong) 0);
		this.noteList.Save();
		
		//Load flowerlist from ashfile
		generateNotelist();
	}
	
	//generate the flower list from the ashfile
	private void generateFlowerlist(){
		ulong numberOfFlowers;
		
		if(!this.flowerList.CanGetCampAsUlong("numberOfFlowers", out numberOfFlowers)){
			throw new Exception("Cant get the number of flowers");
		}
		
		this.flowers = new List<Flower>();
		
		for(ulong i = 0; i < numberOfFlowers; i++){
			Date d;
			if(!this.flowerList.CanGetCampAsDate("dateFlower" + i.ToString(), out d)){
				throw new Exception("Cant get the date of " + i + " flower");
			}
			
			if(this.flowerList.CanGetCampAsString("messageFlower" + i.ToString(), out string m)){
				this.flowers.Add(new Flower(d, m));
				continue;
			}
			this.flowers.Add(new Flower(d));
		}
	}
	
	private void generateNotelist(){
		ulong numberOfNotes;
		
		if(!this.noteList.CanGetCampAsUlong("numberOfNotes", out numberOfNotes)){
			throw new Exception("Cant get the number of notes");
		}
		
		this.notes = new List<Note>();
		
		for(ulong i = 0; i < numberOfNotes; i++){
			Date d;
			if(!this.noteList.CanGetCampAsDate("dateNote" + i.ToString(), out d)){
				throw new Exception("Cant get the date of " + i + " note");
			}
			
			string t;
			if(!this.noteList.CanGetCampAsString("titleNote" + i.ToString(), out t)){
				throw new Exception("Cant get the title of " + i + " note");
			}
			
			string c;
			if(!this.noteList.CanGetCampAsString("contentNote" + i.ToString(), out c)){
				throw new Exception("Cant get the content of " + i + " note");
			}
			
			this.notes.Add(new Note(d, t, c));
		}
	}
	
	//returns true if the same flower is contained
	public bool containsFlower(Flower f){
		for(int i = 0; i < flowers.Count; i++){
			if(flowers[i] == f){
				return true;
			}
		}
		return false;
	}
	
	public bool containsNote(Note f){
		for(int i = 0; i < notes.Count; i++){
			if(notes[i] == f){
				return true;
			}
		}
		return false;
	}
	
	public bool processMessage(string m, short s, bool b, out short h){
		switch(m[0]){
			case '@':
			Flower f;
			if(!Flower.canParseFromMessage(m, out f)){
				h = 0;
				return false;
			}
			
			if(!containsFlower(f)){
				this.flowers.Add(f);
				if(b){
					this.saveFlowers();
				}
				
				switch(s){
					case 1:
					printInfoOfRecievedFlower(f);
					break;
					
					case 2:
					printInfoOfAwayFlower(f);
					break;
				}
				h = 1;
				return true;
			}
			h = 0;
			return false;
			
			case '#':
			Note n;
			if(!Note.canParseFromMessage(m, out n)){
				h = 0;
				return false;
			}
			
			if(!containsNote(n)){
				this.notes.Add(n);
				if(b){
					this.saveNotes();
				}
				
				switch(s){
					case 1:
					printInfoOfRecievedNote(n);
					break;
					
					case 2:
					printInfoOfAwayNote(n);
					break;
				}
				h = 2;
				return true;
			}
			h = 0;
			return false;
			
			default:
			h = 0;
			return false;
		}
	}
	
	//If it doesnt exists, add it
	public bool processFlower(Flower f, short m){
		if(!containsFlower(f)){
			this.flowers.Add(f);
			switch(m){
				case 1:
				printInfoOfRecievedFlower(f);
				break;
				
				case 2:
				printInfoOfAwayFlower(f);
				break;
			}
			return true;
		}
		return false;
	}
	
	//Transform the flower list to the ashfile and save it into the file
	public void saveFlowers(){
		this.flowerList = new AshFile(this.flowerList.path);
		
		this.flowerList.SetCamp("numberOfFlowers", (ulong) this.flowers.Count);
		
		for(int i = 0; i < this.flowers.Count; i++){
			this.flowerList.SetCamp("dateFlower" + i.ToString(), this.flowers[i].date);
			if(this.flowers[i].message != null){
				this.flowerList.SetCamp("messageFlower" + i.ToString(), this.flowers[i].message);
			}
		}
		
		this.flowerList.Save();
	}
	
	public void saveNotes(){
		this.noteList = new AshFile(this.noteList.path);
		
		this.noteList.SetCamp("numberOfNotes", (ulong) this.notes.Count);
		
		for(int i = 0; i < this.notes.Count; i++){
			this.noteList.SetCamp("dateNote" + i.ToString(), this.notes[i].date);
			this.noteList.SetCamp("titleNote" + i.ToString(), this.notes[i].title);
			this.noteList.SetCamp("contentNote" + i.ToString(), this.notes[i].content);
		}
		
		this.noteList.Save();
	}
	
	public void printEndOfMailbox(ulong f){
		if(f == 0){
			writeToConsole("It appears that your {$0}mailbox/[rc] is empty!", Palette.Mailbox);
		} else{
			writeToConsole("Those were the contents of your {$0}mailbox", Palette.Mailbox);
		}
		writeToConsole("");
	}
	
	private void printInfoOfAwayFlower(Flower f){
		
		FormatString s = new FormatString();
		s.Append("There is a {$0}flower/[rc] in your {$1}mailbox/[rc]. {$2}{$3}/[rc] was thinking about you while you weren't there, at {$4}{$5}", Palette.Flower, Palette.Mailbox, Palette.PartnerName, partnerName, Palette.Date, f.date);
		
		if(f.message != null){
			s.Append(Environment.NewLine);
			
			s.Append("/[rc]The message included with the {$0}flower/[rc] reads: \"{$1}{$2}/[rc]\"", Palette.Flower, Palette.FlowerMessage, f.message);
		}
		
		writeToConsole(s);
		writeToConsole("");
		
		this.trySendNotification("There is a flower in your mailbox!");
	}
	
	private void printInfoOfRecievedFlower(Flower f){
		FormatString s = new FormatString();
		
		s.Append("{$0}{$1}/[rc] just sent you a {$2}flower/[rc] right now! They were thinking about you at {$3}{$4}", Palette.PartnerName, partnerName, Palette.Flower, Palette.Date, f.date);

		if (f.message != null){
			s.Append(Environment.NewLine);
			s.Append("/[rc]The message included with the {$0}flower/[rc] was: \"{$1}{$2}/[rc]\"", Palette.Flower, Palette.FlowerMessage, f.message);
		}
		
		writeToConsole(s);
		writeToConsole("");
		
		this.trySendNotification("You just got a flower!");
	}
	
	private void printInfoOfAwayNote(Note n){
		
		FormatString s = new FormatString();
		
		s.Append("There is a {$0}note/[rc] in your {$1}mailbox/[rc]. {$2}{$3}/[rc] sent it to you while you weren't there, at {$4}{$5}/[rc]. The title of the note is: {$6}{$7}/[rc].", Palette.Note, Palette.Mailbox, Palette.PartnerName, partnerName, Palette.Date, n.date, Palette.NoteTitle, n.title);
		
		bool b;
		if(this.config.CanGetCampAsBool("instantNotes", out b) && b){
			s.Append(" The contents of the note are:");
			writeToConsole(s);
			writeToConsole(n.content);
		} else{
			s.Append(" You can read its contents later if you want to.");
			writeToConsole(s);
		}
		
		writeToConsole("");
		
		this.trySendNotification("There is a note in your mailbox!");
	}
	
	private void printInfoOfRecievedNote(Note n){
		FormatString s = new FormatString();

		s.Append("{$0}{$1}/[rc] just sent you a {$2}note/[rc] right now! The title of the note is: {$3}{$4}/[rc].", Palette.PartnerName, partnerName, Palette.Note, Palette.NoteTitle, n.title);
		
		bool b;
		if(this.config.CanGetCampAsBool("instantNotes", out b) && b){
			s.Append(" The contents of the note are:");
			writeToConsole(s);
			writeToConsole(n.content);
		} else{
			s.Append(" You can read its contents by executing this command: {$0}read/[rc].", Palette.Command);
			writeToConsole(s);
		}
		
		writeToConsole("");
		
		this.trySendNotification("You just got a note!");
	}
	
	//do the setup
	private void doSetup(){
		writeToConsole("/[rc]Hello! {$0}Welcome/[rc] to the {$1}Twenty-Second Garden/[rc]. To start in this program, you need some {$2}values/[rc] for everything to work right. You can ask the {$3}person/[rc] who sent it to you.", Palette.Prompt, Palette.GardenName, Palette.MailboxLogContent, Palette.PartnerName);
		this.ch.canWrite = true;
		
		string s;
		while(true){
			s = ch.askQuestion("Enter the bot token:");
			
			this.cou.registerInput(s);
			
			byte b = doSetupWithCode(s);
			if(b == 0){
				break;
			}else if(b == 1){
				continue;
			}else if(b == 2){
				writeToConsole("The setup is completed. Thank you!");
				writeToConsole("What is this garden? With it, you can send a {$0}flower/[rc] to your {$1}partner/[rc] every time you are thinking about them, and see the ones they send you.", Palette.Flower, Palette.PartnerName);
				writeToConsole("");
				return;
			}
			
		}
		this.config.SetCamp("botToken", s);
		
		while(true){
			s = ch.askQuestion("Enter the other bot id:");
			
			this.cou.registerInput(s);
			
			if(ulong.TryParse(s, out ulong u)){
				this.config.SetCamp("otherBotId", (ulong) ulong.Parse(s));
				break;
			}
			writeToConsole("{$0}The format was incorrect. Try again", Palette.IncorrectFormat);
		}
		
		while(true){
			s = ch.askQuestion("Enter the bot channel:");
			
			this.cou.registerInput(s);
			
			if(ulong.TryParse(s, out ulong u)){
				this.config.SetCamp("botChannel", (ulong) ulong.Parse(s));
				break;
			}
			writeToConsole("{$0}The format was incorrect. Try again", Palette.IncorrectFormat);
		}
		
		s = ch.askQuestion("Enter your name:");
		
		this.cou.registerInput(s);
		
		this.config.SetCamp("myName", s);
		
		s = ch.askQuestion("Enter your partner's name:");
		
		this.cou.registerInput(s);
		
		this.config.SetCamp("partnerName", s);
		
		this.ch.canWrite = false;
		this.config.SetCamp("setupCompleted", true);
		
		writeToConsole("The setup is completed. Thank you!");
		writeToConsole("What is this garden? With it, you can send a {$0}flower/[rc] to your {$1}partner/[rc] every time you are thinking about them, and see the ones they send you.", Palette.Flower, Palette.PartnerName);
		writeToConsole("");
	}
	
	//Do the setup but with an initialization code
	private byte doSetupWithCode(string s){
		writeToConsole("Attempting setup with code");
		if(s.Length < 2){
			writeToConsole("{$0}The format for code setup was incorrect", Palette.IncorrectFormat);
			return 0;
		}
		
		if(s[0] != '@'){
			return 0;
		}
		
		if(s[1] == '@'){
			s = s.Substring(1); //second char is @
			this.config.SetCamp("doDiscordLog", true);
			this.config.SetCamp("doDebugLog", true);
		}
		
		s = s.Substring(1); //first char is @
		string[] p = s.Split(";");
		
		if(p.Length == 1){
			return 0;
		}
		
		if(p.Length < 5){
			writeToConsole("{$0}The format for code setup was incorrect.", Palette.IncorrectFormat);
			return 1;
		}
		
		this.config.SetCamp("botToken", p[0]);
		
		ulong u;
		if(!ulong.TryParse(p[1], out u)){
			writeToConsole("{$0}The format for code setup was incorrect.", Palette.IncorrectFormat);
			return 1;
		}
		this.config.SetCamp("otherBotId", u);
		
		if(!ulong.TryParse(p[2], out u)){
			writeToConsole("{$0}The format for code setup was incorrect.", Palette.IncorrectFormat);
			return 1;
		}
		this.config.SetCamp("botChannel", u);
		
		this.config.SetCamp("myName", p[3]);
		this.config.SetCamp("partnerName", p[4]);
		
		this.config.SetCamp("setupCompleted", true);
		return 2;
	}
	
	public async Task finishedReady(){
		writeToConsole("You can now use the {$0}Garden/[rc]. Have a nice stay!", Palette.GardenName);
		writeToConsole("");
		
		allowWrite(true);
		
		//Start the input
		this.handleConsoleInput();
	}
	
	private void handleConsoleInput(){
		FormatString prompt = new FormatString();
		prompt.Append("{$0}Enter a command, or \"help\" to view the list of commands:", Palette.Prompt);
		
		FormatString inp = new FormatString();
		inp.Append("{$0}> ", Palette.InputStart);
		
		while(true){
			this.ch.addToInput(inp);
			string h = this.ch.askQuestion(prompt, true, Palette.Format(Palette.InputWriting));
			
			h = h.Substring(2);
			
			this.processInput(h);
		}
	}
	
	private void processInput(string p){	
		writeToConsole("{$0}»> {$1}{$2}", Palette.InputStart, Palette.Command, p);
		
		this.cou.registerInput(p);
		
		string[] w = p.Split(" ");
		
		if(w.Length > 0){
			string f = w[0].ToLower();
			
			switch(f){
				case "help":
				case "list":
				printHelp();
				break;
				
				case "send":
				sendFlowerRightNow();
				break;
				
				case "late":
				sendFlowerTime();
				break;
				
				case "garden":
				case "see":
				seeAllFlowers();
				break;
				
				case "notes":
				seeAllNotes();
				break;
				
				case "read":
				readNote();
				break;
				
				case "write":
				writeNote();
				break;
				
				case "config":
				configCommand();
				writeToConsole("");
				break;
				
				case "exit":
				exit();
				break;
				
				default:
				writeToConsole("{$0}Command not found", Palette.CommandNotFound);
				break;
			}
		}
	}
	
	private void printHelp(){
		writeToConsole("The full list of {$0}command/[rc] is:", Palette.InputStart);
		writeToConsole("{$0}send/[rc]    	-   Send a flower to your partner right now, because you are thinking about them right now.", Palette.Command);
		writeToConsole("{$0}late/[rc]	    -   Send a flower to your partner dated past in time, because you were thinking about them and couldn't use the garden.", Palette.Command);
		writeToConsole("{$0}garden/[rc]  	-   See your garden, with all the flowers your partner has ever sent you.", Palette.Command);
		writeToConsole("{$0}notes/[rc]   	-   See all the notes your partner has ever sent you.", Palette.Command);
		writeToConsole("{$0}read/[rc]    	-   Read a note.", Palette.Command);
		writeToConsole("{$0}write/[rc]   	-   Write a note.", Palette.Command);
		writeToConsole("{$0}config/[rc]  	-   Change aspects of the application.", Palette.Command);
		writeToConsole("{$0}help/[rc]    	-   See all the commands and all that there is to offer.", Palette.Command);
		writeToConsole("{$0}exit/[rc]    	-   Exit the application.", Palette.Command);
		writeToConsole("");
	}
	
	//Send a flower dated now, with the argument being the message
	private void sendFlowerRightNow(){
		FormatString c = new FormatString();
		c.Append("Do you want to include a {$0}message/[rc] in this {$1}flower/[rc] (Empty for no message)?", Palette.FlowerMessage, Palette.Flower);
		string s;
		while(true){
			s = this.ch.askQuestion(c, false, Palette.Format(Palette.FlowerMessage));
			
			this.cou.registerInput(s);
			
			if(s != null && s.Length > 190){
				writeToConsole("{$0}Those are too many charachters for a flower! The limit is 190. Maybe try writing a note ;)", Palette.IncorrectFormat);
				continue;
			}
			break;
		}
		
		Date d = (Date) DateTime.Now;
		
		Flower f = new Flower(d, s);
		
		this.dc.sendMessage(f.getAsMessage());
		
		writeToConsole("You sent a {$0}flower/[rc] to {$1}{$2}/[rc] right now", Palette.Flower, Palette.PartnerName, partnerName);
		writeToConsole("");
	}
	
	//Send a flower dated now, with the argument being the message
	private void sendFlowerTime(){
		FormatString c = new FormatString();
		c.Append("When were you thinking about {$0}{$1}/[rc]? ({$2}HH:mm:ss DD/MM/YYYY/[rc])", Palette.PartnerName, partnerName, Palette.Date);
		DateTime dt;
		while(true){
			string h = this.ch.askQuestion(c, false, Palette.Format(Palette.Date));
			
			this.cou.registerInput(h);
			
			if(!DateTime.TryParseExact(h, "HH:mm:ss dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)){
				writeToConsole("{$0}That date is not in a correct format", Palette.IncorrectFormat);
				continue;
			}
			if(dt.Year < 2020){
				writeToConsole("{$0}I highly doubt you were thinking about them that year...", Palette.CommandNotFound);
				continue;
			}
			break;
		}
		Date d = (Date) dt;
		
		c = new FormatString();
		c.Append("Do you want to include a {$0}message/[rc] in this {$1}flower/[rc] (Empty for no message)?", Palette.FlowerMessage, Palette.Flower);
		
		string s;
		while(true){
			s = this.ch.askQuestion(c, false, Palette.Format(Palette.FlowerMessage));
			
			this.cou.registerInput(s);
			
			if(s != null && s.Length > 190){
				writeToConsole("{$0}Those are too many charachters for a flower! The limit is 190. Maybe try writing a note ;)", Palette.IncorrectFormat);
				continue;
			}
			break;
		}
		
		Flower f = new Flower(d, s);
		
		this.dc.sendMessage(f.getAsMessage());
		
		writeToConsole("", Palette.Flower, Palette.PartnerName, partnerName);
		writeToConsole("You sent a {$0}flower/[rc] to {$1}{$2}", Palette.Flower, Palette.PartnerName, partnerName);
		writeToConsole("");
	}
	
	private void seeAllFlowers(){
		writeToConsole("This is your {$0}garden/[rc], with all the {$1}flowers/[rc] you ever recieved from your {$2}partner/[rc]", Palette.GardenName, Palette.Flower, Palette.PartnerName);
		if(this.flowers.Count == 0){
			writeToConsole("It seems like your {$0}garden/[rc] is empty... for now!", Palette.GardenName);
			return;
		}
		writeToConsole("There are {$0}{$1}/[rc] {$2}flowers/[rc]:", Palette.List, this.flowers.Count, Palette.Flower);
		writeToConsole("");
		
		foreach(Flower f in this.flowers){
			FormatString c = new FormatString();
			c.Append("{$0}{$1}/[rc] sent you a {$2}flower/[rc] when they thought about you at {$3}{$4}/[rc]", Palette.PartnerName, partnerName, Palette.Flower, Palette.Date, f.date.ToString());
			
			if (f.message != null){
				c.Append(". The message included with the flower was: {$0}{$1}/[rc]", Palette.FlowerMessage, f.message);
			}
			writeToConsole(c);
		}
		writeToConsole("");
	}
	
	private void seeAllNotes(){
		writeToConsole("These are all the {$0}notes/[rc] you ever received from your {$1}partner/[rc]:", Palette.Note, Palette.PartnerName);

		if (this.notes.Count == 0){
			writeToConsole("You haven't yet received any {$0}notes/[rc].", Palette.Note);
			return;
		}
		writeToConsole("There are {$0}{$1}/[rc] {$2}notes/[rc]:", Palette.List, this.notes.Count, Palette.Note);
		writeToConsole("");
		
		foreach(Note n in this.notes){			
			writeToConsole("{$0}{$1}/[rc] sent you a {$2}note/[rc] at {$3}{$4}/[rc]. Its title is: {$5}{$6}/[rc]", Palette.PartnerName, partnerName, Palette.Note, Palette.Date, n.date.ToString(), Palette.NoteTitle, n.title);
		}
		writeToConsole("");
	}
	
	private void readNote(){
		if(this.notes.Count < 1){
			writeToConsole("{$0}There are no {$1}notes{$0} to read!", Palette.IncorrectFormat, Palette.Note);
			return;
		}
		FormatString f = new FormatString();
		f.Append("What is the title of the {$0}note/[rc] you want to read?:", Palette.Note);
		string v = ch.askQuestion(f, false, Palette.Format(Palette.NoteTitle));
		
		this.cou.registerInput(v);
		
		int[] indexes = findIndexesOfNotesByTitle(v);
		
		Note n = null;
		
		if(indexes.Length < 1){
			writeToConsole("{$0}No notes were found with that title", Palette.IncorrectFormat);
			writeToConsole("");
			return;
		}
		
		if(indexes.Length == 1){
			n = this.notes[indexes[0]];
		}
		
		if(indexes.Length > 1){
			writeToConsole("Multiple {$0}notes/[rc] were found with that title. A list will be shown with a {$1}number/[rc], please enter the {$1}number/[rc] of the note you are referring to to read it:", Palette.Note, Palette.List);
			for(int i = 0; i < indexes.Length; i++){
				writeToConsole("{$0}{$1}/[rc]. {$2}{$3}/[rc] / {$4}{$5}/[rc]", Palette.List, (i+1), Palette.NoteTitle, this.notes[indexes[i]].title, Palette.Date, this.notes[indexes[i]].date);
			}
			while(true){
				f = new FormatString();
				f.Append("Enter the number associated with the {$0}note/[rc] you want to read:", Palette.Note);
				string s = ch.askQuestion(f, false, Palette.Format(Palette.List));
				
				this.cou.registerInput(s);
				
				int u;
				if(!int.TryParse(s, out u) || u < 1 || u > indexes.Length){
					writeToConsole("{$0}That number is not correct or isn't a number", Palette.IncorrectFormat);
					continue;
				}
				
				n = this.notes[indexes[u-1]];
				break;
			}
		}
		
		writeToConsole("");
		
		writeToConsole("{$0}{$1}/[rc] - {$2}{$3}/[rc]", Palette.NoteTitle, n.title, Palette.Date, n.date);
		
		writeToConsole(n.content);
		
		writeToConsole("");
	}
	
	private int[] findIndexesOfNotesByTitle(string t){
		List<int> indexes = new List<int>();
		for(int i = 0; i < this.notes.Count; i++){
			if(this.notes[i].title == t){
				indexes.Add(i);
			}
		}
		return indexes.ToArray();
	}
	
	private void configCommand(){
		writeToConsole("This are all of the {$1}configs/[rc] available:", Palette.List, Palette.Config);
		writeToConsole("{$0}1/[rc]. {$1}Windows notifications/[rc].                    Toggle the windows notifications", Palette.List, Palette.Config);
		writeToConsole("{$0}2/[rc]. {$1}Notification sound only when not focused/[rc]. Toggle between the notification sound playing always or only when the application isn't focused", Palette.List, Palette.Config);
		writeToConsole("{$0}3/[rc]. {$1}Notification sound/[rc].                       Toggle entirely disabling notification sounds", Palette.List, Palette.Config);
		writeToConsole("{$0}4/[rc]. {$1}Instant note read/[rc].                        Toggle showing the note content as soon as the note arrives", Palette.List, Palette.Config);
		writeToConsole("");
		writeToConsole("{$0}5/[rc]. {$1}Debug log/[rc].                        Toggle the general debug log", Palette.List, Palette.LogStart);
		writeToConsole("{$0}6/[rc]. {$1}Discord debug log/[rc].                Toggle the discord debug log", Palette.List, Palette.LogStart);
		
		FormatString f = new FormatString();
		f.Append("Please enter the {$0}number/[rc] of the {$1}config/[rc] that you want to change", Palette.List, Palette.Config);
		
		while(true){
			string h = this.ch.askQuestion(f, false, Palette.Format(Palette.List));
			
			this.cou.registerInput(h);
			
			int c;
			if(!int.TryParse(h, out c) || c < 1 || c > 6){
				writeToConsole("{$0}That number is not correct or isn't a number", Palette.IncorrectFormat);
				continue;
			}
			
			switch(c){
				case 1:
				bool b;
				if(!this.config.CanGetCampAsBool("sendNotifications", out b)){
					b = true;
				}
				b = !b;
				this.config.SetCamp("sendNotifications", b);
				this.config.Save();
				if(b){
					writeToConsole("{$0}Notifications will now show!", Palette.PartnerName);
					return;
				}
				writeToConsole("{$0}Notifications will now not show!", Palette.Flower);
				return;
				
				case 2:
				if(!this.config.CanGetCampAsBool("soundOnlyWhenFocused", out b)){
					b = false;
				}
				b = !b;
				this.config.SetCamp("soundOnlyWhenFocused", b);
				this.config.Save();
				if(b){
					writeToConsole("{$0}Notification sounds will now only play when not focused!", Palette.Flower);
					return;
				}
				writeToConsole("{$0}Notification sounds will now play always!", Palette.PartnerName);
				return;
				
				case 3:
				if(!this.config.CanGetCampAsBool("sendSound", out b)){
					b = true;
				}
				b = !b;
				this.config.SetCamp("sendSound", b);
				this.config.Save();
				if(b){
					writeToConsole("{$0}Sounds will now play", Palette.PartnerName);
					return;
				}
				writeToConsole("{$0}Sounds will now not play", Palette.Flower);
				return;
				
				case 4:
				if(!this.config.CanGetCampAsBool("instantNotes", out b)){
					b = false;
				}
				b = !b;
				this.config.SetCamp("instantNotes", b);
				this.config.Save();
				if(b){
					writeToConsole("{$0}Notes will now be read instantly", Palette.PartnerName);
					return;
				}
				writeToConsole("{$0}Notes will now not be read instantly", Palette.Flower);
				return;
				
				case 5:
				if(!this.config.CanGetCampAsBool("doDebugLog", out b)){
					b = false;
				}
				b = !b;
				this.config.SetCamp("doDebugLog", b);
				this.config.Save();
				if(b){
					writeToConsole("{$0}Debug log will now be shown", Palette.PartnerName);
					return;
				}
				writeToConsole("{$0}Debug log will now not show", Palette.Flower);
				return;
				
				case 6:
				if(!this.config.CanGetCampAsBool("doDiscordLog", out b)){
					b = false;
				}
				b = !b;
				this.config.SetCamp("doDiscordLog", b);
				this.config.Save();
				if(b){
					writeToConsole("{$0}Discord log will now be shown", Palette.PartnerName);
					return;
				}
				writeToConsole("{$0}Discord log will now not show", Palette.Flower);
				return;
			}
		}
	}
	
	private void writeNote(){
		string t;
		while(true){
			FormatString f = new FormatString();
			f.Append("Please enter the title of the {$0}note/[rc] you want to write:", Palette.Note);
			t = ch.askQuestion(f, false, Palette.Format(Palette.NoteTitle));
			
			this.cou.registerInput(t);
			
			if(t.Contains("#[")){
				writeToConsole("{$0}The note title can't contain \"#[\"", Palette.IncorrectFormat);
				continue;
			}
			
			if(t.Length > 80){
				writeToConsole("{$0}Those are too many charachters! The limit is 80. Try again!", Palette.IncorrectFormat);
				continue;
			}
			break;
		}
		
		string c;
		while(true){
			FormatString f = new FormatString();
			f.Append("Please enter the content of the {$0}note/[rc]:", Palette.Note);
			c = ch.askQuestion(f);
			
			this.cou.registerInput(c);
			
			if(t.Length > 1900){
				writeToConsole("{$0}Those are too many charachters! The limit is 1900, due to discord limitations. Try again!", Palette.IncorrectFormat);
				continue;
			}
			break;
		}
		
		Date d = (Date) DateTime.Now;
		Note n = new Note(d, t, c);
		
		this.dc.sendMessage(n.getAsMessage());
		
		writeToConsole("");
		writeToConsole("You sent a {$0}note/[rc] to {$1}{$2}/[rc] right now", Palette.Note, Palette.PartnerName, partnerName);
		writeToConsole("");
	}
	
	private void trySendNotification(string text){
		bool a, b, c;
		bool f = this.isWindowFocused();
		
		if(!f){
			if(this.config.CanGetCampAsBool("sendNotifications", out a) && a){
				this.nh.showNotification(text);
			}
		}
		
		if(this.config.CanGetCampAsBool("sendSound", out b) && b){
			if(this.config.CanGetCampAsBool("soundOnlyWhenFocused", out c)){
				if((c && !f) || !c){
					this.nh.playSound();
				}
			}
		}
		
	}
	
	[DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
	
	private bool isWindowFocused(){
        IntPtr consoleWindow = GetConsoleWindow();        // Get the handle of the console window
        IntPtr foregroundWindow = GetForegroundWindow();  // Get the handle of the currently focused window

        return consoleWindow == foregroundWindow;         // Compare the handles
    }
	
	private void allowWrite(bool b){
		ch.canWrite = b;
	}
	
	public void writeToConsole(FormatString s){
		this.ch.write(s);
	}
	
	public void writeToConsole(string s){
		this.ch.write(s);
	}
	
	public void writeToConsole(string s, params object[] objs){
		FormatString fs = new FormatString();
		fs.Append(s, objs);
		this.ch.write(fs);
	}
	
	private void exit(){
		writeToConsole("{$0}See you later!", Palette.Prompt);
		Environment.Exit(0);
	}
	
	//debug log
	internal void log(object o){
		bool b;
		if(!this.config.CanGetCampAsBool("doDebugLog", out b) || !b){
			return;
		}
		
		writeToConsole("{$0}[DEBUG]/[rc] {$1}{$2}", Palette.LogStart, Palette.LogContent, o);
	}
}