const backendBase = "http://joost.assenbergh.nl:5292/api/User";
const leaderboardBody = document.getElementById("leaderboard-body");
const logoutBtn = document.getElementById("logout-btn");
const myScoreEl = document.getElementById("my-score");

// 🔹 Zelfde emoji's als bij profiel
const allAvatars = {
  1: "🙂", 2: "😎", 3: "👩‍⚕️", 4: "👨‍⚕️", 5: "👽", 6: "👻", 7: "🤖", 8: "🐉",
  9: "🦊", 10: "🐧", 11: "🐵", 12: "🐸", 13: "🐼", 14: "🦁", 15: "🐰", 16: "🐱"
};

// 🔹 Leaderboard ophalen
async function loadLeaderboard() {
  try {
    const res = await fetch(`${backendBase}/GetAll`);
    if (!res.ok) throw new Error("Kon leaderboard niet ophalen");

    const users = await res.json();

    // Sorteer op munten (aflopend)
    users.sort((a, b) => b.munten - a.munten);

    leaderboardBody.innerHTML = "";

    const currentUsername = sessionStorage.getItem("epicqrizzz-username");
    const selectedAvatarId = sessionStorage.getItem("selectedAvatarId");
    const myAvatar = allAvatars[selectedAvatarId] || "🙂";

    let myRank = null;
    let myCoins = 0;

    users.forEach((user, index) => {
      const row = document.createElement("tr");

      // Top 3 icoontjes
      let rankIcon = "";
      if (index === 0) rankIcon = "🥇";
      else if (index === 1) rankIcon = "🥈";
      else if (index === 2) rankIcon = "🥉";

      const avatarEmoji = user.name === currentUsername ? myAvatar : "🙂";

      // 🔸 Username beperken tot max. 15 karakters
      const truncatedName =
        user.name.length > 15 ? user.name.substring(0, 15) + "…" : user.name;

      row.innerHTML = `
        <td><strong>${index + 1}</strong> ${rankIcon}</td>
        <td class="fs-5">${avatarEmoji}</td>
        <td><span class="truncate" title="${user.name}">${truncatedName}</span></td>
        <td><i class="bi bi-coin text-warning"></i> ${user.munten}</td>
      `;
      leaderboardBody.appendChild(row);

      // Onthoud eigen positie
      if (user.name === currentUsername) {
        myRank = index + 1;
        myCoins = user.munten;
      }
    });

    // Toon eigen score
    if (myRank !== null) {
      myScoreEl.innerHTML = `
        ${myAvatar} Jij staat op plek <strong>${myRank}</strong>
        met <strong><i class="bi bi-coin text-warning"></i> ${myCoins}</strong> munten!
      `;
    } else {
      myScoreEl.textContent = "Je staat nog niet in het leaderboard.";
    }

  } catch (err) {
    console.error("Fout bij laden leaderboard:", err);
    leaderboardBody.innerHTML = `
      <tr><td colspan="4" class="text-danger">Kon leaderboard niet laden</td></tr>
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
loadLeaderboard();
