using ToDoList.Models;

namespace ToDoList.Responses
{
    public class TodoResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Todo Data { get; set; }
    }
}
