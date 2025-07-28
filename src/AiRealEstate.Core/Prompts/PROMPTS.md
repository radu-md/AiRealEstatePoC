# 🧠 PROMPTS.md – AI Search Agent (romimo.ro)

Acest fișier documentează prompturile utilizate pentru modelul GPT (Azure OpenAI via Semantic Kernel) în cadrul proiectului **AI Search Agent pentru romimo.ro**.

---

## 📌 1. ExtractUserPreferencesPrompt.txt

### 📄 Descriere
Promptul este folosit pentru a extrage preferințele imobiliare ale utilizatorului (oraș, tip locuință, buget etc.) dintr-un mesaj conversațional în limba română. Este apelat în cadrul skill-ului `ExtractUserPreferencesSkill`.

### 🔁 Format rezultat așteptat
```json
{
  "PropertyType": "...",
  "TransactionType": "...",
  "County": "...",
  "City": "...",
  "MaxPrice": ..., // numeric
  "TextFilter": "..."
}
```

### 🔧 Mapping semantic
- **TransactionType**:
  - "cumpăr", "achiziționez", "de vânzare", "caut o locuință de vânzare" → `"vanzare"`
  - "închiriez", "închiriere", "chirie" → `"de-inchiriat"`

- **PropertyType**:
  - normalizează „locuință”, „imobil” la `"apartamente"`
  - acceptă: `"garsoniere"`, `"apartamente"`, `"case"`, `"terenuri"`, etc.

- **County**: nume județ cu litere mici fără diacritice (ex: "cluj", "timis", "ilfov")
- **City**: localitate cu liniuțe și litere mici (ex: "cluj-napoca", "targu-mures")
- **MaxPrice**: extras numeric (fără simboluri)
- **TextFilter**: expresii libere (ex: "lângă metrou", "zonă liniștită")

### 📌 Prompt (conținut)
```text
Ești un asistent imobiliar care extrage preferințele utilizatorului dintr-un mesaj conversațional în limba română.

Returnează un obiect JSON cu următoarele câmpuri:
{
  "PropertyType": "...",
  "TransactionType": "...",
  "County": "...",
  "City": "...",
  "MaxPrice": ...,
  "TextFilter": "..."
}

🔹 Reguli de transformare și interpretare:
- vezi mapping-ul semantic de mai sus

🔹 Instrucțiuni finale:
- Dacă nu găsești o valoare pentru un câmp, lasă-l cu valoarea `null` sau lipsă
- Returnează **doar** obiectul JSON — fără explicații, fără text înainte sau după

Mesajul utilizatorului:
{{userMessage}}

JSON:
```

---

## 🧠 Recomandări
- Nu modifica structura fără a testa impactul în `ExtractUserPreferencesSkill`
- Evită comentarii sau exemple adiționale în outputul generat
- Poate fi extins pentru `MinPrice`, `NrCamere`, `Etaj`, `Zona` în versiuni viitoare

---

## 📂 Locație fișier în proiect
```
AiRealEstate.Core/
└── Prompts/
    └── ExtractUserPreferencesPrompt.txt
```

---

## 🔧 Utilizare în cod
```csharp
var preferences = await _extractUserPreferencesSkill.ExtractAsync(userMessage);
```

---