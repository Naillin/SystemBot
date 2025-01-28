using IniParser.Model;
using IniParser;
using NLog;
using Telegram.Bot;

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

		private static ITelegramBotClient? _botClient;
		static async Task Main(string[] args)
		{
			logger.Info($"Starting......");
			initConfig();
			logger.Info(configTextDefault);

			_botClient = new TelegramBotClient(_token);

			logger.Info($"Done!!!!!");
		}
	}
}
