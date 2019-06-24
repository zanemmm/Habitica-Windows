using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habitica.Models
{
    class Setting
    {
        public string UserId { get; set; }
        public string ApiToken { get; set; }

        public Setting(string UserId, string ApiToken)
        {
            this.UserId = UserId;
            this.ApiToken = ApiToken;
        }
    }
}
