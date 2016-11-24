namespace Insthync.ChatSystem
{
public class ChatNetworkMessageId
{
    // Developer can changes these Ids to avoid hacking while hosting
    public const short ToServerChatSendMsgId = 4000;
    public const short ToServerChatLoginRequestMsgId = 4001;
    public const short ToClientChatReceiveMsgId = 4002;
    public const short ToClientChatLoginSuccessMsgId = 4003;
}
}
