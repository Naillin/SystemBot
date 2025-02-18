using NLog;
using System.Diagnostics;

namespace SystemBot
{
	internal class SystemTools
	{
		private static readonly string moduleName = "SystemTools";
		private static readonly Logger baseLogger = LogManager.GetLogger(moduleName);
		private static readonly LoggerManager logger = new LoggerManager(baseLogger, moduleName);

		private bool _sudo = false;
		public bool Sudo
		{
			get { return _sudo; }
			set { _sudo = value; }
		}
		public SystemTools(bool sudo = false)
		{
			Sudo = sudo;
		}

		/// <summary>
		/// Выполняет команду в оболочке Bash и возвращает результат.
		/// </summary>
		/// <param name="command">Команда для выполнения.</param>
		/// <returns>Результат выполнения команды в виде строки.</returns>
		public string ExecuteCommand(string command)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/bin/bash",
					Arguments = $"-c \"{(_sudo ? "sudo" : string.Empty) + " " + command}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};

			process.Start();
			string result = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			return result.Trim();
		}

		/// <summary>
		/// Получает текущую загрузку CPU в процентах.
		/// </summary>
		/// <returns>Загрузка CPU в процентах (от 0 до 100).</returns>
		public double GetCpuLoad()
		{
			string output = ExecuteCommand("top -bn1 | grep 'Cpu(s)' | sed 's/.*, *\\([0-9.]*\\)%* id.*/\\1/'");
			double idle = double.Parse(output);
			double result = 100.0 - idle;

			logger.Info($"CPU Load: {result}.");

			return result;
		}

		/// <summary>
		/// Получает текущую температуру CPU в градусах Цельсия.
		/// </summary>
		/// <returns>Температура CPU в градусах Цельсия.</returns>
		public double GetCpuTemperature()
		{
			string output = ExecuteCommand("cat /sys/class/thermal/thermal_zone0/temp");
			double result = double.Parse(output) / 1000.0;

			logger.Info($"CPU Temperature: {result}.");

			return result;
		}

		/// <summary>
		/// Получает текущую загрузку оперативной памяти (RAM) в процентах.
		/// </summary>
		/// <returns>Загрузка RAM в процентах (от 0 до 100).</returns>
		public double GetRamUsage()
		{
			string output = ExecuteCommand("free -m | grep Mem | awk '{print $3/$2 * 100.0}'");
			double result = double.Parse(output);

			logger.Info($"RAM Usage: {result}.");

			return result;
		}

		/// <summary>
		/// Получает текущую загрузку корневого раздела диска в процентах.
		/// </summary>
		/// <returns>Загрузка диска в процентах (от 0 до 100). В случае ошибки возвращает -1.</returns>
		public double GetDiskUsage()
		{
			double result = -1;
			try
			{
				string output = ExecuteCommand("df -h / | awk 'NR==2 {print $5}'");

				// Убираем символ процента и парсим число
				if (output.EndsWith("%"))
				{
					output = output.TrimEnd('%');
				}

				if (double.TryParse(output, out double usage))
				{
					result = usage;
				}
				else
				{
					throw new Exception("Не удалось преобразовать данные в число.");
				}
			}
			catch (Exception ex)
			{
				logger.Error($"Ошибка: {ex.Message}.");

				return result;
			}

			logger.Info($"DISK Usage: {result}.");

			return result;
		}

		/// <summary>
		/// Перечисление, представляющее возможные статусы сервиса.
		/// </summary>
		public enum StatusType
		{
			/// <summary>
			/// Сервис активен (запущен).
			/// </summary>
			ACTIVE = 0,

			/// <summary>
			/// Сервис неактивен (остановлен).
			/// </summary>
			INACTIVE = 1,

			/// <summary>
			/// Сервис не найден.
			/// </summary>
			NOT_FOUND = 2,

			/// <summary>
			/// Произошла ошибка при проверке статуса сервиса.
			/// </summary>
			ERROR = 3
		}

		/// <summary>
		/// Структура, представляющая данные о статусе сервиса.
		/// </summary>
		public struct DataUnit
		{
			/// <summary>
			/// Статус сервиса.
			/// </summary>
			public StatusType _statusType;

