# 💬 README_FRONTEND.md – Frontend Overview for AiRealEstatePoC

This document describes the structure and behavior of the frontend UI for the AI Real Estate Proof-of-Concept application.

---

## 📁 Location
The frontend is located in:
```
src/AiRealEstate.WebApp/
```
It is a static HTML + JavaScript UI that interacts with the backend API exposed by `AiRealEstate.Api`.

---

## 🧩 Files

### 📄 `index.html`
- Basic HTML structure for a chat interface.
- Includes:
  - `#chat-container`: container for messages and input
  - `#messages`: area where conversation is rendered
  - `#input-area`: input field + send button
- Loads `chat.js` for logic

### 📄 `chat.js`
Handles all frontend behavior and communication with the backend API.

---

## ⚙️ Core Logic (chat.js)

### ✅ Session ID
- A unique `chatSessionId` is generated via `crypto.randomUUID()` and stored in `localStorage`.
- This ID is sent as a header (`X-Session-Id`) with every API call to retain conversation context.

### ✅ Sending Messages
```js
fetch("https://localhost:7191/api/chat", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    "X-Session-Id": sessionId
  },
  body: JSON.stringify({ message: input })
})
```
- The user message is sent to the backend.
- The response contains:
  - `response`: AI reply
  - `listings`: optional list of properties
  - `suggestedQuestions`: follow-up questions

### ✅ Rendering
- `appendMessage(sender, text)` adds a message to the chat
- `renderListings(listings)` creates cards with image, title, price, and link
- `renderSuggestions(questions)` adds clickable buttons to auto-fill the input
- `scrollToBottom()` ensures UI scrolls with conversation

### ✅ Enter Key Handling
```js
document.getElementById("userInput").addEventListener("keypress", function (e) {
  if (e.key === "Enter") {
    e.preventDefault();
    sendMessage();
  }
});
```

---

## 🎨 Styling
- Styling is expected from `style.css` (not detailed here)
- Customize `.message`, `.listing-card`, `.suggestions` classes as needed

---

Make sure the backend API (`AiRealEstate.Api`) is running and accessible from browser.

---

## 🔧 Future Improvements
- Add loading indicators or message delays for realism
- Support for richer cards (e.g., property type icons, map embeds)
- Input validation and error handling
- Responsive design for mobile usage

---

## 🔍 Example Workflow
1. User types: "Caut un apartament în Cluj-Napoca, aproape de parc."
2. Frontend sends it to `/api/chat`
3. Backend replies with a structured `ChatResult`
4. UI displays AI reply, property card, and 2–3 suggested buttons
5. User clicks "Sub 100.000 EUR" → message sent again

---