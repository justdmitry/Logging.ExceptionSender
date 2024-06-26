﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.ExceptionSender
{
    public class ExceptionSenderTelegramOptions : ExceptionSenderOptions
    {
        /// <summary>
        /// Chat ID to send reports to.
        /// </summary>
        public string ChatId { get; set; }

        /// <summary>
        /// Unique identifier for the target message thread (topic) of the forum; for forum supergroups only.
        /// </summary>
        public long? MessageThreadId { get; set; }

        /// <summary>
        /// Telegram Bot token (like 123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11).
        /// </summary>
        public string BotToken { get; set; }

        /// <summary>
        /// Additional inifo about app to be added to message text.
        /// </summary>
        public string AppNameSuffix { get; set; }
    }
}
