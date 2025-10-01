const urlParams = new URLSearchParams(window.location.search);
const room = urlParams.get("room");
const userUUID = urlParams.get("user");

if (!room || !userUUID) alert("Missing room or user info!");

document.getElementById("roomInfo").innerText = `Lobby - Room: ${room}`;

const playersList = document.getElementById("playersList");
const statusDiv = document.getElementById("status");
const startBtn = document.getElementById("startBtn");

// Fetch spelers in de kamer
async function updatePlayers() {
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5293/api/Game/GetRoom/${room}`);
    const players = await res.json();

    playersList.innerHTML = "";
    players.forEach(p => {
      const li = document.createElement("li");
      li.className = "list-group-item";
      li.innerText = p.userId;
      playersList.appendChild(li);
    });
  } catch (err) {
    console.error("Error fetching players:", err);
  }
}

// Start game
startBtn.addEventListener("click", async () => {
  const numQuestions = Math.min(100, Math.max(1, parseInt(document.getElementById("numQuestions").value)));
  try {
    const res = await fetch("http://joost.assenbergh.nl:5293/api/Game/StartGame", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userUUID, numQuestions })
    });

    if (!res.ok) {
      const text = await res.text();
      statusDiv.innerText = text;
    }
  } catch (err) {
    console.error("Error starting game:", err);
    statusDiv.innerText = "❌ Kon het spel niet starten";
  }
});

// Poll of game gestart is
async function checkGameStarted() {
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5293/api/Game/IsGameStarted/${userUUID}`);
    const data = await res.json();
    if (data.started) {
      window.location.href = `multiplayer2.html?room=${room}&user=${userUUID}`;
    }
  } catch (err) {
    console.error("Error checking game status:", err);
  }
}

// Auto-refresh spelerslijst en check game start
setInterval(updatePlayers, 2000);
setInterval(checkGameStarted, 2000);
updatePlayers();
checkGameStarted();