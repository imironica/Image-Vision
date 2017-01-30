using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchEngine.DTO
{
    public class Comment
    {
        public string Text { get; set; }
        public string UsernameAdd { get; set; }
        public DateTime DateAdd {get; set;} 
        public DateTime DateDelete { get; set; }
        public string UsernameDelete { get; set; }
        public int IsActive { get; set; }
    }
}
