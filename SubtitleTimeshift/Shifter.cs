using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
	public class Shifter
	{

		private static readonly string[] SEPARATOR_ARROW = { " --> " };
		private static readonly char[] SEPARATOR_TIME_SPAN = { ':', ',', '.' };

		public async static Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
		{
			using (var reader = new StreamReader(input, encoding, false, bufferSize, leaveOpen))
			using (var writer = new StreamWriter(output, encoding, bufferSize, leaveOpen))
			{
				var actualRow = await reader.ReadLineAsync();

				while (actualRow != null)
				{
					var outputRow = GetOutputRow(actualRow, timeSpan);
					actualRow = await reader.ReadLineAsync();

					await writer.WriteLineAsync(outputRow);
				}
			}
		}

		private static string GetOutputRow(string actualRow, TimeSpan timeSpan)
		{
			var separatedRow = actualRow.Split(SEPARATOR_ARROW, StringSplitOptions.RemoveEmptyEntries);

			if (IsTimeRow(separatedRow))
			{
				var startTime = CorrectTime(separatedRow[0], timeSpan);
				var endTime = CorrectTime(separatedRow[1], timeSpan);

				return $"{startTime}{SEPARATOR_ARROW[0]}{endTime}";
			}

			return actualRow;
		}

		private static bool IsTimeRow(string[] separatedRow) => separatedRow.Length == 2;

		private static string CorrectTime(string timeSpanText, TimeSpan timeSpan)
		{
			var separatedNumbers = timeSpanText.Split(SEPARATOR_TIME_SPAN)
								   .Select(stringNumber => int.Parse(stringNumber))
								   .ToArray();

			var correctedTime = new TimeSpan(0, 
									separatedNumbers[0], 
									separatedNumbers[1], 
									separatedNumbers[2], 
									separatedNumbers[3])
									.Add(timeSpan);

			return correctedTime.ToString(@"hh\:mm\:ss\.fff"); ;
		}
	}
}
