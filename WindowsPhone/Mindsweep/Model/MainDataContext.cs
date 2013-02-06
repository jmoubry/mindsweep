using System.Data.Linq;

namespace Mindsweep.Model
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh286405(v=vs.105).aspx
    /// </summary>
    public class MainDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public MainDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a table for the to-do items.
        public Table<Request> RequestQueue;

        // Specify a table for the to-do items.
        public Table<Task> Tasks;

        // Specify a table for the to-do items.
        public Table<TaskSeries> TaskSeries;

        // Specify a table for the categories.
        public Table<Project> Projects;

        // Specify a table for the categories.
        public Table<RepeatRule> RepeatRules;
    }
}
