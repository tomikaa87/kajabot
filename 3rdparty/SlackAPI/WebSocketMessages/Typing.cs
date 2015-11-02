﻿using System;

namespace SlackAPI.WebSocketMessages
{
    [SlackSocketRouting("user_typing")]
    public class Typing : SlackSocketMessage
    {
        public string user;
        public string channel;
    }
}
