const coinDisplay = document.getElementById("coin-balance");
const storeItemsContainer = document.getElementById("store-items");

// Avatars die te koop zijn
const avatars = [
  { emoji: "🤓", price: 0 },   // gratis basis
  { emoji: "😁", price: 20 },
  { emoji: "🤩", price: 30 },
  { emoji: "🥳", price: 30 },
  { emoji: "🐉", price: 50 },
  { emoji: "🤖", price: 75 }
];

// Huidige coins ophalen uit localStorage
let coins = parseInt(localStorage.getItem("epicqrizzz-coins")) || 0;
let ownedAvatars = JSON.parse(localStorage.getItem("epicqrizzz-owned")) || ["🙂"]; // standaard gratis
let currentAvatar = localStorage.getItem("epicqrizzz-avatar") || "🙂";

coinDisplay.textContent = coins;

// Store renderen
function renderStore() {
  storeItemsContainer.innerHTML = "";

  avatars.forEach(item => {
    const div = document.createElement("div");
    div.classList.add("store-item");

    const emoji = document.createElement("div");
    emoji.textContent = item.emoji;
    emoji.classList.add("store-avatar");

    const price = document.createElement("p");
    price.textContent = item.price === 0 ? "Gratis" : `${item.price} coins`;

    const btn = document.createElement("button");

    if (ownedAvatars.includes(item.emoji)) {
      if (currentAvatar === item.emoji) {
        btn.textContent = "Geselecteerd ✅";
        btn.disabled = true;
      } else {
        btn.textContent = "Selecteer";
        btn.addEventListener("click", () => selectAvatar(item.emoji));
      }
    } else {
      btn.textContent = "Koop";
      btn.addEventListener("click", () => buyAvatar(item));
    }

    div.appendChild(emoji);
    div.appendChild(price);
    div.appendChild(btn);
    storeItemsContainer.appendChild(div);
  });
}

function buyAvatar(item) {
  if (coins >= item.price) {
    coins -= item.price;
    ownedAvatars.push(item.emoji);
    localStorage.setItem("epicqrizzz-coins", coins);
    localStorage.setItem("epicqrizzz-owned", JSON.stringify(ownedAvatars));
    alert(`Je hebt ${item.emoji} gekocht!`);
    updateCoins();
    renderStore();
  } else {
    alert("Niet genoeg coins! Speel meer om munten te verdienen.");
  }
}

function selectAvatar(emoji) {
  currentAvatar = emoji;
  localStorage.setItem("epicqrizzz-avatar", emoji);
  alert(`Je hebt ${emoji} geselecteerd als je avatar.`);
  renderStore();
}

function updateCoins() {
  coinDisplay.textContent = coins;
}

renderStore();
