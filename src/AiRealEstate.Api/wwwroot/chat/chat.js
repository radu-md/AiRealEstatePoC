let selectedModel = "GPT 5 nano";
let totalCost = 0.0;

// Lightweight cache to avoid recreating the same suggestion container repeatedly (optional future use)
const uiConfig = {
  avatars: {
    user: "👤",
    assistant: "🤖",
    error: "⚠️"
  }
};

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
    appendMessage("Atentionare", "Te rog introdu un mesaj valid (minim 3 caractere).");
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
    const rawText = await res.text();
    if (!res.ok) {
      renderErrorRaw(res.status, rawText);
      spinner.style.display = 'none';
      sendButton.disabled = false;
      scrollToBottom();
      return;
    }
    let data;
    try { data = JSON.parse(rawText); } catch { data = { response: rawText }; }
    appendMessage("Asistent", data.response);

    if (data.suggestedQuestions) renderSuggestions(data.suggestedQuestions);
    if (data.listings) renderListings(data.listings);
    if (data.requestCost) renderCost(data.requestCost);

    scrollToBottom();

    spinner.style.display = 'none';
    sendButton.disabled = false;
  } catch (e) {
    spinner.style.display = 'none';
    sendButton.disabled = false;
    appendMessage("Eroare", "A apărut o problemă cu rețeaua sau parsarea răspunsului.");
    console.log("Chat error:", e);
  }
}

function renderErrorRaw(status, rawText) {
  let pretty = rawText;
  try {
    const obj = JSON.parse(rawText);
    pretty = JSON.stringify(obj, null, 2);
  } catch { /* leave as is */ }
  const messagesEl = document.getElementById("messages");
  const wrap = document.createElement('div');
  wrap.className = 'message message--error fade-in';
  const avatar = uiConfig.avatars.error || '⚠️';
  wrap.innerHTML = `
    <div class="message__avatar">${avatar}</div>
    <div class="message__bubble">
      <div class="message__meta"><span class="message__sender">Eroare API ${status}</span><span class="message__time">${timeNow()}</span></div>
      <div class="message__content"><pre class="error-json">${escapeHtml(pretty)}</pre></div>
    </div>`;
  messagesEl.appendChild(wrap);
}

function escapeHtml(txt){
  return txt.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

function appendMessage(sender, text) {
  const messagesEl = document.getElementById("messages");
  const msgDiv = document.createElement("div");
  const role = sender === "Eu" ? "user" : (sender === "Eroare" ? "error" : "assistant");
  msgDiv.className = `message message--${role} fade-in`;

  // Basic formatting: preserve line breaks & simple inline code blocks wrapped in backticks
  const safeText = formatMessage(text);
  const avatar = uiConfig.avatars[role] || "💬";
  msgDiv.innerHTML = `
    <div class="message__avatar">${avatar}</div>
    <div class="message__bubble">
      <div class="message__meta"><span class="message__sender">${sender}</span><span class="message__time">${timeNow()}</span></div>
      <div class="message__content">${safeText}</div>
    </div>`;
  messagesEl.appendChild(msgDiv);
}

function formatMessage(text) {
  if (!text) return "";
  // Escape basic HTML
  let escaped = text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
  // Inline code: `code`
  escaped = escaped.replace(/`([^`]+)`/g, '<code class="inline-code">$1</code>');
  // Simple emphasis **bold** and *italic*
  escaped = escaped.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
                   .replace(/\*([^*]+)\*/g, '<em>$1</em>');
  // Line breaks
  return escaped.replace(/\n/g, '<br>');
}

function timeNow() {
  const d = new Date();
  return d.toLocaleTimeString('ro-RO', { hour: '2-digit', minute: '2-digit' });
}

function renderListings(listings) {
  if (!Array.isArray(listings) || listings.length === 0) return;
  const wrapper = document.createElement('div');
  wrapper.className = 'listing-grid fade-in';
  listings.forEach(l => {
    const card = document.createElement('div');
    card.className = 'listing-card';
    const priceVal = l.price ?? l.Price; // support either JSON style
    const price = priceVal ? `${Number(priceVal).toLocaleString('ro-RO')} €` : '';
    const img = l.image || l.Image || '';
    const title = l.title || l.Title || '';
    const link = l.link || l.Link || '#';
    card.innerHTML = `
      <div class="listing-card__image-wrap">${img ? `<img src="${img}" alt="imobil">` : `<div class='img-placeholder'>Fără imagine</div>`}
        ${price ? `<span class="price-badge">${price}</span>` : ''}
      </div>
      <div class="listing-card__body">
        <h4 class="listing-card__title">${title}</h4>
        <div class="listing-card__meta">
          <a class="btn-link" href="${link}" target="_blank" rel="noopener">Detalii ➜</a>
        </div>
      </div>`;
    wrapper.appendChild(card);
  });
  document.getElementById('messages').appendChild(wrapper);
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

function renderCost(cost) {
    if (!cost) return;
    const costDiv = document.createElement("div");
    costDiv.className = "cost-info fade-in";
    costDiv.innerHTML = `
      <div class="cost-info__row"><span>Cost intrare</span><strong>${cost.inputTokens} (${cost.inputCost.toFixed(6)} $)</strong></div>
      <div class="cost-info__row"><span>Cost ieșire</span><strong>${cost.outputTokens} (${cost.outputCost.toFixed(6)} $)</strong></div>
      <div class="cost-info__row total"><span>Total</span><strong>${cost.totalCost.toFixed(6)} $</strong></div>
      <div class="cost-info__time">⏱ ${(cost.processingTimeInMiliseconds / 1000).toFixed(2)} sec</div>`;
    totalCost += cost.totalCost;
    document.getElementById("totalCost").innerText = totalCost.toFixed(8);
    document.getElementById("messages").appendChild(costDiv);
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
            totalCost = 0.0;
            document.getElementById("totalCost").innerText = totalCost.toFixed(8);
            document.getElementById("messages").innerHTML = ""; // Clear messages
            // send first message
            const initialMsg = "am ajuns pe romimo.ro";
            const userInput = document.getElementById("userInput");
            userInput.value = initialMsg;
            sendMessage();
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

  // Auto-send first message when page loads
  const initialMsg = "am ajuns pe romimo.ro";
  const userInput = document.getElementById("userInput");
  userInput.value = initialMsg;
  sendMessage();
});

