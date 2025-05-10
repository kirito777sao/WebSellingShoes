// chatbot.js
document.addEventListener("DOMContentLoaded", () => {
    const bubble = document.getElementById("chatbot-bubble");
    const widget = document.getElementById("chatbot");
    const history = document.getElementById("history");
    const input = document.querySelector(".chat-input");
    const sendBtn = document.querySelector(".send-message-btn");

    // Giữ trạng thái đóng khi reload
    bubble.style.display = 'flex';
    widget.style.display = 'none';

    bubble.addEventListener("click", () => {
        widget.style.display = 'flex';
        bubble.style.display = 'none';
        scrollToBottom();
    });

    document.querySelector(".close-btn").addEventListener("click", () => {
        widget.style.display = 'none';
        bubble.style.display = 'flex';
    });

    sendBtn.addEventListener("click", async () => {
        const msg = input.value.trim();
        if (!msg) return;

        // Hiển thị tin nhắn user
        const userDiv = document.createElement('div');
        userDiv.className = 'message user-msg';
        userDiv.textContent = msg;
        history.appendChild(userDiv);
        input.value = '';
        scrollToBottom();

        try {
            const res = await fetch("http://127.0.0.1:8000/chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: msg, user_id: 1 })
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const { reply } = await res.json();

            const botDiv = document.createElement('div');
            botDiv.className = 'message bot-message';
            botDiv.innerHTML = `<p>${reply}</p>`;
            history.appendChild(botDiv);
        } catch (e) {
            const errDiv = document.createElement('div');
            errDiv.className = 'message error';
            errDiv.innerHTML = `<p>Error: ${e.message}</p>`;
            history.appendChild(errDiv);
        }
        scrollToBottom();
    });

    function scrollToBottom() {
        history.scrollTop = history.scrollHeight;
    }
});