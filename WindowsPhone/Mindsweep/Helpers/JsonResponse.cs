using Mindsweep.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mindsweep.Helpers
{
    public enum StatusCodes
    {
        Unknown = 0,
        Success = 1,
        InvalidAuthToken = 98
    }

    public class JsonResponse
    {
        public JsonResult rsp { get; set; }

        public List<Project> Projects
        {
            get
            {
                if (rsp == null || rsp.lists == null || rsp.lists.projects == null)
                    return new List<Project>();

                return rsp.lists.projects;
            }
        }

        public List<Project> TasksByProject
        {
            get
            {
                List<Project> projects = new List<Project>();

                if (rsp == null
                    || rsp.tasks == null
                    || rsp.tasks.list == null)
                    return projects;


                foreach(JsonTasksResultsList list in rsp.tasks.list)
                {
                    if (list.taskseries != null && list.taskseries.Count  > 0)
                    {
                        Project p = new Project() { Id = list.id };
                        p.TaskSeries.AddRange(list.taskseries);
                        projects.Add(p);
                    }
                }

                return projects;
            }
        }

        public List<Project> DeletedTasksByProject
        {
            get
            {
                List<Project> projects = new List<Project>();

                if (rsp == null
                    || rsp.tasks == null
                    || rsp.tasks.list == null)
                    return projects;

                foreach (JsonTasksResultsList list in rsp.tasks.list)
                {
                    if (list.deleted != null && list.deleted.Count > 0)
                    {
                        foreach (JsonTasksResultsList lt in list.deleted)
                        {
                            if (lt.taskseries != null && lt.taskseries.Count > 0)
                            {
                                Project p = new Project() { Id = list.id };
                                p.TaskSeries.AddRange(lt.taskseries);
                                projects.Add(p);
                            }
                        }
                    }
                }

                return projects;
            }
        }

        public StatusCodes Status
        {
            get
            {
                if (rsp == null)
                    return StatusCodes.Unknown;

                if (rsp.err != null)
                {
                    if (rsp.err.code == "98")
                        return StatusCodes.InvalidAuthToken;

                    return StatusCodes.Unknown;   
                }

                if (rsp.stat == "ok")
                    return StatusCodes.Success;


                return StatusCodes.Unknown;  
            }
        }
    }

    public class JsonResult
    {
        public string stat { get; set; }
        public JsonError err { get; set; }
        public JsonListsResults lists { get; set; }
        public JsonTasksResults tasks { get; set; }
    }

    public class JsonError
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class JsonListsResults
    {
        [JsonProperty(PropertyName = "list")]
        public List<Project> projects { get; set; }
    }

    public class JsonTasksResults
    {
        public string rev { get; set; }

        [JsonConverter(typeof(ObjectToArrayConverter<JsonTasksResultsList>))]
        public List<JsonTasksResultsList> list { get; set; }
    }

    public class JsonTasksResultsList
    {
        public string id { get; set; }
        public DateTime current { get; set; }

        [JsonConverter(typeof(ObjectToArrayConverter<TaskSeries>))]
        public List<TaskSeries> taskseries { get; set; }

        [JsonConverter(typeof(ObjectToArrayConverter<JsonTasksResultsList>))]
        public List<JsonTasksResultsList> deleted { get; set; }
    }
}
