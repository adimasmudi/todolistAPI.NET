using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models.InputTodo
{
    public class TodoInput
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
