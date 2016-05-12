using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    class PreviewAction
    {
        public string Name { get; set; }
        public Action Action { get; set; }
    }


    public class PreviewableService
    {
        List<PreviewAction> actions = new List<PreviewAction>();
        public void AddAction(string name, Action action)
        {
            Console.WriteLine(name);
            actions.Add(new PreviewAction { Action = action, Name = name });
        }

        void ExecuteAllAction()
        {
            foreach(var e in actions)
            {
                Console.Write(e.Name + "... ");
                try
                {
                    e.Action();
                    Console.WriteLine("Done");
                }
                catch(Exception _e)
                {
                    Console.WriteLine("Failed, " + _e.Message);
                }
            }
        }

        public bool AskAndExecute()
        {
            Console.WriteLine("Commit action?");
            var key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Y) return false;
            Console.WriteLine("Ok, commiting action");
            ExecuteAllAction();
            return true;
        }
    }
}
