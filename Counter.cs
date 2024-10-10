using System;
using AshLib;

class Counter{
	private Dependencies dep;
	private Garden gar;
	private AshFile counter;
	
	private ulong count;
	
	public Counter(Dependencies d, Garden g){
		this.dep = d;
		this.gar = g;
		initialize();
	}
	
	private void initialize(){
		counter = this.dep.ReadAshFile("22counter.ash");
		
		counter.InitializeCamp("numberOf22", (ulong) 0);
		counter.Save();
		
		if(!counter.CanGetCampAsUlong("numberOf22", out count)){
			throw new Exception("Cant get the number of [SPECIAL NUMBER]");
		}
	}
	
	private void save(){
		counter.SetCamp("numberOf22", count);
		counter.Save();
	}
	
	public void registerInput(string s){
		if(s == "22"){
			gar.writeToConsole("{$0}HEY! Thats a special number! What do you think you are doing?", Palette.SpecialNumber);
			count++;
			save();
			
			if(count % 22 == 0){
				gar.writeToConsole("{$0}Thats like the {$1} number you have typed it...", Palette.SpecialNumber, count);
			}
			
			return;
		}
		ulong c = 0;
		int i = 0;
		
		while ((i = s.IndexOf("22", i)) != -1){
			c++;
			i += 1;
		}
		
		if(c > 0){
			gar.writeToConsole("{$0}HEY! That had {$1} special numbers! What do you think you are doing?", Palette.SpecialNumber, c);
			
			if((count + c) % 22 < count % 22){
				count += c;
				gar.writeToConsole("{$0}Thats like the {$1} number you have typed it...", Palette.SpecialNumber, count);
			}			
			save();
		}
	}
}