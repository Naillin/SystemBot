using IniParser.Model;
using IniParser;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;

namespace SystemBot
{
    public class Host
    {
		private static readonly string moduleName = "Host";
		private static readonly Logger baseLogger = LogManager.GetLogger(moduleName);
		private static readonly LoggerManager logger = new LoggerManager(baseLogger, moduleName);

		public Action<ITelegramBotClient, Update>? OnMessage;

        private static TelegramBotClient? _botClient;

        public Host(string _token) 
        {
            _botClient = new TelegramBotClient(_token);
        }

        public void Start()
        {
            _botClient?.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("Бот запущен"); //Используй логер вместо Console.WriteLine!!!!!!!!!!!!!
		}

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine("Ошибка: " + exception.Message); //Используй логер вместо Console.WriteLine!!!!!!!!!!!!
			await Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            Console.WriteLine($"Пришло сообщение: {update.Message?.Text ?? "[не текст]"}"); //Используй логер вместо Console.WriteLine!!!!!!!!!
			OnMessage?.Invoke(client, update);
            await Task.CompletedTask;
        }
    }
}
