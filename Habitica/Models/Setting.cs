using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Habitica.Models
{
    class Setting
    {
        public string UserId { get; set; }
        public string ApiToken { get; set; }
        public Point Position = new Point(SystemParameters.PrimaryScreenWidth - 340, 0);
        public bool IsPinned = true;

        public Setting(string UserId, string ApiToken)
        {
            this.UserId = UserId;
            this.ApiToken = ApiToken;
        }
    }
}
