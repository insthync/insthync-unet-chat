using UnityEngine.Networking;

namespace Insthync.ChatSystem
{
    public class MsgChatReceiveFromServer : MessageBase
    {
        public const short MsgId = ChatNetworkMessageId.ToClientChatReceiveMsgId;
        public string channelId = string.Empty;
        public string senderId = string.Empty;
        public string senderName = string.Empty;
        public string message;
    }
}
