document.addEventListener("submit", async function (event) {
    const form = event.target.closest("form.js-like-form");

    if (!form) {
        return;
    }

    event.preventDefault();

    const button = form.querySelector("[data-like-button]");

    if (!button || button.disabled) {
        return;
    }

    const label = button.querySelector("[data-like-label]");
    const count = button.querySelector("[data-like-count]");

    button.disabled = true;

    try {
        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "Accept": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("The like request failed.");
        }

        const result = await response.json();

        // Update only the button that was clicked.
        if (label) {
            label.textContent = result.isLiked
                ? "♥ Liked"
                : "♡ Like";
        }

        if (count) {
            count.textContent = result.likeCount;
        }

        button.classList.toggle("btn-primary", result.isLiked);
        button.classList.toggle("btn-outline-primary", !result.isLiked);
        button.setAttribute("aria-pressed", String(result.isLiked));
    } catch (error) {
        console.error("Could not update like:", error);
        alert("Could not update your like. Please try again.");
    } finally {
        button.disabled = false;
    }
});

document.addEventListener("click", async function (event) {
    const link = event.target.closest(".show-likers");
    if (!link) return;

    event.preventDefault();

    const type = link.dataset.type;
    const id = link.dataset.id;

    if (!["post", "comment"].includes(type) || !id) return;

    const modalElement = document.getElementById("likersModal");
    const list = document.getElementById("likersList");
    const loading = document.getElementById("likersLoading");
    const error = document.getElementById("likersError");
    const empty = document.getElementById("likersEmpty");

    list.replaceChildren();
    loading.classList.remove("d-none");
    error.classList.add("d-none");
    empty.classList.add("d-none");

    document.getElementById("likersModalLabel").textContent =
        "People who liked this";

    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
    modal.show();

    const endpoint = type === "post"
        ? `/Like/GetPostLikers?postId=${encodeURIComponent(id)}`
        : `/Like/GetCommentLikers?commentId=${encodeURIComponent(id)}`;

    try {
        const response = await fetch(endpoint, {
            headers: { "Accept": "application/json" }
        });

        if (!response.ok) throw new Error("Failed to load likes.");

        const usernames = await response.json();

        loading.classList.add("d-none");

        if (!Array.isArray(usernames) || usernames.length === 0) {
            empty.classList.remove("d-none");
            return;
        }

        for (const username of usernames) {
            const item = document.createElement("li");
            item.className = "list-group-item";
            item.textContent = username;
            list.appendChild(item);
        }
    } catch {
        loading.classList.add("d-none");
        error.classList.remove("d-none");
    }
});