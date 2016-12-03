using UnityEngine.Networking;

namespace Insthync.ChatSystem
{
    public class MsgChatLoginRequestFromClient : MessageBase
    {
        public const short MsgId = ChatNetworkMessageId.ToServerChatLoginRequestMsgId;
        public string userId = string.Empty;
        public string name = string.Empty;
    }
}
