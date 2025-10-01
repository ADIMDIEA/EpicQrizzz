const backendBase = "http://joost.assenbergh.nl:5292/api/User";

const avatarDisplay = document.getElementById("avatar-display");
const adminBtn = document.getElementById("admin-btn");
const logoutBtn = document.getElementById("logout-btn");

// Huidige gebruiker ophalen (sessie/cookie uit backend)
async function loadUser() {
  try {

    const res = await fetch(`${backendBase}/GetById/${sessionStorage.getItem("epicqrizzz-userId")}`);
    if (!res.ok) throw new Error("Netwerkfout");
    const uuid = await res.json();
    if (!uuid) throw new Error("Geen gebruiker gevonden");
    const username = sessionStorage.getItem("epicqrizzz-username");

    // Zoek de span op
    const welcomeEl = document.getElementById("welcome-text");

    // Pas de tekst aan
    if (username) {
      welcomeEl.textContent = `🙂 Hallo, ${username}!`;
    } else {
      welcomeEl.textContent = "🙂 Hallo, gebruiker!";
    }

    if (!sessionStorage.getItem("epicqrizzz-userId")) {
      // niet ingelogd, terug naar login
      window.location.href = "login.html";
      return;
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
