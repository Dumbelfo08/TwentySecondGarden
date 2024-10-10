using System;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Media;
using System.Reflection;
using System.IO;

class NotificationHandler{
	
	// Import the User32.dll for SetForegroundWindow and ShowWindow
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	
	// Import the GetConsoleWindow function to get the handle for the current console window
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();
	
	const int SW_RESTORE = 9;
	
	private Garden gar;
	
	public NotificationHandler(Garden g){
		this.gar = g;
		// Register an event listener for toast activation
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            // Obtain the arguments passed from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
			
			gar.log("Notification clicked");
			
            // Handle the activation logic
            if (args.TryGetValue("action", out string action))
            {
                if (action == "open")
                {
                    IntPtr handle = GetConsoleWindow();
					if (handle != GetForegroundWindow())
					{
						ShowWindow(handle, SW_RESTORE); // Restore the window if minimized
						SetForegroundWindow(handle);    // Bring it to the foreground
					}
					gar.log("Window brought to the front.");
                }
            }
        };
	}
	
	public void showNotification(string text){
		ToastAudio ta = new ToastAudio();
		ta.Silent = true;
		//ta.Src = new Uri("ms-appx:///res/notif.m4a");;
		
		new ToastContentBuilder()
		.AddArgument("action", "open")
		.AddText(text)
		.AddText("Click here to open the app")
		.AddAudio(ta) // Add custom sound
		.Show(toast =>
    {
        toast.ExpirationTime = DateTime.Now.AddHours(1);
    });
	}
	
	
	public void playSound(){
		// Get the current assembly
        var assembly = Assembly.GetExecutingAssembly();

        // Specify the full path to the embedded resource (adjust based on your project structure)
        string resourceName = "TwentySecondGarden.res.notif.wav";

        // Load the embedded sound from the resource stream
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                SoundPlayer player = new SoundPlayer(stream);
                
                // Play the sound asynchronously (won't block the main thread)
                player.Play();
                
                // Optionally, you can wait for input to prevent immediate exit
            }
            else
            {
                gar.log("Sound not found!");
            }
        }
	}
}