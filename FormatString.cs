using System;
using System.Runtime.InteropServices;
using AshLib;
using System.Text;

public class CharFormat{
	public byte? density{get;} //0 is normal, 1 is bold, 2 is thin
	public bool? italic{get;} 
	public byte? underline{get;}  //0 is not, 1 is single, 2 is double
	public bool? strikeThrough{get;} 
	
	public Color3? foreground{get;} //null is skip
	public bool? foregroundReset{get;}
	public Color3? background{get;} //null is skip
	public bool? backgroundReset{get;}
	
	public CharFormat(byte? b1, bool? b2, byte? b3, bool? b4, Color3? c1, bool? b5, Color3? c2, bool? b6){
		density = b1;
		italic = b2;
		underline = b3;
		strikeThrough = b4;
		foreground = c1;
		foregroundReset = b5;
		background = c2;
		backgroundReset = b6;
	}
	
	public CharFormat(Color3? c1, bool b5, Color3? c2, bool b6){
		density = null;
		italic = null;
		underline = null;
		strikeThrough = null;
		foreground = c1;
		foregroundReset = b5;
		background = c2;
		backgroundReset = b6;
	}
	
	public CharFormat(Color3? c1, bool b5){
		density = null;
		italic = null;
		underline = null;
		strikeThrough = null;
		foreground = c1;
		foregroundReset = b5;
		background = null;
		backgroundReset = false;
	}
	
	public CharFormat(){
		density = null;
		italic = null;
		underline = null;
		strikeThrough = null;
		foreground = null;
		foregroundReset = false;
		background = null;
		backgroundReset = false;
	}
	
	public override string ToString(){
		return "#CharFormat# Density: " + (density == null ? "Default" : (density == 0 ? "Normal" : (density == 1 ? "Bold" : (density == 2 ? "Thin" : "Invalid or Unknown"))))
		+ " Italic: " + (italic == null ? "Default" : (italic == true ? "Activated" : "Deactivated"))
		+ " Underlined: " + (underline == null ? "Default" : (underline == 0 ? "Normal" : (underline == 1 ? "Single" : (underline == 2 ? "Double" : "Invalid or Unknown"))))
		+ " Strike-Through: " + (strikeThrough == null ? "Default" : (strikeThrough == true ? "Activated" : "Deactivated"))
		+ " Foreground Color: " + (foreground == null ? "Default" : (foregroundReset == true ? "Reset" : foreground.ToString()))
		+ " Background Color: " + (background == null ? "Default" : (foregroundReset == true ? "Reset" : background.ToString()));
	}
	
	public static bool operator ==(CharFormat a, CharFormat b){
		// Check for null references
		if (ReferenceEquals(a, b)) return true;
		if (a is null && b is null) return true;
		if (a is null || b is null) return false;

		// Compare all properties
		return a.density == b.density &&
			a.italic == b.italic &&
			a.underline == b.underline &&
			a.strikeThrough == b.strikeThrough &&
			a.foreground == b.foreground &&
			a.background == b.background;
    }
	
	public static bool operator !=(CharFormat a, CharFormat b){
        return !(a==b);
    }
}

public class FormatString{

    private const int  STD_OUTPUT_HANDLE                  	= -11;
    private const uint ENABLE_PROCESSED_OUTPUT            = 0x0001;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
	
	[DllImport("kernel32")]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32")]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	[DllImport("kernel32")]
	private static extern IntPtr GetStdHandle(int nStdHandle);
	
	//================================
	
	private static bool hasConsoleBeenPrepared = false;
	
	public bool addFinalReset = true;
	
	public string content{get{
		return new string(privateContent.ToArray());
	}}
	private List<char> privateContent{get;}
	public List<CharFormat?> format{get;}
	
	private string _built;
	public string built {get{
		if(!hasConsoleBeenPrepared){
			PrepareConsole();
			hasConsoleBeenPrepared = true;
		}
		
		if(flagToBuild){
			Build();
		}
		return _built;
	}}
	
