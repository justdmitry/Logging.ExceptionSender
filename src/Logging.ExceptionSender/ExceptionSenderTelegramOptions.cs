using System;
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
        /// Telegram Bot token (like 123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11).
        /// </summary>
        public string BotToken { get; set; }
    }
}
