using IniParser.Model;
using IniParser;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System;

namespace SystemBot
{
	internal class Program
	{

        private static readonly string moduleName = "SystemBot.Program";
        private static readonly Logger baseLogger = LogManager.GetLogger(moduleName);
        private static readonly LoggerManager logger = new LoggerManager(baseLogger, moduleName);

        private static string _token = string.Empty;
        public static string TOKEN
        {
            get
            {
                return _token;
            }
        }
        public static int EXAMPLE1 { get; set; } = 6;

        private const string filePathConfig = "config.ini";
        private static string configTextDefault = string.Empty;
        private static void initConfig()
        {
            FileIniDataParser parser = new FileIniDataParser();

            if (File.Exists(filePathConfig))
            {
                logger.Info($"Чтение конфигурационного файла.");

                IniData data = parser.ReadFile(filePathConfig);
                _token = data["Settings"]["TOKEN"];
                EXAMPLE1 = Convert.ToInt32(data["Settings"]["EXAMPLE1"]);
            }
            else
            {
                logger.Info($"Создание конфигурационного файла.");

                IniData data = new IniData();
                data.Sections.AddSection("Settings");
                data["Settings"]["TOKEN"] = _token.ToString();
                data["Settings"]["EXAMPLE1"] = EXAMPLE1.ToString();
                parser.WriteFile(filePathConfig, data);
            }

            configTextDefault = $"TOKEN = [{_token}]\r\n" +
                                $"EXAMPLE1 = [{EXAMPLE1}]";
        }

        
        static async Task Main(string[] args)
		{
			logger.Info($"Starting...");
			initConfig();
			logger.Info(configTextDefault);
            
			logger.Info($"Done!");

            Host bot = new Host(_token);
            bot.Start();
            bot.OnMessage += OnMessage;

            Console.ReadLine();

            await Task.CompletedTask;
        }

        private static async void OnMessage(ITelegramBotClient client, Update update)
        {
            if (update.Message?.Text == "/start")
            {
                await client.SendMessage(update.Message.Chat.Id, "Здравствуйте, это бля бот!");
                var replyKeyboard = new ReplyKeyboardMarkup(true).AddButton("Убрать клавиатуру");

                await client.SendMessage(update.Message.Chat.Id, "Выберите действие", replyMarkup: replyKeyboard);

            }
            else if (update.Message?.Text == "Убрать клавиатуру")
            {
                await client.SendMessage(update.Message.Chat.Id, "Клавиатура убрана", replyMarkup: new ReplyKeyboardRemove()).ConfigureAwait(false);
            }
            
        }
    }
}