	public int length{get{
		return privateContent.Count;
	}}
	
	private bool flagToBuild;
	
	public FormatString(){
		privateContent = new List<char>();
		format = new List<CharFormat?>();
	}
	
	private void Build(){
		CharFormat lastFormat = new CharFormat();
		StringBuilder final = new StringBuilder();
		
		if(privateContent.Count > format.Count){
			_built = new string(privateContent.ToArray());
			flagToBuild = false;
			return;
		}
		
		for(int i = 0; i < privateContent.Count; i++){
			CharFormat current = format[i];
			
			if(current == null){
				lastFormat = current;
				final.Append(privateContent[i]);
				continue;
			}
			
			if(current == lastFormat){
				lastFormat = current;
				final.Append(privateContent[i]);
				continue;
			}
			
			bool formatChanges = false;
			List<string> changes = new List<string>();
			
			if(lastFormat == null || current.density != lastFormat.density){
				switch(current.density){
					case 0:
					formatChanges = true;
					changes.Add("22");
					break;
					
					case 1:
					formatChanges = true;
					changes.Add("1");
					break;
					
					case 2:
					formatChanges = true;
					changes.Add("2");
					break;
				}
			}
			
			if(lastFormat == null || current.italic != lastFormat.italic){
				switch(current.italic){
					case true:
					formatChanges = true;
					changes.Add("3");
					break;
					
					case false:
					formatChanges = true;
					changes.Add("23");
					break;
				}
			}
			
			if(lastFormat == null || current.underline != lastFormat.underline){
				switch(current.underline){
					case 0:
					formatChanges = true;
					changes.Add("24");
					break;
					
					case 1:
					formatChanges = true;
					changes.Add("4");
					break;
					
					case 2:
					formatChanges = true;
					changes.Add("21");
					break;
				}
			}
			
			if(lastFormat == null || current.strikeThrough != lastFormat.strikeThrough){
				switch(current.strikeThrough){
					case true:
					formatChanges = true;
					changes.Add("9");
					break;
					
					case false:
					formatChanges = true;
					changes.Add("29");
					break;
				}
			}
			
			if(lastFormat == null || (current.foreground != lastFormat.foreground && current.foreground != null) || current.foregroundReset == true){
				if(current.foregroundReset == true){
					formatChanges = true;
					changes.Add("39");
				}else if(current.foreground != null){
					formatChanges = true;
					changes.Add("38");
					changes.Add("2");
					Color3 foregroundColor = current.foreground.Value;
					changes.Add(foregroundColor.R.ToString());
					changes.Add(foregroundColor.G.ToString());
					changes.Add(foregroundColor.B.ToString());
				}
			}
			
			if(lastFormat == null || (current.background != lastFormat.background && current.background != null) || current.backgroundReset == true){
				if(current.backgroundReset == true){
					formatChanges = true;
					changes.Add("49");
				}else if(current.background != null){
					formatChanges = true;
					changes.Add("48");
					changes.Add("2");
					Color3 backgroundColor = current.background.Value;
					changes.Add(backgroundColor.R.ToString());
					changes.Add(backgroundColor.G.ToString());
					changes.Add(backgroundColor.B.ToString());
				}
			}
			lastFormat = current;
			
			if(formatChanges){
				final.Append("\x1b[");
				for(int j = 0; j < changes.Count; j++){
					final.Append(changes[j]);
					if(j != changes.Count - 1){
						final.Append(";");
					}
				}
				final.Append("m");
			}
			final.Append(privateContent[i]);
		}
		if(addFinalReset){
			final.Append("\x1b[0m");
		}
		flagToBuild = false;
		_built = final.ToString();
	}
	
	public void Append(string s, CharFormat? f){
		privateContent.AddRange(s.ToCharArray());
		for(int i = 0; i < s.Length; i++){
			format.Add(f);
		}
		flagToBuild = true;
	}
	
