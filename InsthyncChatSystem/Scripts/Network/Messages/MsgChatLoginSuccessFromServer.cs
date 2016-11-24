using UnityEngine.Networking;

namespace Insthync.ChatSystem
{
public class MsgChatLoginSuccessFromServer : MessageBase
{
    public const short MsgId = ChatNetworkMessageId.ToClientChatLoginSuccessMsgId;
    public string userId = string.Empty;
    public string name = string.Empty;
}
}
