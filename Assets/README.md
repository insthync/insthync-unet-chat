# insthync-unet-chat

This is the chat system for Unity UNET it's designed to use with **NetworkManager** class

## How to use it

Attach **ChatManager** to any game object

You have to add code to register client/server messages (You can add codes like: chatManager.SetupServerMessages(); in **OnStartServer** function AND chatManager.SetupClientMessages(client); in **OnStartClient** function)

You have to set **client** when connecting to server (You can add codes like: chatManager.client = networkManager.client; in **OnStartClient** function)