			/// <summary>
			/// Время активности/неактивности сервиса или сообщение об ошибке.
			/// </summary>
			public string _date = string.Empty;

			/// <summary>
			/// Конструктор для создания экземпляра структуры DataUnit.
			/// </summary>
			/// <param name="statusType">Статус сервиса.</param>
			/// <param name="data">Время активности/неактивности или сообщение об ошибке.</param>
			public DataUnit(StatusType statusType = StatusType.NOT_FOUND, string data = "")
			{
				_statusType = statusType;
				_date = data;
			}

			public override string ToString()
			{
				return $"Status: {_statusType} from {_date}";
			}
		}

		/// <summary>
		/// Получает статус указанного сервиса и время его активности или неактивности.
		/// </summary>
		/// <param name="serviceName">Имя сервиса.</param>
		/// <returns>
		/// Структура DataUnit, содержащая:
		/// - Статус сервиса (ACTIVE, INACTIVE, NOT_FOUND, ERROR).
		/// - Время активности/неактивности или сообщение об ошибке.
		/// </returns>
		public DataUnit GetServiceStatus(string serviceName)
		{
			DataUnit result = new DataUnit();

			try
			{
				// Проверяем, существует ли сервис
				string exists = ExecuteCommand($"systemctl list-units | grep -w {serviceName}");
				if (string.IsNullOrEmpty(exists))
				{
					result._statusType = StatusType.NOT_FOUND;
					result._date = "Service does not exist";
					logger.Warn($"Service {serviceName} not found.");
					return result;
				}

				// Получаем статус сервиса
				string status = ExecuteCommand($"systemctl is-active {serviceName}");
				// Получаем время активности или неактивности
				if (status == "active")
				{
					string uptime = ExecuteCommand($"systemctl show -p ActiveEnterTimestamp {serviceName} | cut -d= -f2");
					result._statusType = StatusType.ACTIVE;
					result._date = uptime;
				}
				else
				{
					string downtime = ExecuteCommand($"systemctl show -p InactiveEnterTimestamp {serviceName} | cut -d= -f2");
					result._statusType = StatusType.INACTIVE;
					result._date = downtime;
				}

				logger.Info($"Status of service {serviceName}: {result._statusType} from {result._date}.");
			}
			catch (Exception ex)
			{
				result._statusType = StatusType.ERROR;
				result._date = ex.Message;
				logger.Error($"Error while checking status of service {serviceName}: {ex.Message}.");
			}

			return result;
		}

		/// <summary>
		/// Получает статусы для нескольких сервисов.
		/// </summary>
		/// <param name="servicesNames">Массив имен сервисов.</param>
		/// <returns>
		/// Массив структур DataUnit, каждая из которых содержит:
		/// - Статус сервиса (ACTIVE, INACTIVE, NOT_FOUND, ERROR).
		/// - Время активности/неактивности или сообщение об ошибке.
		/// </returns>
		public DataUnit[] GetServicesStatus(string[] servicesNames)
		{
			DataUnit[] dataUnits = new DataUnit[servicesNames.Length];

			for (int i = 0; i < servicesNames.Length; i++)
			{
				dataUnits[i] = GetServiceStatus(servicesNames[i]);
			}

			return dataUnits;
		}

		/// <summary>
		/// Перезагружает сервер.
		/// </summary>
		/// <param name="delaySeconds">Задержка в секундах.</param>
		public void RestartServer(int delaySeconds)
		{
			logger.Info($"Перезагрузка через {delaySeconds} секунд...");
			Thread.Sleep(delaySeconds * 1000);
			ExecuteCommand("reboot");
		}

		/// <summary>
		/// Выключает сервер.
		/// </summary>
		/// <param name="delaySeconds">Задержка в секундах.</param>
		public void ShutdownServer(int delaySeconds)
		{
			logger.Info($"Выключение через {delaySeconds} секунд...");
			Thread.Sleep(delaySeconds * 1000);
			ExecuteCommand("poweroff");
		}
	}
}
