using IniParser.Model;
using IniParser;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static SystemBot.SystemTools;
using System.Reflection;
using System.Runtime.InteropServices;

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

			AppDomain.CurrentDomain.ProcessExit += OnProcessExit; // Для ProcessExit
			Console.CancelKeyPress += OnCancelKeyPress;          // Для Ctrl+C (SIGINT)

			// Подписываемся на SIGTERM (только для Linux)
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				UnixSignalHandler.Register(Signum.SIGTERM, OnSigTerm);
			}

			logger.Info(configTextDefault);

			logger.Info($"Done!");
			_commands = new Dictionary<string, Func<ITelegramBotClient, Message, Task>>();
			RegisterCommands();

			Host bot = new Host(_token);
			bot.Start();
			bot.OnMessage += OnMessage;

			// Бесконечный цикл для работы демона
			while (true)
			{
				await Task.Delay(1000); // Ожидание 1 секунду
			}

			//await Task.CompletedTask;
		}

		private static HashSet<long> chatIds = new HashSet<long>();
		private static async void OnMessage(ITelegramBotClient client, Update update)
		{
			try
			{
				if (update.Message == null)
				{
					return;
				}


				chatIds.Add(update.Message.Chat.Id);
				if (update.Message.Text != null && _commands.TryGetValue(update.Message.Text, out var commandHandler))
				{
					if (!isShutdowned)
					{           
						// Проверяем, есть ли команда в словаре
						await commandHandler(client, update.Message);
					}
					else
					{
						await client.SendMessage(update.Message.Chat.Id, "Нельзя исполнить команду! Сервер в процессе выключения/перезагрузки!");
						logger.Warn($"Вызов команды отменен! Сервер в процессе выключения/перезагрузки.");
					}
				}
			}
			catch (Exception ex)
			{
				if (update.Message == null)
				{
					return;
				}

				await client.SendMessage(update.Message.Chat.Id, "Ошибка! Кажется что-то пошло не так, попробуйте позже.\nКлавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());
				logger.Error($"Ошибка в время обработки сообщения: {ex.Message}");
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
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
		public class CommandAttribute : Attribute
		{
			public string Name { get; }

			public CommandAttribute(string name)
			{
				Name = name;
			}
		}

		[Command("/sys_start")]
		[Command("/sys_start@system_ultra_bot")]
		public static async Task HandleStartCommand(ITelegramBotClient client, Message message)
		{
			if (message != null && message.From != null)
			{
				if (message.From.Id == ADMIN_ID)
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

					await client.SendMessage(message.Chat.Id, "Выберите действие:", replyMarkup: replyAdminKeyboard, replyParameters: message.Id);
				}
				else
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

					await client.SendMessage(message.Chat.Id, "Выберите действие:", replyMarkup: replyKeyboard, replyParameters: message.Id);
				}
			}
		}

		[Command("Загрузка CPU")]
		public static async Task CpuLoadOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double cpuLoad = Math.Round(systemTools.GetCpuLoad(), 0);
			await client.SendMessage(message.Chat.Id, $"CPU загружен на {cpuLoad}%.");
		}

		[Command("Температура CPU")]
		public static async Task CpuTempOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double cpuTemp = Math.Round(systemTools.GetCpuTemperature(), 0);
			await client.SendMessage(message.Chat.Id, $"Температура CPU составляет {cpuTemp}°C.");
		}

		[Command("Загрузка RAM")]
		public static async Task RamUsageOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double ramUsage = Math.Round(systemTools.GetRamUsage(), 0);
			await client.SendMessage(message.Chat.Id, $"RAM загружен на {ramUsage}%.");
		}

		[Command("Загрузка диска")]
		public static async Task DiskUsageOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			double diskUsage = Math.Round(systemTools.GetDiskUsage(), 0);
			await client.SendMessage(message.Chat.Id, $"Диск загружен на {diskUsage}%.");
		}

		[Command("Статус VPN сервера")]
		public static async Task VPNStatusOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			string serviceName = "openvpn-server@server.service";
			DataUnit dataUnit = systemTools.GetServiceStatus(serviceName);
			await client.SendMessage(message.Chat.Id, $"{dataUnit}.");
		}

		[Command("Статус TeamSpeak сервера")]
		public static async Task TeamSpeakStatusOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools();
			string serviceName = "teamspeak.service";
			DataUnit dataUnit = systemTools.GetServiceStatus(serviceName);
			await client.SendMessage(message.Chat.Id, $"{dataUnit}.");
		}

		private static bool isShutdowned = false;
		[Command("Перезагрузка сервера")]
		public static async Task RestartServerOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools(false);
			int delayMinutesRestart = 5;

			foreach (long id in chatIds)
			{
				await client.SendMessage(id, $"Сервер перезагрузится через {delayMinutesRestart} минут.\nКлавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());
			}
			isShutdowned = true;
			systemTools.RestartServer(delayMinutesRestart);
		}

		[Command("Выключение сервера")]
		public static async Task ShutdownServerOperation(ITelegramBotClient client, Message message)
		{
			SystemTools systemTools = new SystemTools(false);
			int delayMinutesShutDown = 5;

			foreach (long id in chatIds)
			{
				await client.SendMessage(id, $"Сервер выключится через {delayMinutesShutDown} минут.\nКлавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove());
			}
			isShutdowned = true;
			systemTools.ShutdownServer(delayMinutesShutDown);
		}

		[Command("Выход")]
		public static async Task HandleRemoveKeyboard(ITelegramBotClient client, Message message)
		{
			await client.SendMessage(message.Chat.Id, "Выход\nКлавиатура скрыта.", replyMarkup: new ReplyKeyboardRemove() { Selective = true }, replyParameters: message.Id);
		}

		//----------------------------------------- SYSTEM -----------------------------------------

		public static async Task RemoveKeyboardForAll(ITelegramBotClient client)
		{
			foreach (long id in chatIds)
			{
				await client.SendMessage(id, "Бот выключается.", replyMarkup: new ReplyKeyboardRemove());
			}
		}

		private static bool _isExiting = false; // Флаг для отслеживания состояния завершения
		private static readonly object _lock = new object(); // Объект для блокировки
		private static async void OnProcessExit(object? sender, EventArgs e)
		{
			lock (_lock)
			{
				if (_isExiting) return; // Если уже завершаемся, выходим
				_isExiting = true; // Устанавливаем флаг
			}

			logger.Info("Обработчик ProcessExit: завершение работы...");

			try
			{
				if (Host.BotClient != null)
					await RemoveKeyboardForAll(Host.BotClient);
			}
			catch (Exception ex)
			{
				logger.Error($"Ошибка завершения работы: {ex.Message}");
			}
			finally
			{
				Environment.Exit(0); // Завершаем программу
			}
		}

		private static async void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
		{
			lock (_lock)
			{
				if (_isExiting) return; // Если уже завершаемся, выходим
				_isExiting = true; // Устанавливаем флаг
			}

			logger.Info("Обработчик Ctrl+C (SIGINT): завершение работы...");
			e.Cancel = true; // Предотвращаем завершение процесса по умолчанию

			try
			{
				if (Host.BotClient != null)
					await RemoveKeyboardForAll(Host.BotClient);
			}
			catch (Exception ex)
			{
				logger.Error($"Ошибка завершения работы: {ex.Message}");
			}
			finally
			{
				Environment.Exit(0); // Завершаем программу
			}
		}

		private static async void OnSigTerm()
		{
			lock (_lock)
			{
				if (_isExiting) return; // Если уже завершаемся, выходим
				_isExiting = true; // Устанавливаем флаг
			}

			logger.Info("Обработчик SIGTERM: завершение работы...");

			try
			{
				if (Host.BotClient != null)
					await RemoveKeyboardForAll(Host.BotClient);
			}
			catch (Exception ex)
			{
				logger.Error($"Ошибка завершения работы: {ex.Message}");
			}
			finally
			{
				Environment.Exit(0); // Завершаем программу
			}
		}
	}
}