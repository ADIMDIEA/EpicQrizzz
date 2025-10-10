const backendBase = "http://joost.assenbergh.nl:5292/api/User";

const avatarDisplay = document.getElementById("avatar-display");
const adminBtn = document.getElementById("admin-btn");
const logoutBtn = document.getElementById("logout-btn");

// ===== Alle avatars =====
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

// Huidige gebruiker ophalen (sessie/cookie uit backend)
async function loadUser() {
  try {
    const userId = sessionStorage.getItem("epicqrizzz-userId");
    if (!userId) {
      window.location.href = "login.html";
      return;
    }

    const res = await fetch(`${backendBase}/GetById/${userId}`);
    if (!res.ok) throw new Error("Netwerkfout");

    const uuid = await res.json();
    if (!uuid) throw new Error("Geen gebruiker gevonden");

    const username = sessionStorage.getItem("epicqrizzz-username");
    const welcomeEl = document.getElementById("welcome-text");

    // ===== Avatar ophalen uit sessionStorage =====
    const selectedAvatarId = sessionStorage.getItem("selectedAvatarId");
    const avatarEmoji = selectedAvatarId && allAvatars[selectedAvatarId]
      ? allAvatars[selectedAvatarId]
      : "🙂";

    // Tekst tonen met emoji en gebruikersnaam
    if (username) {
      welcomeEl.textContent = `${avatarEmoji} Hallo, ${username}!`;
    } else {
      welcomeEl.textContent = `${avatarEmoji} Hallo, gebruiker!`;
    }

  } catch (err) {
    console.error("Kon gebruiker niet ophalen:", err);
    window.location.href = "login.html";
  }
}

// Uitloggen
logoutBtn.addEventListener("click", async () => {
  try {
    await fetch(`${backendBase}/Logout`, {
      method: "POST",
      credentials: "include"
    });
  } catch (err) {
    console.error("Fout bij uitloggen:", err);
  }
  window.location.href = "login.html";
});

loadUser();
