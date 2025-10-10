const backendBase = "http://joost.assenbergh.nl:5292/api/User";
const leaderboardBody = document.getElementById("leaderboard-body");
const logoutBtn = document.getElementById("logout-btn");
const welcomeEl = document.getElementById("welcome-text");

// 🔹 Huidige gebruiker laden
async function loadUser() {
  try {
    const userId = sessionStorage.getItem("epicqrizzz-userId");
    if (!userId) {
      window.location.href = "login.html";
      return;
    }

    const res = await fetch(`${backendBase}/GetById/${userId}`);
    if (!res.ok) throw new Error("Netwerkfout");
    const user = await res.json();

    const username = sessionStorage.getItem("epicqrizzz-username") || user.name || "gebruiker";
    welcomeEl.textContent = `🙂 Hallo, ${username}!`;

  } catch (err) {
    console.error("Kon gebruiker niet ophalen:", err);
    window.location.href = "login.html";
  }
}

// 🔹 Leaderboard ophalen
async function loadLeaderboard() {
  try {
    const res = await fetch(`${backendBase}/GetAll`);
    if (!res.ok) throw new Error("Kon leaderboard niet ophalen");

    const users = await res.json();

    // Sorteer op munten (aflopend)
    users.sort((a, b) => b.munten - a.munten);

    // HTML tabel opbouwen
    leaderboardBody.innerHTML = "";
    users.forEach((user, index) => {
      const row = document.createElement("tr");

      // Gouden, zilveren, bronzen icoontjes voor top 3
      let rankIcon = "";
      if (index === 0) rankIcon = "🥇";
      else if (index === 1) rankIcon = "🥈";
      else if (index === 2) rankIcon = "🥉";

      row.innerHTML = `
        <td><strong>${index + 1}</strong> ${rankIcon}</td>
        <td>${user.name}</td>
        <td><i class="bi bi-coin text-warning"></i> ${user.munten}</td>
      `;
      leaderboardBody.appendChild(row);
    });
  } catch (err) {
    console.error("Fout bij laden leaderboard:", err);
    leaderboardBody.innerHTML = `
      <tr><td colspan="3" class="text-danger">Kon leaderboard niet laden</td></tr>
    `;
  }
}

// 🔹 Uitloggen
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

// Init
loadUser();
loadLeaderboard();
