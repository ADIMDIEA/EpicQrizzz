document.addEventListener("DOMContentLoaded", () => {
  const joinBtn = document.getElementById("joinBtn");
  const roomInput = document.getElementById("roomCode");
  const statusDiv = document.getElementById("status");

  joinBtn.addEventListener("click", joinRoom);

  async function joinRoom() {
    const room = roomInput.value.trim().toUpperCase();
    const userUUID = sessionStorage.getItem("epicqrizzz-userId");

    if (!room) {
      alert("Please enter a room code!");
      return;
    }
    if (!userUUID) {
      alert("UUID not found in sessionStorage!");
      return;
    }

    try {
      const res = await fetch("http://joost.assenbergh.nl:5293/api/Game/JoinOrAdd", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          room: room,
          userId: userUUID,
          score: "0"
        })
      });

      const responseText = await res.text();
      console.log("Room response:", responseText);

      // ✅ Redirect to lobby.html instead of multiplayer2.html
      window.location.href = `lobby.html?room=${room}&user=${userUUID}`;
    } catch (err) {
      console.error("Error joining room:", err);
      statusDiv.innerHTML = `<span style="color:red">❌ Failed to join room</span>`;
    }
  }
});