	public void Append(object s, CharFormat? f){
		string o = s.ToString();
		privateContent.AddRange(o.ToCharArray());
		for(int i = 0; i < o.Length; i++){
			format.Add(f);
		}
		flagToBuild = true;
	}
	
	public void Append(string s, CharFormat?[] f){
		if(s.Length != f.Length){
			return;
		}
		privateContent.AddRange(s.ToCharArray());
		for(int i = 0; i < s.Length; i++){
			format.Add(f[i]);
		}
		flagToBuild = true;
	}
	
	public void Append(FormatString f){
		privateContent.AddRange(f.privateContent.ToArray());
		format.AddRange(f.format.ToArray());
		flagToBuild = true;
	}
	
	public void Append(string s, params object[] objs){
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < s.Length; i++){
			if(s[i] == '{' && s[i+1] == '$'){
				int x = i + 2;
				string n = UntilNextCurlyBrace(s, ref x);
				int k;
				if(int.TryParse(n, out k) && k > -1 && objs != null && k < objs.Length){
					sb.Append(objs[k].ToString());
					i = x - 1;
				}
			}else{
				sb.Append(s[i]);
			}
		}
		Append(sb.ToString());
		flagToBuild = true;
	}
	
	public void Append(string s){
		CharFormat lastFormat = null;
		for(int i = 0; i < s.Length; i++){
			if(s[i] == '/' && s[i+1] == '['){
				int x = i;
				x += 2;
				
				CharFormat f = new CharFormat();
				bool hasChanged = false;
				
				while(true){
					bool increment = false;
					switch(UntilNextComma(s, ref x).ToUpper()){
						case "B":
						f = new CharFormat(1, f.italic, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						hasChanged = true;
						increment = true;
						break;
						
						case "T":
						f = new CharFormat(2, f.italic, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						hasChanged = true;
						increment = true;
						break;
						
						case "RT":
						case "RD":
						case "D":
						f = new CharFormat(0, f.italic, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						hasChanged = true;
						increment = true;
						break;
						
						case "I":
						f = new CharFormat(f.density, true, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "RI":
						f = new CharFormat(f.density, false, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "U":
						f = new CharFormat(f.density, f.italic, 1, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "DU":
						f = new CharFormat(f.density, f.italic, 2, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "RU":
						f = new CharFormat(f.density, f.italic, 0, f.strikeThrough, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "S":
						f = new CharFormat(f.density, f.italic, f.underline, true, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "RS":
						f = new CharFormat(f.density, f.italic, f.underline, false, f.foreground, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "C":
						case "F":
						byte r;
						if(!byte.TryParse(UntilNextComma(s, ref x), out r)){
							break;
						}
						byte g;
						if(!byte.TryParse(UntilNextComma(s, ref x), out g)){
							break;
						}
						byte b;
						if(!byte.TryParse(UntilNextComma(s, ref x), out b)){
							break;
						}
						f = new CharFormat(f.density, f.italic, f.underline, f.strikeThrough, new Color3(r, g, b), f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "C#":
						case "F#":
						string h = UntilNextComma(s, ref x);
						Color3 v = new Color3(h);
						f = new CharFormat(f.density, f.italic, f.underline, f.strikeThrough, v, f.foregroundReset, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "RC":
						case "RF":
						f = new CharFormat(f.density, f.italic, f.underline, f.strikeThrough, f.foreground, true, f.background, f.backgroundReset);
						increment = true;
						break;
						
						case "BG":
						if(!byte.TryParse(UntilNextComma(s, ref x), out r)){
							break;
						}
						if(!byte.TryParse(UntilNextComma(s, ref x), out g)){
							break;
						}
						if(!byte.TryParse(UntilNextComma(s, ref x), out b)){
							break;
						}
						f = new CharFormat(f.density, f.italic, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, new Color3(r, g, b), f.backgroundReset);
						increment = true;
						break;
						
						case "BG#":
						h = UntilNextComma(s, ref x);
						v = new Color3(h);
						f = new CharFormat(f.density, f.italic, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, v, f.backgroundReset);
						increment = true;
						break;
						
						case "RB":
						f = new CharFormat(f.density, f.italic, f.underline, f.strikeThrough, f.foreground, f.foregroundReset, f.background, true);
						increment = true;
						break;
						
						case "0":
						f = new CharFormat(0, false, 0, false, null, true, null, true);
						increment = true;
						break;
						
					}
					
					if(increment){
						hasChanged = true;
					}
					
					if(s[x] == ']'){
						break;
					}
				}
				
				if(hasChanged){
					x++;
					
					i = x;
					lastFormat = f;
				}
			}
			if(i > s.Length - 1){
				continue;
			}
			privateContent.Add(s[i]);
			format.Add(lastFormat);
		}
		flagToBuild = true;
	}
	
	private string UntilNextComma(string s, ref int index){
		StringBuilder sb = new StringBuilder();
		int ind = index;
		while(true){
			if(ind > s.Length - 1){
				return sb.ToString();
			}
			if(s[ind] == ','){
				ind++;
				index = ind;
				return sb.ToString();
			}
			if(s[ind] == ']'){
				index = ind;
				return sb.ToString();
			}
			sb.Append(s[ind]);
			ind++;
		}
	}
	
	private string UntilNextCurlyBrace(string s, ref int index){
		StringBuilder sb = new StringBuilder();
		int ind = index;
		while(true){
			if(ind > s.Length - 1){
				return sb.ToString();
			}
			if(s[ind] == '}'){
				ind++;
				index = ind;
				return sb.ToString();
			}
			sb.Append(s[ind]);
			ind++;
		}
	}
	
	public void DeleteFromStart(int n){
		if(n > length){
			return;
		}
		privateContent.RemoveRange(0, n);
		format.RemoveRange(0, n);
		flagToBuild = true;
	}
	
	public void DeleteFromEnd(int n){
		if(n > length){
			return;
		}
		
		int l = length;
		
		privateContent.RemoveRange(l - n, n);
		format.RemoveRange(l - n, n);
		flagToBuild = true;
	}
	
	public void Delete(int n, int l){
		if(l > length){
			return;
		}
		privateContent.RemoveRange(n, l);
		format.RemoveRange(n, l);
		flagToBuild = true;
	}
	
	private static void PrepareConsole(){
		var iStdOut =   GetStdHandle(STD_OUTPUT_HANDLE);
        var _ =    GetConsoleMode(iStdOut, out var outConsoleMode)
        && SetConsoleMode(iStdOut, outConsoleMode | ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
	}
	
	public override string ToString(){
		return built;
	}
	
	public static FormatString operator + (FormatString a, FormatString b){
		FormatString c = new FormatString();
		
		c.Append(a);
		c.Append(b);
		
		return c;
	}
	
	public static FormatString operator + (string a, FormatString b){
		FormatString c = new FormatString();
		
		c.Append(a);
		c.Append(b);
		
		return c;
	}
	
	public static FormatString operator + (FormatString a, string b){
		FormatString c = new FormatString();
		
		c.Append(a);
		c.Append(b);
		
		return c;
	}
	
	public static FormatString operator + (char a, FormatString b){
		FormatString c = new FormatString();
		
		c.Append(a, null);
		c.Append(b);
		
		return c;
	}
	
	public static FormatString operator + (FormatString a, char b){
		FormatString c = new FormatString();
		
		c.Append(a);
		c.Append(b, null);
		
		return c;
	}
	
	public static implicit operator FormatString(string s){
		FormatString c = new FormatString();
		
		c.Append(s);
		
		return c;
	}
	
	public static void test(){
		string t = "/[c#,ff5900]";
		FormatString s = new FormatString();
		s.Append("{$0}Ki/[bg,100,0,0,rc]no", t);
		Console.WriteLine(s);
	}
}