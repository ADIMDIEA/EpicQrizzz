const gameData = {
  player: {
    name: "Jij",
    avatar: localStorage.getItem("epicqrizzz-avatar") || "🙂",
    score: 0,           // goed beantwoorde vragen
    mistakes: 0         // fout beantwoorde vragen
  },
  currentQuestionId: null,
  questionCount: 0,
  maxQuestions: 170,
  gameEnded: false,
  askedQuestions: new Set() // om herhaling te voorkomen
};

// UI vullen
document.getElementById("player-avatar").textContent = gameData.player.avatar;
document.getElementById("player-name").textContent = gameData.player.name;
document.getElementById("player-score").textContent = gameData.player.score;
document.getElementById("player-mistakes").textContent = gameData.player.mistakes;

// Vraag laden
async function loadQuestion() {
  if (gameData.gameEnded) return;

  if (gameData.questionCount >= gameData.maxQuestions) {
    showEndMessage();
    return;
  }

  try {
    let randomId;
    do {
      randomId = Math.floor(Math.random() * gameData.maxQuestions) + 1;
    } while (gameData.askedQuestions.has(randomId)); // geen herhaling
    gameData.askedQuestions.add(randomId);

    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/GetById/${randomId}`);
    if (!res.ok) {
      showEndMessage();
      return;
    }

    const q = await res.json();
    if (!q || !q.questionText) {
      showEndMessage();
      return;
    }

    gameData.currentQuestionId = q.id;
    gameData.questionCount++;

    // UI bijwerken
    document.getElementById("quiz-question").textContent = q.questionText;
    const answerContainer = document.getElementById("answer-options");
    answerContainer.innerHTML = "";
    document.getElementById("result").innerHTML = "";

    q.options.forEach(opt => {
      const btn = document.createElement("button");
      btn.className = "btn btn-outline-primary";
      btn.textContent = opt.awnserText;

      btn.addEventListener("click", async () => {
        await checkAnswer(q.id, opt.option); // A1, A2, etc.
      });

      answerContainer.appendChild(btn);
    });

  } catch (err) {
    console.error("Fout bij ophalen van vraag:", err);
    showEndMessage();
  }
}

// Antwoord checken
async function checkAnswer(questionId, answerOption) {
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/${questionId}/${answerOption}`);
    if (!res.ok) throw new Error("Netwerkfout bij checkAnswer");

    const text = await res.text();             // API returns plain true/false
    const isCorrect = text.trim() === "true";

    const resultDiv = document.getElementById("result");

    if (isCorrect) {
      gameData.player.score++;
      document.getElementById("player-score").textContent = gameData.player.score;
      resultDiv.innerHTML = `<p class="text-success fw-bold">✅ Correct!</p>`;
    } else {
      gameData.player.mistakes++;
      document.getElementById("player-mistakes").textContent = gameData.player.mistakes;
      resultDiv.innerHTML = `<p class="text-danger fw-bold">❌ Fout!</p>`;
    }

    // Volgende vraag na korte pauze
    setTimeout(() => {
      loadQuestion();
    }, 1000);

  } catch (err) {
    console.error("Fout bij checkAnswer:", err);
    alert("Fout bij controleren van antwoord.");
  }
}

// Einde quiz
function showEndMessage() {
  gameData.gameEnded = true;
  const msg = `🎉 Oefenen gestopt!\n\nAantal vragen: ${gameData.questionCount}\nGoed: ${gameData.player.score}\nFout: ${gameData.player.mistakes}`;
  alert(msg);
  window.location.href = "home.html";
}

// Terugknop
document.getElementById("back-btn").addEventListener("click", () => {
  if (confirm("Wil je stoppen met oefenen?")) {
    showEndMessage();
  }
});

// Start quiz
loadQuestion();
