using System;

class ConsoleHandler{
	private bool isWriting;
	private bool isPrompt;
	public bool canWrite;
	
	private FormatString input;
	private FormatString prompt;
	
	private int consoleWidth;
	
	private Garden gar;
	
	public ConsoleHandler(Garden g){
		this.gar = g;
		
		this.isWriting = false;
		this.canWrite = false;
		this.input = new FormatString();
		this.prompt = new FormatString();
		this.consoleWidth = Console.WindowWidth;
		
		Thread monitorThread = new Thread(() => monitorConsoleWidth());
        monitorThread.Start();
	}
	
	private int getLinesOfInput(){
		int lines = (int) (input.length - 1) / consoleWidth;
		return lines;
	}
	
	private int getLinesOfPrompt(){
		int lines = (int) (prompt.length - 1) / consoleWidth;
		return lines;
	}
	
	public void write(FormatString s){
		int lines = getLinesOfInput();
		int promptLines = getLinesOfPrompt();
		
		if(isPrompt){
			lines += promptLines + 1;
		}
		
		if(isWriting || isPrompt){
			canWrite = false;
			Console.SetCursorPosition(0, Console.CursorTop-lines);
		}
		
		Console.Write(s);
		
		if(isWriting || isPrompt){
			FormatString r = new FormatString();
			r += new string(' ', consoleWidth - (int) s.length % consoleWidth);
			r += Environment.NewLine;
			
			for(int i = 0; i < lines; i++){
				r += new string(' ', consoleWidth);
				r += Environment.NewLine;
			}
			Console.Write(r);
			
			Console.SetCursorPosition(0, Console.CursorTop - lines);
			
			if(isPrompt){
				Console.Write(prompt + Environment.NewLine);
			}
			if(isWriting){
				Console.Write(input);
			}
			canWrite = true;
		}else{
			Console.WriteLine();
		}
	}
	
	private void read(bool delete, CharFormat? cf){
		if(input.length != 0){
			Console.Write(input);
			this.isWriting = true;
		}
		while(true){
			ConsoleKeyInfo key = Console.ReadKey(intercept: true);
			
			if(!canWrite){
				continue;
			}
			
			if(key.Key == ConsoleKey.Enter){
				if(delete){
					int lines = getLinesOfInput();
					if(isPrompt){
						lines++;
					}
					Console.SetCursorPosition(0, Console.CursorTop - lines);
					
					FormatString r = new FormatString();
					r += new string(' ', consoleWidth);
					r += Environment.NewLine;
					
					for(int i = 0; i < lines; i++){
						r += new string(' ', consoleWidth);
						r += Environment.NewLine;
					}
					
					Console.Write(r);
					
					Console.SetCursorPosition(0, Console.CursorTop - lines - 1);
				}else{
					Console.WriteLine();
				}
				
				isWriting = false;
				
				return;
			} else if(key.Key == ConsoleKey.Backspace && input.length > 0){
				// Handle backspace
				input.DeleteFromEnd(1);
				
				if(Console.CursorLeft != 0){
					Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
					Console.Write(" ");
					Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
				} else if(input.length != 0){
					Console.SetCursorPosition(Console.WindowWidth - 1, Console.CursorTop - 1);
					Console.Write(" ");
					Console.SetCursorPosition(Console.WindowWidth - 1, Console.CursorTop - 1);
				}
			} else if (!char.IsControl(key.KeyChar)){
				isWriting = true;
				input.Append(key.KeyChar, cf);
				FormatString fs = new FormatString();
				fs.Append(key.KeyChar, cf);
				Console.Write(fs);
			}
		}
	}
	
	public void addToInput(FormatString fs){
		input += fs;
	}
	
	public string askQuestion(FormatString question){
		return askQuestion(question, false, null);
	}
	
	public string askQuestion(FormatString question, bool delete, CharFormat? cf){
		prompt = question;
		
		this.write(prompt);
		this.isPrompt = true;
		
		this.read(delete, cf);
		
		this.isPrompt = false;
		
		string s = input.content;
		
		input = new FormatString();
		
		return s;
	}
	
	private void monitorConsoleWidth()
    {
        while (true)
        {
            int newWidth = Console.WindowWidth;
            if (newWidth != consoleWidth)
            {
                consoleWidth = newWidth;
				this.gar.log("The console width changed");
            }

            // Esperar un tiempo antes de volver a comprobar
            Thread.Sleep(200);
        }
    }
}