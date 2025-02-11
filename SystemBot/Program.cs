using IniParser.Model;
using IniParser;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static SystemBot.SystemTools;

namespace SystemBot
{
	internal class Program
	{
		private static readonly string moduleName = "Program";
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
		private static int ADMIN_ID { get; set; } = 0;

		private const string filePathConfig = "config.ini";
		private static string configTextDefault = string.Empty;

		private static Dictionary<string, string> Services = new Dictionary<string, string>();
		private static void initConfig()
		{
			FileIniDataParser parser = new FileIniDataParser();

			if (File.Exists(filePathConfig))
			{
				logger.Info($"Чтение конфигурационного файла.");

				IniData data = parser.ReadFile(filePathConfig);
				_token = data["Settings"]["TOKEN"];
				ADMIN_ID = Convert.ToInt32(data["Settings"]["ADMIN_ID"]);
			}
			else
			{
				logger.Info($"Создание конфигурационного файла.");

				IniData data = new IniData();
				data.Sections.AddSection("Settings");
				data["Settings"]["TOKEN"] = _token.ToString();
				data["Settings"]["ADMIN_ID"] = ADMIN_ID.ToString();

				parser.WriteFile(filePathConfig, data);
			}

			configTextDefault = $"TOKEN = [{_token}]\r\n" +
								$"ADMIN_ID = [{ADMIN_ID}]";

			//запись из JSON массива
			
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

			Console.ReadKey();
			await Task.CompletedTask;
		}

		// Словарь команд и их обработчиков
		private static readonly Dictionary<string, Func<ITelegramBotClient, Message, Task>> _commands = new()
		{
			{ "/start", HandleStartCommand },
			{ "Загрузка CPU", CpuLoadOperation },
			{ "Температура CPU", CpuTempOperation },
			{ "Загрузка RAM", RamUsageOperation },
			{ "Загрузка диска", DiskUsageOperation },
			{ "Статус сервиса", ServiceStatusOperation },
			//{ "Статус сервисов", ServicesStatusOperation },
			{ "Перезагрузка сервера", RestartServerOperation },
			{ "Выключение сервера", ShutdownServerOperation },
			{ "Выход", HandleRemoveKeyboard }
		};

		private static async void OnMessage(ITelegramBotClient client, Update update)
		{
			try
			{
				if (update.Message == null)
				{
					return;
				}
				// Проверяем, есть ли команда в словаре
				if (update.Message.Text != null && _commands.TryGetValue(update.Message.Text, out var commandHandler))
				{
					await commandHandler(client, update.Message);
				}
			}
			catch 
			{
				if (update.Message == null)
				{  
					return; 
				}

				await client.SendMessage(update.Message.Chat.Id, "Ошибка! Возможно сервер не работает или выключен, попробуйте позже.");
				await client.SendMessage(update.Message.Chat.Id, "Клавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());
			}
		}

		// Обработчики команд
		private static async Task HandleStartCommand(ITelegramBotClient client, Message message)
		{
			var replyKeyboard = new ReplyKeyboardMarkup(
			new List<KeyboardButton[]>()
			{
				new KeyboardButton[]
				{
					new KeyboardButton("Загрузка CPU"),
					new KeyboardButton("Температура CPU"),
				},
				new KeyboardButton[]
				{
					new KeyboardButton("Загрузка RAM"),
					new KeyboardButton("Загрузка диска"),
				},
				new KeyboardButton[]
				{
					new KeyboardButton("Статус сервиса"),
					new KeyboardButton("Статус сервисов"),
				},
				new KeyboardButton[]
				{
					new KeyboardButton("Перезагрузка сервера"),
					new KeyboardButton("Выключение сервера"),
				},
				new KeyboardButton[]
				{
					new KeyboardButton("Выход")
				}
			})
			{
				ResizeKeyboard = true,
			};

			await client.SendMessage(message.Chat.Id, "Здравствуйте! Выберите действие:", replyMarkup: replyKeyboard);
		}

		private static async Task CpuLoadOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double cpuLoad = Math.Round(systemTools.GetCpuLoad(), 0);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Загрузка CPU");
			await client.SendMessage(message.Chat.Id, $"CPU загружен на {cpuLoad}%");
		}
		private static async Task CpuTempOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double cpuTemp = Math.Round(systemTools.GetCpuTemperature(), 0);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Температура CPU");
			await client.SendMessage(message.Chat.Id, $"Температура CPU составляет {cpuTemp}°C");
		}
		private static async Task RamUsageOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double ramUsage = Math.Round(systemTools.GetRamUsage(), 0);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Загрузка RAM");
			await client.SendMessage(message.Chat.Id, $"CPU загружен на {ramUsage}%");
		}
		private static async Task DiskUsageOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double diskUsage = Math.Round(systemTools.GetDiskUsage(), 0);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Загрузка диска");
			await client.SendMessage(message.Chat.Id, $"Диск загружен на {diskUsage}%");
		}

		private static async Task ServiceStatusOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			string serviceName = string.Empty;
			DataUnit dataUnit = systemTools.GetServiceStatus(serviceName);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Статус сервиса");
			await client.SendMessage(message.Chat.Id, $"Статус сервиса: {dataUnit}");
		}
		//private static async Task ServicesStatusOperation(ITelegramBotClient client, Message message)
		//{
		//	string[] servicesName = new string[0];
		//	SystemTools systemTools = new SystemTools();
		//	DataUnit[] dataUnits = systemTools.GetServicesStatus(servicesName);
		//	await client.SendMessage(message.Chat.Id, "Вы выбрали Статус сервисов");
		//	await client.SendMessage(message.Chat.Id, $"Статус сервисов: {string.Join(Environment.NewLine, dataUnits)}");
		//}
		private static async Task RestartServerOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			int delaySecondsRestart = 300;
			systemTools.RestartServer(delaySecondsRestart);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Перезагрузка сервера");
			await client.SendMessage(message.Chat.Id, $"Сервер перезагружается через {delaySecondsRestart} секунд");
		}
		private static async Task ShutdownServerOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			int delaySecondsShutDown = 300;
			systemTools.ShutdownServer(delaySecondsShutDown);
			await client.SendMessage(message.Chat.Id, "Вы выбрали Выключение сервера");
			await client.SendMessage(message.Chat.Id, $"Сервер выключается через {delaySecondsShutDown} секунд");
			await client.SendMessage(message.Chat.Id, "Клавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());

		}
		private static async Task HandleRemoveKeyboard(ITelegramBotClient client, Message message)
		{
			await client.SendMessage(message.Chat.Id, "Выход");
			await client.SendMessage(message.Chat.Id, "Клавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());
		}
	}
}