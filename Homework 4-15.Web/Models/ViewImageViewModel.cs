using Homework_4_15.Data;

namespace Homework_4_15.Web.Models
{
    public class ViewImageViewModel
    {
        public bool HasPermissionToView { get; set; }
        public Image Image { get; set; }
        public string Message { get; set; }
    }
}
