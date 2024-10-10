using AshLib;

public static class Palette{
	
	public static string Reset = "/[rc]";
	public static string GardenName = "/[C#,E83737]";
	public static string IncorrectFormat = "/[C#,D8725B]";
	public static string Flower = "/[C#,FF5555]";
	public static string PartnerName = "/[C#,5FDD58]";
	public static string Mailbox = "/[C#,A73FD3]";
	public static string Date = "/[C#,DBDB69]";
	public static string FlowerMessage = "/[C#,D8D8A4]";
	public static string Note = "/[C#,C6FFBA]";
	public static string NoteTitle = "/[C#,EA9723]";
	public static string Command = "/[C#,FFE460]";
	public static string CommandNotFound = "/[C#,DA1D00]";
	public static string Prompt = "/[C#,D28EFF]";
	public static string InputStart = "/[C#,FFCC00]";
	public static string InputWriting = "/[C#,FFEEA3]";
	public static string List = "/[C#,FF2869]";
	public static string Config = "/[C#,9CEDE5]";
	public static string SpecialNumber = "/[C#,FF7A68]";
	
	public static string LogStart = "/[C#,4900C1]";
	public static string LogContent = "/[C#,8554D3]";
	public static string DiscordLogStart = "/[C#,FF593F]";
	public static string DiscordLogContent = "/[C#,C6645D]";
	public static string MailboxLogStart = "/[C#,E8883A]";
	public static string MailboxLogContent = "/[C#,EFAC53]";
	
	public static CharFormat Format(string p){
		CharFormat cf = new CharFormat(Color(p), false);
		return cf;
	}
	
	public static Color3 Color(string p){
		return new Color3(p.Substring(5, 6));
	}
}