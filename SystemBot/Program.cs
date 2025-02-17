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
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace SystemBot
{
	internal class Program
	{
		private static readonly string moduleName = "Program";
		private static readonly Logger baseLogger = LogManager.GetLogger(moduleName);
		private static readonly LoggerManager logger = new(baseLogger, moduleName);

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

		private static Dictionary<string, Func<ITelegramBotClient, Message, Task>> _commands = [];
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
		}
		static async Task Main(string[] args)
		{
			logger.Info($"Starting...");
			initConfig();
			logger.Info(configTextDefault);

			logger.Info($"Done!");
			_commands = new Dictionary<string, Func<ITelegramBotClient, Message, Task>>();
			RegisterCommands();

			Host bot = new Host(_token);
			bot.Start();
			bot.OnMessage += OnMessage;

			Console.ReadKey();
			await Task.CompletedTask;
		}
		private static async void OnMessage(ITelegramBotClient client, Update update)
		{
			RegisterCommands();
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

		public static void RegisterCommands()
		{
			var methods = Assembly.GetExecutingAssembly()
				.GetTypes()
				.SelectMany(t => t.GetMethods())
				.Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0);

			foreach (var method in methods)
			{
				var attribute = (CommandAttribute)method.GetCustomAttributes(typeof(CommandAttribute), false).First();
				_commands[attribute.Name] = (Func<ITelegramBotClient, Message, Task>)Delegate.CreateDelegate(typeof(Func<ITelegramBotClient, Message, Task>), method);
			}
		}

		// Атрибут для пометки методов-обработчиков
		[AttributeUsage(AttributeTargets.Method)]
		public class CommandAttribute : Attribute
		{
			public string Name { get; }

			public CommandAttribute(string name)
			{
				Name = name;
			}
		}

		[Command("/start")]
		public static async Task HandleStartCommand(ITelegramBotClient client, Message message)
		{
			if (message?.From?.Id == ADMIN_ID)
			{
				var replyAdminKeyboard = new ReplyKeyboardMarkup(
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
					new KeyboardButton("Статус VPN сервера"),
					new KeyboardButton("Статус TeamSpeak сервера"),
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
					Selective = true,
					ResizeKeyboard = true,
				};

				await client.SendMessage(message.Chat.Id, "Здравствуйте! Выберите действие:", replyMarkup: replyAdminKeyboard, replyParameters: message.Id);
			}
			else if (message?.From?.Id != ADMIN_ID)
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
					new KeyboardButton("Статус VPN сервера"),
					new KeyboardButton("Статус TeamSpeak сервера"),
				},
				new KeyboardButton[]
				{
					new KeyboardButton("Выход")
				}
				})
				{
					Selective = true,
					ResizeKeyboard = true,
				};

				await client.SendMessage(message.Chat.Id, "Здравствуйте! Выберите действие:", replyMarkup: replyKeyboard, replyParameters: message.Id);
			}
		}

		[Command("Загрузка CPU")]
		public static async Task CpuLoadOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double cpuLoad = Math.Round(systemTools.GetCpuLoad(), 0);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Загрузка CPU\nCPU загружен на {cpuLoad}%.");
		}

		[Command("Температура CPU")]
		public static async Task CpuTempOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double cpuTemp = Math.Round(systemTools.GetCpuTemperature(), 0);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Температура CPU\nТемпература CPU составляет {cpuTemp}°C.");
		}

		[Command("Загрузка RAM")]
		public static async Task RamUsageOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double ramUsage = Math.Round(systemTools.GetRamUsage(), 0);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Загрузка RAM\nCPU загружен на {ramUsage}%.");
		}

		[Command("Загрузка диска")]
		public static async Task DiskUsageOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double diskUsage = Math.Round(systemTools.GetDiskUsage(), 0);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Загрузка диска\nДиск загружен на {diskUsage}%.");
		}

		[Command("Статус VPN сервера")]
		public static async Task VPNStatusOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			string serviceName = string.Empty;
			DataUnit dataUnit = systemTools.GetServiceStatus(serviceName);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Статус VPN сервера\nСтатус VPN сервера: {dataUnit}.");
		}

		[Command("Статус TeamSpeak сервера")]
		public static async Task TeamSpeakStatusOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			string serviceName = string.Empty;
			DataUnit dataUnit = systemTools.GetServiceStatus(serviceName);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Статус TeamSpeak сервера\nСтатус TeamSpeak сервера: {dataUnit}.");
		}

		[Command("Перезагрузка сервера")]
		public static async Task RestartServerOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			int delaySecondsRestart = 300;
			systemTools.RestartServer(delaySecondsRestart);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Перезагрузка сервера\nСервер перезагружается через {delaySecondsRestart} секунд.");
		}

		[Command("Выключение сервера")]
		public static async Task ShutdownServerOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			int delaySecondsShutDown = 300;
			systemTools.ShutdownServer(delaySecondsShutDown);
			await client.SendMessage(message.Chat.Id, $"Вы выбрали Выключение сервера\nСервер выключается через {delaySecondsShutDown} секунд\nКлавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());
		}

		[Command("Выход")]
		public static async Task HandleRemoveKeyboard(ITelegramBotClient client, Message message)
		{
			await client.SendMessage(message.Chat.Id, "Выход\nКлавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove() { Selective = true }, replyParameters: message.Id);
		}
	}
}