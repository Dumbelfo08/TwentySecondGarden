using System;
using AshLib;

class Note{
	public Date date;
	public string title;
	public string content;
	
	public Note(Date d, string t, string c){
		this.date = d;
		this.title = t;
		this.content = c;
	}
	
	public string getAsMessage(){
		return "#" + this.date.ToCPTF() + this.title + "#[#" + this.content;
	}
	
	public static bool canParseFromMessage(string m, out Note n){
		if(m.Length < 11){
			n = null;
			return false;
		}
		
		if(m[0] != '#'){
			n = null;
			return false;
		}
		string d = m.Substring(1, 6);
		Date dat = new Date(d);
		
		string cont = m.Substring(7);
		
		string[] c = cont.Split("#[#");
		if(c.Length < 1){
			n = null;
			return false;
		}
		
		string body = String.Join("#[#", c.Skip(1));
		
		if(c[0].Length < 1 || body.Length < 1){
			n = null;
			return false;
		}
		
		n = new Note(dat, c[0], body);
		
		return true;
	}
	
	public static bool operator ==(Note a, Note b){
		if(a.date == b.date){
			return true;
		}
		return false;
	}
	
	public static bool operator !=(Note a, Note b){
		return !(a == b);
	}
	
	public override string ToString(){
		return this.date + " / " + this.title + " / " + this.content;
	}
}