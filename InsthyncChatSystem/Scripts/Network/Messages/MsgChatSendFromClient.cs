using UnityEngine.Networking;

namespace Insthync.ChatSystem
{
    public class MsgChatSendFromClient : MessageBase
    {
        public const short MsgId = ChatNetworkMessageId.ToServerChatSendMsgId;
        public string channelId = string.Empty;
        public string[] chatData;
    }
}
