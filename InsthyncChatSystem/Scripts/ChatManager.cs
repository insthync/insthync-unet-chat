using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Insthync.ChatSystem
{
    public class ChatManager : MonoBehaviour
    {
        public ChatChannelData defaultChannel = new ChatChannelData("default", "", "", "", Color.white);
        public ChatChannelData[] channelList;
        public delegate void ChatMessageReceiveEvent(ChatManager chatManager, ChatMessage chatMessage);
        public static ChatMessageReceiveEvent onReceiveMessage;
        private Dictionary<string, ChatChannelData> channels;
        public Dictionary<string, ChatChannelData> Channels
        {
            get
            {
                if (channels == null)
                {
                    channels = new Dictionary<string, ChatChannelData>();
                    if (channelList == null || channelList.Length == 0)
                        channelList = new ChatChannelData[1] { defaultChannel };

                    for (int i = 0; i < channelList.Length; ++i)
                    {
                        ChatChannelData channel = channelList[i];
                        if (channel != null && !string.IsNullOrEmpty(channel.channelId) && !channels.ContainsKey(channel.channelId))
                            channels.Add(channel.channelId, channel);
                    }
                }
                return channels;
            }
        }
        private List<ChatMessage> messages;
        public List<ChatMessage> Messages
        {
            get
            {
                if (messages == null)
                    messages = new List<ChatMessage>();
                return messages;
            }
        }
        private Dictionary<NetworkConnection, ChatUser> chatUsers;
        public Dictionary<NetworkConnection, ChatUser> ChatUsers
        {
            get
            {
                if (chatUsers == null)
                    chatUsers = new Dictionary<NetworkConnection, ChatUser>();
                return chatUsers;
            }
        }
        public static ChatManager Singleton { get; private set; }
        public NetworkConnection clientConnection;
        private ChatUser clientChatUser;

        void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;

            DontDestroyOnLoad(gameObject);
        }

        public void SetupServerMessages()
        {
            NetworkServer.RegisterHandler(MsgChatSendFromClient.MsgId, OnServerChatReceive);
            NetworkServer.RegisterHandler(MsgChatLoginRequestFromClient.MsgId, OnServerLoginRequest);
        }

        public void SetupClientMessages(NetworkClient client)
        {
            client.RegisterHandler(MsgChatReceiveFromServer.MsgId, OnClientChatReceive);
            client.RegisterHandler(MsgChatLoginSuccessFromServer.MsgId, OnClientLoginSuccess);
        }

        public void AddChatUser(NetworkConnection conn, string userId, string name)
        {
            AddChatUser(new ChatUser(conn, userId, name));
        }

        public void AddChatUser(ChatUser user)
        {
            if (user != null && user.conn != null && !ChatUsers.ContainsKey(user.conn))
            {
                ChatUsers.Add(user.conn, user);
                MsgChatLoginSuccessFromServer msg = new MsgChatLoginSuccessFromServer();
                msg.userId = user.userId;
                msg.name = user.name;
                user.conn.Send(MsgChatLoginSuccessFromServer.MsgId, msg);
            }
        }

        public void RemoveChatUser(NetworkConnection conn)
        {
            if (conn != null && ChatUsers.ContainsKey(conn))
                ChatUsers.Remove(conn);
        }

        public void ClearChatUser()
        {
            ChatUsers.Clear();
        }

        public void SetClientChatUser(string userId, string name)
        {
            SetClientChatUser(new ChatUser(clientConnection, userId, name));
        }

        public void SetClientChatUser(ChatUser user)
        {
            clientChatUser = user;
        }

        public void ClearClientChatUser()
        {
            clientChatUser = null;
        }

        public void ClientChatReceive(ChatChannelDataResult result)
        {
            if (result == null)
                return;

            MsgChatReceiveFromServer msg = new MsgChatReceiveFromServer();
            msg.channelId = result.channel.channelId;
            msg.senderId = result.sender.userId;
            msg.senderName = result.sender.name;
            msg.message = result.message;
            if (result.isBroadcast)
            {
                foreach (ChatUser user in ChatUsers.Values)
                {
                    if (user.conn != null)
                        user.conn.Send(MsgChatReceiveFromServer.MsgId, msg);
                }
            }
            else
            {
                if (result.receiver != null && result.receiver.conn != null)
                    result.receiver.conn.Send(MsgChatReceiveFromServer.MsgId, msg);
            }
        }

        public void ClientChatSend(string channelId, string message)
        {
            if (clientChatUser == null || string.IsNullOrEmpty(message))
                return;

            ChatChannelData channel = null;
            if (Channels.TryGetValue(channelId, out channel))
            {
                string[] chatData = channel.GetChatData(message);
                if (chatData != null && chatData.Length > 0)
                {
                    NetworkConnection conn = clientConnection;
                    MsgChatSendFromClient chatSendMsg = new MsgChatSendFromClient();
                    chatSendMsg.channelId = channelId;
                    chatSendMsg.chatData = chatData;
                    conn.Send(MsgChatSendFromClient.MsgId, chatSendMsg);
                }
                else
                    Debug.LogWarning("[Warning] Invalid chat data");
            }
            else
                Debug.LogWarning("[Warning] Chat channel (" + channelId + ") not found");
        }

        public void OnServerChatReceive(NetworkMessage netMsg)
        {
            MsgChatSendFromClient msg = netMsg.ReadMessage<MsgChatSendFromClient>();
            ChatChannelData channel = defaultChannel;
            if (ChatUsers.ContainsKey(netMsg.conn))
            {
                ChatUser user = ChatUsers[netMsg.conn];
                if (Channels.ContainsKey(msg.channelId))
                    channel = Channels[msg.channelId];
                else
                    Debug.LogWarning("[Warning] Chat channel (" + msg.channelId + ") not found");

                if (channel != null)
                    ClientChatReceive(channel.DoChatLogic(user, msg.chatData));
            }
            else
                Debug.LogError("[Error] Invalid chat user " + netMsg.conn.connectionId);
        }

        public void OnServerLoginRequest(NetworkMessage netMsg)
        {
            MsgChatLoginRequestFromClient msg = netMsg.ReadMessage<MsgChatLoginRequestFromClient>();
            string userId = msg.userId;
            if (string.IsNullOrEmpty(userId))
                userId = System.Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(msg.name))
                AddChatUser(netMsg.conn, userId, msg.name);
            else
                Debug.LogWarning("[Warning] Chat user " + netMsg.conn.connectionId + " entered empty name");
        }

        public void OnClientChatReceive(NetworkMessage netMsg)
        {
            MsgChatReceiveFromServer msg = netMsg.ReadMessage<MsgChatReceiveFromServer>();
            ChatChannelData channel = defaultChannel;
            if (Channels.ContainsKey(msg.channelId))
                channel = Channels[msg.channelId];
            else
                Debug.LogWarning("[Warning] Chat channel (" + msg.channelId + ") not found");

            if (channel != null)
            {
                ChatMessage.ChatState chatState = ChatMessage.ChatState.Receive;
                if (msg.senderId.Equals(clientChatUser.userId))
                    chatState = ChatMessage.ChatState.Send;
                ChatMessage chatMessage = new ChatMessage(channel, msg.senderId, msg.senderName, msg.message, chatState);
                Messages.Add(chatMessage);
                if (onReceiveMessage != null)
                    onReceiveMessage(this, chatMessage);
            }
        }

        public void OnClientLoginSuccess(NetworkMessage netMsg)
        {
            MsgChatLoginSuccessFromServer msg = netMsg.ReadMessage<MsgChatLoginSuccessFromServer>();
            if (!string.IsNullOrEmpty(msg.userId))
                SetClientChatUser(msg.userId, msg.name);
        }
    }
}
