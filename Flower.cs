using System;
using AshLib;

class Flower{
	public Date date;
	public string? message;
	
	public Flower(Date d, string? m){
		this.date = d;
		this.message = m;
	}
	
	public Flower(Date d){
		this.date = d;
		this.message = null;
	}
	
	public string getAsMessage(){
		return "@" + this.date.ToCPTF() + this.message;
	}
	
	public static bool canParseFromMessage(string m, out Flower f){
		if(m.Length < 7){
			f = null;
			return false;
		}
		
		if(m[0] != '@'){
			f = null;
			return false;
		}
		string d = m.Substring(1, 6);
		Date dat = new Date(d);
		
		string mes = null;
		if(m.Length > 7){
			mes = m.Substring(7);
		}
		
		f = new Flower(dat, mes);
		return true;
	}
	
	public static bool operator ==(Flower a, Flower b){
		if(a.date == b.date){
			return true;
		}
		return false;
	}
	
	public static bool operator !=(Flower a, Flower b){
		return !(a == b);
	}
	
	public override string ToString(){
		return this.date + " / " + this.message;
	}
}