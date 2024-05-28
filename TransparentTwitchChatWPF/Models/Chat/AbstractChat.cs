namespace TransparentTwitchChatWPF.Models.Chat
{
    public abstract class Chat
    {
        protected ChatTypes ChatType { get; private set; }

        protected Chat(ChatTypes chatType)
        {
            ChatType = chatType;
        }

        public virtual string PushNewChatMessage(string message = "", string nickname = "", string color = "")
        {
            return string.Empty;
        }

        public virtual string PushNewMessage(string message = "")
        {
            return string.Empty;
        }

        public virtual string SetupJavascript()
        {
            return string.Empty;
        }

        public virtual string SetupCustomCSS()
        {
            return string.Empty;
        }
    }
}
