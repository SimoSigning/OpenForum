//chat-index.js
document.addEventListener("DOMContentLoaded", () => {

    const privateRoomsList = document.getElementById("privateRoomsList");
    const miscRoomsList = document.getElementById("miscRoomsList");

    const errorElement = document.getElementById("chatError");

    function showError(message) {
        errorElement.textContent = message;
        errorElement.classList.remove("d-none");
    }

    async function loadPrivateRooms() {
        try {
            const response = await fetch("/Chat/MyRooms");

            if (!response.ok) {
                throw new Error("Could not load your private chats.");
            }

            const rooms = await response.json();

            document.getElementById("privateRoomsLoading").classList.add("d-none");

            if (rooms.length === 0) {
                document.getElementById("privateRoomsEmpty").classList.remove("d-none");
                return;
            }

            rooms.forEach(room => {
                const item = document.createElement("a");

                item.href = `/Chat/OpenRoom?roomId=${room.id}`;
                item.className = "list-group-item list-group-item-action";

                const title = document.createElement("strong");
                title.textContent = room.name;

                const details = document.createElement("div");
                details.className = "small text-muted";
                details.textContent = `${room.memberCount} members`;

                item.append(title, details);
                privateRoomsList.appendChild(item);
            });

        } catch (error) {
            showError(error.message);
        }
    }

    async function loadMiscellaneousRooms() {
        try {
            const response = await fetch("/Chat/MiscellaneousRooms");

            if (!response.ok) {
                throw new Error("Could not load community chatrooms.");
            }

            const rooms = await response.json();

            document.getElementById("miscRoomsLoading").classList.add("d-none");

            if (rooms.length === 0) {
                document.getElementById("miscRoomsEmpty").classList.remove("d-none");
                return;
            }

            rooms.forEach(room => {
                const item = document.createElement("div");
                item.className = "list-group-item d-flex justify-content-between align-items-center";

                const info = document.createElement("div");

                const title = document.createElement("strong");
                title.textContent = room.name;

                const details = document.createElement("div");
                details.className = "small text-muted";
                details.textContent = `${room.memberCount} members`;

                info.append(title, details);

                item.appendChild(info);

                if (room.isMember) {
                    const openButton = document.createElement("a");
                    openButton.href = `/Chat/OpenRoom?roomId=${room.id}`;
                    openButton.className = "btn btn-outline-primary btn-sm";
                    openButton.textContent = "Open";

                    item.appendChild(openButton);
                } else {
                    const joinButton = document.createElement("button");
                    joinButton.className = "btn btn-primary btn-sm";
                    joinButton.textContent = "Join";

                    joinButton.addEventListener("click", async () => {
                        await joinRoom(room.id);
                    });

                    item.appendChild(joinButton);
                }

                miscRoomsList.appendChild(item);
            });

        } catch (error) {
            showError(error.message);
        }
    }

    async function joinRoom(roomId) {
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const response = await fetch("/Chat/JoinRoom", {
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
                throw new Error("Could not join the chatroom.");
            }

            window.location.href = `/Chat/OpenRoom?roomId=${roomId}`;

        } catch (error) {
            showError(error.message);
        }
    }

    // --------------------------------
    // Create private chat
    // --------------------------------

    const createChatButton = document.getElementById("createChatButton");

    const createChatModalElement = document.getElementById("createChatModal");

    const createChatModal = new bootstrap.Modal(createChatModalElement);

    const chatFriendsList = document.getElementById("chatFriendsList");

    const createChatError = document.getElementById("createChatError");

    const confirmCreateChatButton = document.getElementById("confirmCreateChatButton");

    async function loadChatFriends() {

        try {
            chatFriendsList.replaceChildren();

            document.getElementById("chatFriendsLoading").classList.remove("d-none");

            document.getElementById("chatFriendsEmpty").classList.add("d-none");

            const response = await fetch("/Chat/Friends");

            if (!response.ok) {
                throw new Error("Could not load your friends.");
            }

            const friends = await response.json();

            document.getElementById("chatFriendsLoading").classList.add("d-none");

            if (friends.length === 0) {
                document.getElementById("chatFriendsEmpty").classList.remove("d-none");
                return;
            }

            friends.forEach(friend => {

                const label = document.createElement("label");

                label.className = "list-group-item d-flex align-items-center gap-2";

                const checkbox = document.createElement("input");

                checkbox.type = "checkbox";
                checkbox.className = "form-check-input mt-0";
                checkbox.value = friend.userId;

                const username = document.createElement("span");

                username.textContent = friend.username;

                label.append(checkbox, username);

                chatFriendsList.appendChild(label);
            });

        } catch (error) {

            document.getElementById("chatFriendsLoading").classList.add("d-none");

            showError(error.message);
        }
    }

    createChatButton.addEventListener("click", async () => {

        createChatError.classList.add("d-none");

        document.getElementById("chatRoomName").value = "";

        await loadChatFriends();

        createChatModal.show();
    });

    confirmCreateChatButton.addEventListener("click", async () => {

        createChatError.classList.add("d-none");

        const name = document.getElementById("chatRoomName").value.trim();

        if (!name) {
            createChatError.textContent = "Please enter a chatroom name.";

            createChatError.classList.remove("d-none");
            return;
        }

        const invitedUserIds = Array.from(chatFriendsList.querySelectorAll('input[type="checkbox"]:checked')).map(checkbox => checkbox.value);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        confirmCreateChatButton.disabled = true;

        try {

            const body = new URLSearchParams();

            body.append("name", name);

            invitedUserIds.forEach(userId => {
                body.append("invitedUserIds", userId);
            });

            body.append("__RequestVerificationToken", token);

            const response = await fetch("/Chat/CreatePrivateRoom", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body
            });

            if (!response.ok) {

                const result = await response.json();

                throw new Error(result.error || "Could not create the chatroom.");
            }

            const result = await response.json();

            window.location.href = `/Chat/OpenRoom?roomId=${result.roomId}`;

        } catch (error) {

            createChatError.textContent = error.message;

            createChatError.classList.remove("d-none");

        } finally {

            confirmCreateChatButton.disabled = false;
        }
    });

    loadPrivateRooms();
    loadMiscellaneousRooms();

});
