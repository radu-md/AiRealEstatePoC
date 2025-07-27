function sendMessage() {
  const inputEl = document.getElementById("userInput");
  const input = inputEl.value;
  if (!input.trim()) return;

  appendMessage("Tu", input);
  inputEl.value = "";

  fetch("/api/chat", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ message: input })
  })
  .then(res => res.json())
  .then(data => {
    appendMessage("Asistent", data.response);

    if (data.listings) renderListings(data.listings);
    if (data.suggestedQuestions) renderSuggestions(data.suggestedQuestions);

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
      <p>${listing.price}</p>
      <a href="${listing.link}" target="_blank">Vezi detalii</a>
    `;
    document.getElementById("messages").appendChild(card);
  });
}

function renderSuggestions(questions) {
  const container = document.createElement("div");
  container.className = "suggestions";
  questions.forEach(q => {
    const btn = document.createElement("button");
    btn.innerText = q;
    btn.onclick = () => {
      document.getElementById("userInput").value = q;
      sendMessage();
    };
    container.appendChild(btn);
  });
  document.getElementById("messages").appendChild(container);
}

function scrollToBottom() {
  const messages = document.getElementById("messages");
  messages.scrollTop = messages.scrollHeight;
}