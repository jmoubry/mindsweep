using Mindsweep.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mindsweep.Helpers
{
    /// <summary>
    /// Expression Helpers
    /// </summary>
    public static class Exp
    {
        public static Func<TaskSeries, bool> HasTags = t => !string.IsNullOrWhiteSpace(t.Tags);
        public static Func<Project, bool> IsInbox = p => p.Name == "Inbox" && p.Locked;
        public static Func<TaskSeries, bool> IsOpen = t => t.Tasks.Any(tt => !tt.Completed.HasValue && !tt.Deleted.HasValue);
    }
}
