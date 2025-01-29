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
        public Action<ITelegramBotClient, Update>? OnMessage;

        private static TelegramBotClient? _botClient;

        public Host(string _token) 
        {
            _botClient = new TelegramBotClient(_token);
        }

        public void Start()
        {
            _botClient?.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("Бот запущен");
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine("Ошибка: " + exception.Message);
            await Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            Console.WriteLine($"Пришло сообщение: {update.Message?.Text ?? "[не текст]"}");
            OnMessage?.Invoke(client, update);
            await Task.CompletedTask;
        }
    }
}
