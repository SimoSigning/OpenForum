
document.addEventListener("DOMContentLoaded", async () => {

    const roomId = window.chatRoomId;

    const messagesContainer = document.getElementById("messagesContainer");
    const messagesLoading = document.getElementById("messagesLoading");
    const messageForm = document.getElementById("messageForm");
    const messageInput = document.getElementById("messageInput");
    const sendMessageButton = document.getElementById("sendMessageButton");
    const errorElement = document.getElementById("chatError");

    const MAX_MESSAGES = 100;
    const messages = new Map();

    let connection = null;
    let isPageLeaving = false;
    let isLeavingRoom = false;

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

            messageElement.className = isOwnMessage
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

            timestamp.className = isOwnMessage
                ? "d-block mt-2 text-white-50"
                : "d-block mt-2 text-muted";

            const sentAt = new Date(message.sentAt);
            timestamp.textContent = sentAt.toLocaleString();

            messageElement.append(sender, content, timestamp);
            messagesContainer.appendChild(messageElement);
        });

        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    function addMessage(message) {

        if (isPageLeaving || isLeavingRoom)
            return;

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

        if (isPageLeaving || isLeavingRoom)
            return;

        try {

            const response = await fetch(
                `/Chat/Messages?roomId=${roomId}`
            );

            if (!response.ok) {
                throw new Error("Could not load chat messages.");
            }

            const loadedMessages = await response.json();

            // The user may have started leaving while the request was running.
            if (isPageLeaving || isLeavingRoom)
                return;

            loadedMessages.forEach(message => {
                addMessage(message);
            });

            messagesLoading.classList.add("d-none");

        } catch (error) {

            if (!isPageLeaving && !isLeavingRoom) {
                showError(error.message);
                messagesLoading.classList.add("d-none");
            }
        }
    }

    // --------------------------------
    // SignalR connection
    // --------------------------------

    async function connectToChat() {

        // Do not create or reconnect while leaving the page or room.
        if (isPageLeaving || isLeavingRoom)
            return;

        // Avoid creating a duplicate connection.
        if (connection &&
            connection.state !== signalR.HubConnectionState.Disconnected) {
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .build();

        // Receive messages from other users.
        connection.on("ReceiveMessage", message => {

            if (!isPageLeaving && !isLeavingRoom) {
                addMessage(message);
            }
        });

        // Receive notifications when room membership changes.
        connection.on("RoomMembersChanged", changedRoomId => {

            if (isPageLeaving || isLeavingRoom)
                return;

            if (Number(changedRoomId) !== Number(roomId))
                return;

            loadRoomMembers();
        });

        // Handle deletion of the current chatroom.
        connection.on("RoomDeleted", async deletedRoomId => {

            if (Number(deletedRoomId) !== Number(roomId))
                return;

            if (isPageLeaving)
                return;

            // Prevent further room operations.
            isPageLeaving = true;

            // Stop the SignalR connection.
            if (connection) {
                try {
                    await connection.stop();
                } catch {
                    // The connection may already have stopped.
                }
            }

            // Return the user to the chat overview.
            window.location.replace("/Chat/Index");
        });

        // Handle a lost connection.
        connection.onreconnecting(() => {

            if (isPageLeaving || isLeavingRoom)
                return;

            showError("Connection lost. Reconnecting...");
            sendMessageButton.disabled = true;
        });

        // Rejoin the room after SignalR reconnects.
        connection.onreconnected(async () => {

            if (isPageLeaving || isLeavingRoom)
                return;

            clearError();

            try {

                const joined = await connection.invoke("JoinRoom", roomId);

                if (isPageLeaving || isLeavingRoom)
                    return;

                // The server rejected the join because the user is no longer
                // a member of the room.
                if (!joined) {
                    window.location.replace("/Chat/Index");
                    return;
                }

                await loadMessages();

                if (isPageLeaving || isLeavingRoom)
                    return;

                sendMessageButton.disabled = false;

            } catch (error) {

                if (!isPageLeaving && !isLeavingRoom) {
                    showError("Could not rejoin the chatroom.");
                    sendMessageButton.disabled = true;
                }
            }
        });

        // Handle a permanently closed connection.
        connection.onclose(() => {

            if (isPageLeaving || isLeavingRoom)
                return;

            showError(
                "Connection to the chatroom was lost. Please refresh the page."
            );

            sendMessageButton.disabled = true;
        });

        // Start the SignalR connection.
        try {

            await connection.start();

            // The user may have started leaving while SignalR was connecting.
            if (isPageLeaving || isLeavingRoom) {
                await connection.stop();
                return;
            }

            const joined = await connection.invoke("JoinRoom", roomId);

            // The user may have started leaving while JoinRoom was executing.
            if (isPageLeaving || isLeavingRoom) {
                await connection.stop();
                return;
            }

            // The server rejected the join because the user is no longer
            // a member of the room.
            if (!joined) {
                await connection.stop();
                window.location.replace("/Chat/Index");
                return;
            }

            clearError();

        } catch (error) {

            if (!isPageLeaving && !isLeavingRoom) {
                showError("Could not connect to the chatroom.");
                sendMessageButton.disabled = true;
            }
        }
    }

    // --------------------------------
    // Page lifecycle
    // --------------------------------

    // Stop SignalR when the user navigates away or closes the page.
    // This allows the server's OnDisconnectedAsync to run.
    window.addEventListener("pagehide", () => {

        isPageLeaving = true;

        if (connection) {
            connection.stop().catch(() => {
                // The connection may already have stopped.
            });
        }
    });

    // If the browser restores the page from its back-forward cache,
    // establish a fresh SignalR connection.
    window.addEventListener("pageshow", async event => {

        if (!event.persisted)
            return;

        isPageLeaving = false;

        if (isLeavingRoom)
            return;

        await connectToChat();

        if (connection &&
            connection.state === signalR.HubConnectionState.Connected &&
            !isLeavingRoom) {

            await loadMessages();
            await loadRoomMembers();
        }
    });

    // --------------------------------
    // Send messages
    // --------------------------------

    messageForm.addEventListener("submit", async event => {

        event.preventDefault();

        if (isPageLeaving || isLeavingRoom)
            return;

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

            await connection.invoke("SendMessage", roomId, content);

            if (isPageLeaving || isLeavingRoom)
                return;

            messageInput.value = "";
            messageInput.focus();

        } catch (error) {

            if (!isPageLeaving && !isLeavingRoom) {
                showError("Could not send your message.");
            }

        } finally {

            if (!isPageLeaving && !isLeavingRoom) {
                sendMessageButton.disabled =
                    !connection ||
                    connection.state !== signalR.HubConnectionState.Connected;
            }
        }
    });

    // --------------------------------
    // Room members
    // --------------------------------

    async function loadRoomMembers() {

        if (isPageLeaving || isLeavingRoom)
            return;

        const membersList = document.getElementById("roomMembersList");
        const loading = document.getElementById("roomMembersLoading");

        try {

            const response = await fetch(
                `/Chat/RoomMembers?roomId=${roomId}`
            );

            if (!response.ok) {
                throw new Error("Could not load room members.");
            }

            const members = await response.json();

            // The user may have started leaving while the request was running.
            if (isPageLeaving || isLeavingRoom)
                return;

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

            if (!isPageLeaving && !isLeavingRoom) {
                showError(error.message);
            }

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

        const inviteModal = new bootstrap.Modal(inviteModalElement);

        const inviteFriendsList =
            document.getElementById("inviteFriendsList");

        const inviteFriendsError =
            document.getElementById("inviteFriendsError");

        const inviteFriendsSuccess =
            document.getElementById("inviteFriendsSuccess");

        async function loadAvailableFriends() {

            if (isPageLeaving || isLeavingRoom)
                return;

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

                if (isPageLeaving || isLeavingRoom)
                    return;

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

                if (!isPageLeaving && !isLeavingRoom) {
                    inviteFriendsError.textContent = error.message;
                    inviteFriendsError.classList.remove("d-none");
                }
            }
        }

        async function inviteFriend(userId, button) {

            if (isPageLeaving || isLeavingRoom)
                return;

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

                if (isPageLeaving || isLeavingRoom)
                    return;

                inviteFriendsSuccess.textContent =
                    "Friend successfully invited.";

                inviteFriendsSuccess.classList.remove("d-none");

                await loadRoomMembers();
                await loadAvailableFriends();

                if (isPageLeaving || isLeavingRoom)
                    return;

                inviteFriendsSuccess.textContent =
                    "Friend successfully invited.";

                inviteFriendsSuccess.classList.remove("d-none");

            } catch (error) {

                if (!isPageLeaving && !isLeavingRoom) {
                    inviteFriendsError.textContent = error.message;
                    inviteFriendsError.classList.remove("d-none");
                    button.disabled = false;
                }
            }
        }

        inviteFriendsButton.addEventListener("click", async () => {

            if (isPageLeaving || isLeavingRoom)
                return;

            await loadAvailableFriends();

            if (isPageLeaving || isLeavingRoom)
                return;

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

            if (isPageLeaving || isLeavingRoom)
                return;

            clearError();

            const confirmed = confirm(
                "Are you sure you want to leave this chatroom?"
            );

            if (!confirmed)
                return;

            // Prevent reconnects and further room operations.
            isLeavingRoom = true;

            const token = document.querySelector(
                'input[name="__RequestVerificationToken"]'
            )?.value;

            leaveRoomButton.disabled = true;
            sendMessageButton.disabled = true;

            try {

                // Stop SignalR BEFORE removing the database membership.
                // This prevents a reconnect from trying to rejoin the room.
                if (connection) {
                    try {
                        await connection.stop();
                    } catch {
                        // The connection may already have stopped.
                    }
                }

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

                        if (result.error) {
                            errorMessage = result.error;
                        }

                    } catch {
                        // Keep the default error message.
                    }

                    throw new Error(errorMessage);
                }

                // Successfully left the room.
                window.location.replace("/");

            } catch (error) {

                // The leave request failed. Allow the user to try again.
                isLeavingRoom = false;

                showError(error.message);

                leaveRoomButton.disabled = false;

                // The previous SignalR connection was stopped.
                // Reconnect so the user can continue using the room.
                await connectToChat();

                if (connection &&
                    connection.state === signalR.HubConnectionState.Connected &&
                    !isLeavingRoom) {

                    sendMessageButton.disabled = false;
                }
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

    if (connection &&
        connection.state === signalR.HubConnectionState.Connected &&
        !isLeavingRoom) {

        await loadMessages();
        await loadRoomMembers();
    }

});
