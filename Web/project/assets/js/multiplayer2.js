const urlParams = new URLSearchParams(window.location.search);
const room = urlParams.get("room");
const userUUID = urlParams.get("user");

if (!room || !userUUID) alert("Missing room or user information!");

document.getElementById("roomInfo").innerText = `Room: ${room} | User: ${userUUID}`;

let currentQuestionId = null;
let gameEnded = false;

async function loadQuestion() {
  if (gameEnded) return;

  try {
    const res = await fetch(`http://joost.assenbergh.nl:5293/api/Game/GetGameQuestion/${userUUID}`);
    if (res.status === 400) {
      document.getElementById("quiz").innerHTML = `<p class="text-primary fw-bold">🎉 Je hebt alle vragen beantwoord!</p>`;
      document.getElementById("result").innerHTML = "";
      pollEndGame();
      return;
    }

    const data = await res.json();
    currentQuestionId = data.id;

    const quizContainer = document.getElementById("quiz");
    quizContainer.innerHTML = "";

    const q = document.createElement("div");
    q.className = "mb-3 fw-bold";
    q.innerText = data.questionText;
    quizContainer.appendChild(q);

    data.options.slice(0,6).forEach(opt => {
      const div = document.createElement("div");
      div.className = "form-check";

      const radio = document.createElement("input");
      radio.type = "radio";
      radio.name = "answer";
      radio.value = opt.option.toLowerCase();
      radio.className = "form-check-input";

      const label = document.createElement("label");
      label.className = "form-check-label";
      label.innerText = opt.awnserText;

      div.appendChild(radio);
      div.appendChild(label);
      quizContainer.appendChild(div);
    });
  } catch (err) {
    console.error("Error loading question:", err);
    document.getElementById("quiz").innerText = "❌ Kon de vraag niet laden.";
  }
}

async function submitAnswer() {
  const selected = document.querySelector("input[name='answer']:checked");
  if (!selected) {
    alert("Kies een antwoord!");
    return;
  }

  const chosenOption = selected.value;

  try {
    const res = await fetch(
      `http://joost.assenbergh.nl:5293/api/Game/CheckGameAnswer/${userUUID}/${currentQuestionId}/${chosenOption}`
    );
    const data = await res.json();

    const resultDiv = document.getElementById("result");
    if (data.isCorrect) {
      resultDiv.innerHTML = `<p class="text-success fw-bold">✅ Correct!</p>`;
    } else {
      resultDiv.innerHTML = `<p class="text-danger fw-bold">❌ Fout!</p>`;
    }

    updatePlayers();

    setTimeout(() => {
      resultDiv.innerHTML = "";
      loadQuestion();
    }, 1000);

  } catch (err) {
    console.error("Error checking answer:", err);
    document.getElementById("result").innerHTML = `<p class="text-danger fw-bold">❌ Kon antwoord niet checken</p>`;
  }
}

async function updatePlayers() {
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5293/api/Game/GetRoom/${room}`);
    const players = await res.json();

    const tbody = document.getElementById("playersTableBody");
    tbody.innerHTML = "";

    players.forEach(p => {
      const row = document.createElement("tr");
      row.innerHTML = `<td>${p.userId}</td><td><span class="fw-bold">${p.score}</span></td>`;
      tbody.appendChild(row);
    });
  } catch (err) {
    console.error("Error fetching players:", err);
  }
}

async function pollEndGame() {
  if (gameEnded) return;
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5293/api/Game/EndGame/${userUUID}`);
    if (!res.ok) return;

    const data = await res.json();
    if (data.top3 && data.top3.length > 0) {
      gameEnded = true;
      document.getElementById("endGameCard").style.display = "block";
      const ul = document.getElementById("top3List");
      ul.innerHTML = "";
      data.top3.forEach(p => {
        const li = document.createElement("li");
        li.className = "list-group-item fw-bold";
        li.innerText = `${p.name} - ${p.score} punten`;
        ul.appendChild(li);
      });
    } else {
      setTimeout(pollEndGame, 2000);
    }
  } catch (err) {
    console.error("Error polling end game:", err);
    setTimeout(pollEndGame, 2000);
  }
}

setInterval(updatePlayers, 5000);
updatePlayers();
loadQuestion();
