// ===== Configuratie =====
const API_BASE = "http://joost.assenbergh.nl:5295/api/Inventory"; // pas aan naar jouw backend-poort
const userId = sessionStorage.getItem("epicqrizzz-userId"); // tijdelijk, later dynamisch via login

// ===== DOM =====
const avatarPreview = document.getElementById("avatar-preview");
const inventoryContainer = document.getElementById("inventory-container");
const saveBtn = document.getElementById("save-btn");

// ===== 16 avatars =====
const allAvatars = {
  1: "🙂",
  2: "😎",
  3: "👩‍⚕️",
  4: "👨‍⚕️",
  5: "👽",
  6: "👻",
  7: "🤖",
  8: "🐉",
  9: "🦊",
  10: "🐧",
  11: "🐵",
  12: "🐸",
  13: "🐼",
  14: "🦁",
  15: "🐰",
  16: "🐱"
};

let userInventory = []; // itemId’s die de user heeft
let selectedAvatar = null;

// ===== Laad inventory van gebruiker =====
async function loadInventory() {
  try {
    const res = await fetch(`${API_BASE}/GetById/${userId}`);
    if (!res.ok) throw new Error("Kon inventory niet ophalen.");

    const data = await res.json();
    userInventory = data.map(item => item.itemId);

    // Bouw avatar lijst
    inventoryContainer.innerHTML = "";
    userInventory.forEach(id => {
      const emoji = allAvatars[id];
      if (emoji) {
        const div = document.createElement("div");
        div.classList.add("fs-1", "avatar-option");
        div.textContent = emoji;
        div.addEventListener("click", () => selectAvatar(id, emoji, div));
        inventoryContainer.appendChild(div);
      }
    });

    // Kijk of er al iets in sessionStorage staat
    const saved = sessionStorage.getItem("selectedAvatarId");
    if (saved && allAvatars[saved]) {
      selectedAvatar = parseInt(saved);
      avatarPreview.textContent = allAvatars[selectedAvatar];
      highlightSelected();
    }
  } catch (err) {
    console.error(err);
  }
}

// ===== Avatar selecteren =====
function selectAvatar(id, emoji, element) {
  selectedAvatar = id;
  avatarPreview.textContent = emoji;
  sessionStorage.setItem("selectedAvatarId", id);
  highlightSelected();
}

// ===== Highlight geselecteerde =====
function highlightSelected() {
  const options = document.querySelectorAll(".avatar-option");
  options.forEach(opt => {
    opt.classList.remove("border", "border-primary", "border-3");
    if (opt.textContent === allAvatars[selectedAvatar]) {
      opt.classList.add("border", "border-primary", "border-3");
    }
  });
}

// ===== Opslaan (equip) =====
saveBtn.addEventListener("click", async () => {
  if (!selectedAvatar) {
    alert("Kies eerst een avatar!");
    return;
  }

  try {
    const res = await fetch(`${API_BASE}/Equip/${selectedAvatar}/${userId}`, {
      method: "POST"
    });

    if (res.ok) {
      alert("Avatar succesvol uitgerust!");
    } else {
      alert("Er is iets misgegaan bij het opslaan.");
    }
  } catch (err) {
    console.error("Fout bij opslaan:", err);
  }
});

// ===== Start =====
loadInventory();
