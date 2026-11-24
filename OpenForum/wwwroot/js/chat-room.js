//chat-room.js
document.addEventListener("DOMContentLoaded", async () => {

    const roomId = window.chatRoomId;

    const messagesContainer =
        document.getElementById("messagesContainer");

    const messagesLoading =
        document.getElementById("messagesLoading");

    const messageForm =
        document.getElementById("messageForm");

    const messageInput =
        document.getElementById("messageInput");

    const sendMessageButton =
        document.getElementById("sendMessageButton");

    const errorElement =
        document.getElementById("chatError");

    const MAX_MESSAGES = 100;

    const messages = new Map();

    let connection = null;

    // --------------------------------
    // Error handling
    // --------------------------------

    function showError(message) {
        errorElement.textContent = message;
        errorElement.classList.remove("d-none");
    }

    function clearError() {
        errorElement.textContent = "";
        errorElement.classList.add("d-none");
    }

    // --------------------------------
    // Message rendering
    // --------------------------------

function renderMessages() {

    const sortedMessages = Array.from(messages.values())
        .sort((a, b) =>
            new Date(a.sentAt) - new Date(b.sentAt)
        );

    // Keep the client-side message limit bounded.
    while (sortedMessages.length > MAX_MESSAGES) {
        const oldestMessage = sortedMessages.shift();
        messages.delete(oldestMessage.id);
    }

    messagesContainer.replaceChildren();

    sortedMessages.forEach(message => {

        // Determine ownership for the currently logged-in user.
        const isOwnMessage =
            message.senderUserId === window.chatCurrentUserId;

        const messageElement = document.createElement("div");

        messageElement.className =
            isOwnMessage
                ? "align-self-start bg-primary text-white rounded p-3"
                : "align-self-end bg-light border rounded p-3";

        messageElement.style.maxWidth = "75%";

        const sender = document.createElement("strong");
        sender.className = "d-block small mb-1";
        sender.textContent = message.senderUsername;

        const content = document.createElement("div");
        content.style.whiteSpace = "pre-wrap";
        content.style.overflowWrap = "anywhere";
        content.textContent = message.content;

        const timestamp = document.createElement("small");

        timestamp.className =
            isOwnMessage
                ? "d-block mt-2 text-white-50"
                : "d-block mt-2 text-muted";

        const sentAt = new Date(message.sentAt);

        timestamp.textContent = sentAt.toLocaleString();

        messageElement.append(
            sender,
            content,
            timestamp
        );

        messagesContainer.appendChild(messageElement);
    });

    messagesContainer.scrollTop =
        messagesContainer.scrollHeight;
}

    function addMessage(message) {

        // Prevent duplicates when history and SignalR overlap.
        if (messages.has(message.id))
            return;

        messages.set(message.id, message);

        renderMessages();
    }

    // --------------------------------
    // Load existing messages
    // --------------------------------

    async function loadMessages() {

        try {
            const response = await fetch(
                `/Chat/Messages?roomId=${roomId}`
            );

            if (!response.ok) {
                throw new Error("Could not load chat messages.");
            }

            const loadedMessages = await response.json();

            loadedMessages.forEach(message => {
                addMessage(message);
            });

            messagesLoading.classList.add("d-none");

        } catch (error) {
            showError(error.message);
            messagesLoading.classList.add("d-none");
        }
    }

    // --------------------------------
    // SignalR connection
    // --------------------------------

    async function connectToChat() {

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect() 
            .build();

        // Receive messages from other users.
        connection.on("ReceiveMessage", message => {
            addMessage(message);
        });

        connection.onreconnecting(() => {
            showError("Connection lost. Reconnecting...");
            sendMessageButton.disabled = true;
        });

        connection.onreconnected(async () => {

            clearError();

            try {
                await connection.invoke("JoinRoom", roomId);

                await loadMessages();

                sendMessageButton.disabled = false;

            } catch (error) {
                showError("Could not rejoin the chatroom.");
            }
        });

        connection.onclose(() => {
            showError(
                "Connection to the chatroom was lost. Please refresh the page."
            );

            sendMessageButton.disabled = true;
        });

        try {
            await connection.start();

            await connection.invoke("JoinRoom", roomId);

            clearError();

        } catch (error) {
            showError("Could not connect to the chatroom.");
            sendMessageButton.disabled = true;
        }
    }

    // --------------------------------
    // Send messages
    // --------------------------------

    messageForm.addEventListener("submit", async event => {

        event.preventDefault();

        clearError();

        const content = messageInput.value.trim();

        if (!content)
            return;

        if (content.length > 2000) {
            showError("Messages cannot exceed 2000 characters.");
            return;
        }

        if (!connection ||
            connection.state !== signalR.HubConnectionState.Connected) {

            showError("You are not connected to the chatroom.");
            return;
        }

        sendMessageButton.disabled = true;

        try {
            await connection.invoke(
                "SendMessage",
                roomId,
                content
            );

            messageInput.value = "";
            messageInput.focus();

        } catch (error) {
            showError("Could not send your message.");
        } finally {
            sendMessageButton.disabled =
                connection.state !== signalR.HubConnectionState.Connected;
        }
    });

    // --------------------------------
    // Room members
    // --------------------------------

    async function loadRoomMembers() {

        const membersList =
            document.getElementById("roomMembersList");

        const loading =
            document.getElementById("roomMembersLoading");

        try {

            const response = await fetch(
                `/Chat/RoomMembers?roomId=${roomId}`
            );

            if (!response.ok) {
                throw new Error("Could not load room members.");
            }

            const members = await response.json();

            membersList.replaceChildren();

            members.forEach(member => {

                const badge = document.createElement("span");

                badge.className = "badge bg-light text-dark border p-2";

                badge.textContent = member.isOwner
                    ? `${member.username} (Owner)`
                    : member.username;

                membersList.appendChild(badge);
            });

            document.getElementById("roomMemberCount").textContent =
                members.length;

        } catch (error) {

            showError(error.message);

        } finally {

            loading.classList.add("d-none");
        }
    }

    // --------------------------------
    // Invite friends
    // --------------------------------

    const inviteFriendsButton =
        document.getElementById("inviteFriendsButton");

    if (inviteFriendsButton) {

        const inviteModalElement =
            document.getElementById("inviteFriendsModal");

        const inviteModal =
            new bootstrap.Modal(inviteModalElement);

        const inviteFriendsList =
            document.getElementById("inviteFriendsList");

        const inviteFriendsError =
            document.getElementById("inviteFriendsError");

        const inviteFriendsSuccess =
            document.getElementById("inviteFriendsSuccess");

        async function loadAvailableFriends() {

            inviteFriendsList.replaceChildren();

            inviteFriendsError.classList.add("d-none");
            inviteFriendsSuccess.classList.add("d-none");

            document.getElementById("inviteFriendsLoading")
                .classList.remove("d-none");

            document.getElementById("inviteFriendsEmpty")
                .classList.add("d-none");

            try {

                const response = await fetch(
                    `/Chat/AvailableFriendsToInvite?roomId=${roomId}`
                );

                if (!response.ok) {
                    throw new Error("Could not load available friends.");
                }

                const friends = await response.json();

                document.getElementById("inviteFriendsLoading")
                    .classList.add("d-none");

                if (friends.length === 0) {

                    document.getElementById("inviteFriendsEmpty")
                        .classList.remove("d-none");

                    return;
                }

                friends.forEach(friend => {

                    const item = document.createElement("div");

                    item.className =
                        "list-group-item d-flex justify-content-between align-items-center";

                    const username = document.createElement("span");

                    username.textContent = friend.username;

                    const button = document.createElement("button");

                    button.type = "button";
                    button.className = "btn btn-primary btn-sm";
                    button.textContent = "Invite";

                    button.addEventListener("click", async () => {

                        await inviteFriend(friend.userId, button);
                    });

                    item.append(username, button);

                    inviteFriendsList.appendChild(item);
                });

            } catch (error) {

                document.getElementById("inviteFriendsLoading")
                    .classList.add("d-none");

                inviteFriendsError.textContent = error.message;
                inviteFriendsError.classList.remove("d-none");
            }
        }

        async function inviteFriend(userId, button) {

            inviteFriendsError.classList.add("d-none");
            inviteFriendsSuccess.classList.add("d-none");

            const token = document.querySelector(
                'input[name="__RequestVerificationToken"]'
            )?.value;

            button.disabled = true;

            try {

                const response = await fetch("/Chat/InviteFriend", {

                    method: "POST",

                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },

                    body: new URLSearchParams({
                        roomId,
                        invitedUserId: userId,
                        __RequestVerificationToken: token
                    })
                });

                if (!response.ok) {

                    const result = await response.json();

                    throw new Error(
                        result.error || "Could not invite friend."
                    );
                }

                inviteFriendsSuccess.classList.remove("d-none");

                await loadRoomMembers();
                await loadAvailableFriends();

                inviteFriendsSuccess.textContent =
                    "Friend successfully invited.";

            } catch (error) {

                inviteFriendsError.textContent = error.message;
                inviteFriendsError.classList.remove("d-none");

                button.disabled = false;
            }
        }

        inviteFriendsButton.addEventListener("click", async () => {

            await loadAvailableFriends();

            inviteModal.show();
        });
    }

    
    // --------------------------------
    // Leave room
    // --------------------------------

    const leaveRoomButton =
        document.getElementById("leaveRoomButton");

    if (leaveRoomButton) {

        leaveRoomButton.addEventListener("click", async () => {

            clearError();

            const confirmed = confirm(
                "Are you sure you want to leave this chatroom?"
            );

            if (!confirmed)
                return;

            const token = document.querySelector(
                'input[name="__RequestVerificationToken"]'
            )?.value;

            leaveRoomButton.disabled = true;

            try {

                const response = await fetch("/Chat/LeaveRoom", {

                    method: "POST",

                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },

                    body: new URLSearchParams({
                        roomId,
                        __RequestVerificationToken: token
                    })
                });

                if (!response.ok) {

                    let errorMessage = "Could not leave the chatroom.";

                    try {
                        const result = await response.json();

                        if (result.error)
                            errorMessage = result.error;

                    } catch {
                        // Keep the default error message.
                    }

                    throw new Error(errorMessage);
                }

                // Successfully left the room.
                // Redirect to the application front page.
                window.location.href = "/";

            } catch (error) {

                showError(error.message);

                leaveRoomButton.disabled = false;
            }
        });
    }


    // --------------------------------
    // Initialize chat
    // --------------------------------

    if (!roomId || roomId <= 0) {
        showError("Invalid chatroom.");
        return;
    }

    await connectToChat();

    if (connection.state === signalR.HubConnectionState.Connected) {
        await loadMessages();
    }

    await loadRoomMembers();

});
