# 📡 README_API.md – API Reference for AiRealEstatePoC

This document describes the HTTP endpoints exposed by the AI Real Estate Proof-of-Concept backend API, implemented in **AiRealEstate.Api**.

---

## 🔗 Base URL

Local development (default):
```
https://localhost:7191
```

---

## 🧠 `POST /api/chat`

Handles user input and returns an AI-generated reply, including structured real estate preferences, listing suggestions, and follow-up questions.

### 🔸 Request
```
POST /api/chat
Content-Type: application/json
X-Session-Id: <GUID>
```

#### Body
```json
{
  "message": "Caut un apartament de închiriat în Timișoara, aproape de parc."
}
```

If `X-Session-Id` is not provided, one will be generated automatically. It is used to maintain short-term conversation context.

---

### 🔸 Response
Returns a `200 OK` with a structured `ChatResult`.

#### JSON structure
```json
{
  "response": "Am găsit un apartament în Timișoara, aproape de parc. Vrei să-l vezi?",
  "listings": [
    {
      "title": "Apartament 2 camere - Timișoara Central",
      "price": "450 EUR",
      "image": "https://www.romimo.ro/img/apartament.jpg",
      "link": "https://www.romimo.ro/inchiriere/apartamente/timisoara/..."
    }
  ],
  "suggestedQuestions": [
    "Ai un buget maxim în minte?",
    "Vrei să fie aproape de o școală?"
  ]
}
```

---

## 🧾 `ChatResult` model

This is the main output object returned by the API.

### Properties:
- `response` *(string, required)*: Textual AI reply to user message.
- `listings` *(array of Listing, optional)*: Parsed real estate offers from romimo.ro.
- `suggestedQuestions` *(array of string, optional)*: Dynamic questions to continue the conversation.

---

## 🏘️ `Listing` model

Each object in `listings` has the following fields:

### Properties:
- `title` *(string)*: Short headline of the property.
- `price` *(string)*: Price with currency.
- `image` *(string)*: Image URL from the listing.
- `link` *(string)*: Direct URL to romimo.ro offer.

---

## 📌 Notes
- The AI uses Semantic Kernel + Azure OpenAI (GPT-4o) for natural language processing.
- Extracted preferences are stored per session (max last 10 messages).
- The API does not persist user data. No login is required.

---

## 🔍 Example curl
```bash
curl -X POST https://localhost:7191/api/chat \
  -H "Content-Type: application/json" \
  -H "X-Session-Id: 12345678-abcd-ef00-1234-56789abcdef0" \
  -d '{"message": "Caut un apartament în Cluj, sub 500 EUR"}'
```

---

For more on the prompt logic and AI templates, see [`PROMPTS.md`](./AiRealEstate.Core/Prompts/PROMPTS.md).