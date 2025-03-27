# SystemBot

[Russian version](README_RU.md)

---

SystemBot is a bot written using Telegram API Core. It's designed for use on Linux operating systems. The bot allows you to view basic system (server) information, additional information (optional), and interact with the system (server).

## Installation and Setup

1. Before starting, you need to message [BotFather](https://t.me/botfather) on Telegram. Create a new bot and obtain the token for your created bot.
2. Run `initialization.sh` to create a daemon that will run the bot. You can manage the service status using `systemctl`.
3. After the first launch, the program will create a `config.ini` file in the root directory.
4. In the `config.ini` file, you need to specify the following:
   - The token of your created bot
   - User IDs that will have access to administrative functions
   Additionally:
   - The time interval (in hours) for automatic system information updates (see below).
5. After configuring `config.ini`, restart the application.
6. If the bot is no longer needed, use `stop_and_remove_service.sh`, then clean the bot's executable directory.

## Features

### Core Features

- **CPU Load**: Shows CPU usage from 0 to 100% with tenths precision.
- **CPU Temperature**: Shows CPU temperature in Celsius.
- **RAM Usage**: Shows RAM usage from 0 to 100% with hundredths precision.
- **Disk (SSD) Usage**: Shows disk usage from 0 to 100%.

### Administrative Features

- **Reboot**: Initiates system (server) reboot. After command execution, the bot will send a message about the upcoming reboot 5 minutes before execution, along with all current system information. After 5 minutes, the bot will send a message about system startup.
- **Shutdown**: Initiates system (server) shutdown. After command execution, the bot will send a message about the upcoming shutdown 5 minutes before execution, along with all current system information. After 5 minutes, the bot will send a message about system shutdown.
- **Delete Chats File**: Deletes the file containing all chat IDs where the bot is used.

### Additional Features

- **VPN Server Status**: Shows VPN server status (if available).
- **TeamSpeak Server Status**: Shows TeamSpeak server status (if available).

The bot can also display additional information:
- **Uptime**: Shows system uptime in hours and minutes.
- **Date**: Shows date in day/month/year format.
- **Day of Week**: Shows current day of week.
- **Day Progress**: Shows day progress from 0 to 100% with hundredths precision.

### Example Information Output

```
Uptime: up 14 hours, 36 minutes
CPU Load: 57.8%
CPU Temperature: 28°C
RAM Usage: 3.95%
DISK Usage: 10%
Date: March 23, 2025
Day of Week: Sunday
Day Progress: 29.5%
```

## Usage Example

1. Launch the application and configure `config.ini`.
2. Restart the application.
3. Go to Telegram and open a chat with your bot.
4. Type the `/start` command to open the keyboard according to your access level.
6. Click the **Exit** button to close the keyboard.

## License

MIT License.

## Donate

[Thank you very much!❤️](https://boosty.to/naillin/donate)