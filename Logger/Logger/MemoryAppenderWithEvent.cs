using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logger
{
    class MemoryAppenderWithEvent : MemoryAppender
    {
        public event EventHandler<LoggingEvent> Updated;

        protected override void Append(LoggingEvent loggingEvent)
        {
            // Append the event as usual
            base.Append(loggingEvent);

            // Then alert the Updated event that an event has occurred
            Updated?.Invoke(this, loggingEvent);
        }
    }
}
