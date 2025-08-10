let selectedModel = "GPT 5 mini";

async function sendMessage() {  
  const spinner = document.getElementById("spinner");
  spinner.style.display = 'block';
  const sendButton = document.getElementById("sendButton");
  sendButton.disabled = true;
  let sessionId = localStorage.getItem("chatSessionId");
  if (!sessionId) {
    sessionId = crypto.randomUUID();
    localStorage.setItem("chatSessionId", sessionId);
  }

  const userInput = document.getElementById("userInput");
  const input = userInput.value;
  if (!input || input.trim() === "" || input.length < 3) {
    appendMessage("Eroare", "Te rog introdu un mesaj valid (minim 3 caractere).");
    spinner.style.display = 'none';
    sendButton.disabled = false;
    return;
  }

  const trimmed = input.trim();
  appendMessage("Eu", trimmed);
    userInput.value = "";
    scrollToBottom();

  try {
    const res = await fetch('/api/chat', {
      method: "POST",
      headers: { "Content-Type": "application/json", "X-Session-Id": sessionId },
        body: JSON.stringify({ model: selectedModel, message: trimmed })
    });

    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();    
    appendMessage("Asistent", data.response);

    if (data.suggestedQuestions) renderSuggestions(data.suggestedQuestions);
    if (data.listings) renderListings(data.listings);

    scrollToBottom();

    spinner.style.display = 'none';
    sendButton.disabled = false;
  } catch (e) {
    spinner.style.display = 'none';
    sendButton.disabled = false;
    appendMessage("Eroare", "A apărut o problemă cu serverul.");
    console.log("Chat error:", e);
  }
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

function updateSelectedModel() {
    document.getElementById("selectedModel").innerText = getSelectedModel();
}

function getSelectedModel() {
    const radios = document.getElementsByName('modelSelect');
    for (let i = 0; i < radios.length; i++) {
        if (radios[i].checked) {
            selectedModel = radios[i].value;
            return selectedModel;
        }
    }
    return selectedModel;
}

document.addEventListener("DOMContentLoaded", function () {
  document.getElementById("userInput").addEventListener("keypress", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      sendMessage();
    }
  });
});