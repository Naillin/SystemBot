﻿using IniParser.Model;
using IniParser;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System;
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

			Console.ReadKey();
			await Task.CompletedTask;
		}

		private static async void OnMessage(ITelegramBotClient client, Update update)
		{
			SystemTools systemTools = new SystemTools();

			string serviceName = "";
			string[] servicesName = new string[0];

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
					new KeyboardButton("Выход")
				}
			})
			{
				ResizeKeyboard = true,
			};
			switch (update.Message?.Text)
			{
				case "/start":
					await client.SendMessage(update.Message.Chat.Id, "Здравствуйте, это бля бот!");

					await client.SendMessage(update.Message.Chat.Id, "Выберите действие", replyMarkup: replyKeyboard);
					break;
				case "Загрузка CPU":
					double cpuLoad = Math.Round(systemTools.GetCpuLoad(), 0);
					await client.SendMessage(update.Message.Chat.Id, Convert.ToString(cpuLoad), replyMarkup: replyKeyboard);
					break;

				case "Температура CPU":
					await client.SendMessage(update.Message.Chat.Id, Convert.ToString(systemTools.GetCpuTemperature()), replyMarkup: replyKeyboard);
					break;

				case "Загрузка RAM":
					await client.SendMessage(update.Message.Chat.Id, Convert.ToString(systemTools.GetRamUsage()), replyMarkup: replyKeyboard);
					break;

				case "Загрузка диска":
					await client.SendMessage(update.Message.Chat.Id, Convert.ToString(systemTools.GetDiskUsage()), replyMarkup: replyKeyboard);
					break;

				case "Статус сервиса":
					await client.SendMessage(update.Message.Chat.Id, systemTools.GetServiceStatus(serviceName).ToString(), replyMarkup: replyKeyboard);
					break;

				case "Статус сервисов":
					DataUnit[] dataUnits = systemTools.GetServicesStatus(servicesName);
					await client.SendMessage(update.Message.Chat.Id, string.Join(Environment.NewLine, dataUnits), replyMarkup: replyKeyboard);
					break;

				case "Выход":
					await client.SendMessage(update.Message.Chat.Id, "Клавиатура скрыта", replyMarkup: new ReplyKeyboardRemove());
					break;

				default:
					// Обработка случая, когда текст сообщения не соответствует ни одному из условий
					break;
			}
		}
	}
}
