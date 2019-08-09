/*
 * Copyright 2019, Digi International Inc.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using Common.Logging;
using Common.Logging.Simple;
using System;
using System.Text;

namespace XBeeLibrary.Core.Utils.Logger
{
	public class ConsoleOutLogger : AbstractSimpleLogger
	{
		public ConsoleOutLogger(string logName, LogLevel logLevel, bool showLevel, bool showDateTime,
			bool showLogName, string dateTimeFormat)
			: base(logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat) { }

		protected override void WriteInternal(LogLevel level, object message, Exception e)
		{
			// Use a StringBuilder for better performance
			StringBuilder sb = new StringBuilder();
			FormatOutput(sb, level, message, e);

			// Print to the appropriate destination
			Console.Out.WriteLine(sb.ToString());
		}
	}
}
