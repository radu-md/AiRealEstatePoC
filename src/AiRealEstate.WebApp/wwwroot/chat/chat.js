function sendMessage() {
  let sessionId = localStorage.getItem("chatSessionId");
  if (!sessionId) {
    sessionId = crypto.randomUUID();
    localStorage.setItem("chatSessionId", sessionId);
  }

  const userInput = document.getElementById("userInput");
  const input = userInput.value;

  appendMessage("Eu", input);
  userInput.value = "";

  fetch("https://localhost:7191/api/chat", {
    method: "POST",
    headers: { 
      "Content-Type": "application/json",
      "X-Session-Id": sessionId 
    },
    body: JSON.stringify({ message: input })
  })
  .then(res => res.json())
  .then(data => {
    appendMessage("Asistent", data.response);

    if (data.suggestedQuestions) renderSuggestions(data.suggestedQuestions);
    if (data.listings) renderListings(data.listings);

    scrollToBottom();
  })
  .catch(err => {
    appendMessage("Eroare", "A apărut o problemă cu serverul.");
    console.error(err);
  });
}

function appendMessage(sender, text) {
  const msgDiv = document.createElement("div");
  msgDiv.className = "message";
  msgDiv.innerHTML = `<strong>${sender}:</strong> ${text}`;
  document.getElementById("messages").appendChild(msgDiv);
}

function renderListings(listings) {
  listings.forEach(listing => {
    const card = document.createElement("div");
    card.className = "listing-card";
    card.innerHTML = `
      <img src="${listing.image}" alt="poza">
      <h4>${listing.title}</h4>
      <a href="${listing.link}" target="_blank">Vezi detalii</a>
    `;
    document.getElementById("messages").appendChild(card);
  });
}

function renderSuggestions(questions) {
  const container = document.createElement("div");
  container.className = "suggestions";
  questions.forEach(q => {
    if (!q.includes("?")) {
    const btn = document.createElement("button");
    btn.innerText = q;    
    btn.onclick = () => {
      document.getElementById("userInput").value = q;
      sendMessage();
    };
    container.appendChild(btn);    
    }
    else {
      const suggestion = document.createElement("div");
      suggestion.className = "suggestion";
      suggestion.innerText = q;
      container.appendChild(suggestion);
    }
  });
  document.getElementById("messages").appendChild(container);
}

function scrollToBottom() {
  const messages = document.getElementById("messages");
  messages.scrollTop = messages.scrollHeight;
}

document.addEventListener("DOMContentLoaded", function () {
  document.getElementById("userInput").addEventListener("keypress", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      sendMessage();
    }
  });
});