using NLog;
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
		public static TelegramBotClient? BotClient
		{
			get
			{
				return _botClient;
			}
			//set
			//{
			//	//_botClient = value;
			//}
		}

		public Host(string _token) 
		{
			_botClient = new TelegramBotClient(_token);
		}

		public void Start()
		{
			_botClient?.StartReceiving(UpdateHandler, ErrorHandler);
			logger.Info("Бот запущен"); 
		}

		private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
		{
			logger.Info("Ошибка: " + exception.Message); 
			await Task.CompletedTask;
		}

		private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
		{
			logger.Info($"Пришло сообщение: {update.Message?.Text ?? "[не текст]"}");
			OnMessage?.Invoke(client, update);
			await Task.CompletedTask;
		}

	}
}
