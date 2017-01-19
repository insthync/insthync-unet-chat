using UnityEngine.Networking;

namespace Insthync.ChatSystem
{
    public class ChatNetworkMessageId
    {
        // Developer can changes these Ids to avoid hacking while hosting
        public const short ToServerChatSendMsgId = MsgType.Highest + 101;
        public const short ToServerChatLoginRequestMsgId = MsgType.Highest + 102;
        public const short ToClientChatReceiveMsgId = MsgType.Highest + 103;
        public const short ToClientChatLoginSuccessMsgId = MsgType.Highest + 104;
    }
}
