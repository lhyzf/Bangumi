﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int expires { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
        public int user_id { get; set; }
    }
}
