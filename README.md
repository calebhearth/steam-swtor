## Installation

Build Program.cs one of two ways:

### Average Joe

1. Copy the contents of
   [Program.cs](https://raw.github.com/calebthompson/steam-swtor/master/Program.cs)
   into a text file called `Star Wars - The Old Republic.txt` on your
   desktop.
2. Open a command prompt
     `Start Menu -> All Programs -> Accessories -> Command Prompt`
3. Copy/Paste the following command into the Command Prompt and hit enter.
   The code will take the contents of the text file you pasted and pass
   them into the C# (C Sharp) compiler on your system.  It will create an
   exe file, `Star Wars - The Old Republic.exe` for you.

         %windir%\Microsoft.Net\Framework\v3.5\csc /platform:x86 ^
         "/out:%USERPROFILE%\Desktop\Star Wars - The Old Republic.exe" ^
         "%USERPROFILE%\Desktop\Star Wars - The Old Republic.txt"

4. Copy `Star Wars - The Old Republic.exe` from your desktop into SWToR's
   install folder.  It is likely located at
    `C:\Program Files (x86)\Electronic Arts\BioWare\Star Wars - The Old Republic\`
5. Go into Steam and add the executable you just made to your Steam Library.
   Do this just like adding any other game, except that you will probably have to
   browse for it.
    
### Programmer

1. There's a whole Visual Studio 2010 solution here.
2. You are a programmer.  Figure it out.

## Problems

If things don't seem to be working right, please file an
[Issue](https://github.com/calebthompson/steam-swtor/issues/new).

