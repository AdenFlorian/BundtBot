# BundtBot
### Introduction
Hi I am Bundt

### Setup
Create a file called "BotToken.txt" in the keys folder (create it in project root if it doesn't exist), and put your bot token in there. Go here to get a bot token: https://discordapp.com/developers/applications/me. Also, bot tokens need a `Bot ` prefix now. Here is an example of what should be in your BotToken.txt:
```
Bot zTg4zzzzzDI3OTYzNDc2ODY0.zzzzzg.fpzzztS5d6y9YhaQFiczzzddzyI
```

### Dependencies
Download the following binaries and place them in the BundtBot project root (the folder with BundtBot.csproj in it):
- https://www.ffmpeg.org/download.html
  - ffmpeg.exe
  - ffplay.exe
  - ffprobe.exe
- https://download.libsodium.org/libsodium/releases/
  - libsodium.dll
- https://www.opus-codec.org/downloads/
  - opus.dll
- https://rg3.github.io/youtube-dl/download.html
  - youtube-dl.exe

### Configuration
There some settings you can configure in the App.config:
- CommandPrefix: Will determine the prefix used for commands.

### Functions
TODO

### Joining servers
https://discordapp.com/oauth2/authorize?&client_id=BOT_CLIENT_ID&scope=bot&permissions=70376448
