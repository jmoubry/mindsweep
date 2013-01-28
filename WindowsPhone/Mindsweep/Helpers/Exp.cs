using Mindsweep.Model;
using System;

namespace Mindsweep.Helpers
{
    /// <summary>
    /// Expression Helpers
    /// </summary>
    public static class Exp
    {
        #region Project Expressions
        public static Func<Project, bool> IsInbox = p => p.Name == "Inbox" && p.Locked;

        
        // Not archived, not smart, not inbox, not sent
        public static Func<Project, bool> IsActive = p => !p.Archived && !p.Smart && !p.Locked;

        #endregion

        #region Task Expressions

        public static Func<Task, bool> HasTags = t => !string.IsNullOrWhiteSpace(t.TaskSeries.Tags);

        public static Func<Task, bool> IsOpen = t => !t.Completed.HasValue && !t.Deleted.HasValue;

        public static Func<Task, bool> IsNextAction = t => !t.Completed.HasValue && !t.Deleted.HasValue && t.Due.HasValue
                                                           && ((t.HasDueTime && t.Due.Value.ToLocalTime() <= DateTime.Now)
                                                           || (!t.HasDueTime && t.Due.Value.ToLocalTime().Date <= DateTime.Now.Date));

        public static Func<Task, bool> IsDueToday = t => !t.Completed.HasValue && !t.Deleted.HasValue && t.Due.HasValue
                                                         && t.Due.Value.ToLocalTime().Date == DateTime.Now.Date;

        public static Func<Task, bool> IsDueTomorrow = t => !t.Completed.HasValue && !t.Deleted.HasValue && t.Due.HasValue
                                                            && t.Due.Value.ToLocalTime().Date == DateTime.Now.Date.AddDays(1);

        public static Func<Task, bool> IsDueThisWeek = t => !t.Completed.HasValue && !t.Deleted.HasValue && t.Due.HasValue
                                                            && t.Due.Value.ToLocalTime().Date > DateTime.Now.Date.AddDays(1)
                                                            && t.Due.Value.ToLocalTime().Date < DateTime.Now.Date.AddDays(7);

        public static Func<Task, bool> IsOverdue = t => !t.Completed.HasValue && !t.Deleted.HasValue
                                                        && t.Due.HasValue
                                                        && ((t.HasDueTime && t.Due.Value.ToLocalTime() < DateTime.Now) 
                                                              || (!t.HasDueTime && t.Due.Value.ToLocalTime().Date < DateTime.Now.Date));

        #endregion
    }
}
