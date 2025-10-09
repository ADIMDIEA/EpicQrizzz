const gameData = {  
  player: {
    name: "Jij",
    avatar: localStorage.getItem("epicqrizzz-avatar") || "🙂",
    score: 0,
    mistakes: 0
  },
  currentQuestionId: null,
  questionCount: 0,
  maxQuestions: 170,
  gameEnded: false,
  askedQuestions: new Set(),
  durationMinutes: 10, // standaard oefentijd
  timeRemaining: 0,
  timerInterval: null
};

// UI vullen
document.getElementById("player-avatar").textContent = gameData.player.avatar;
document.getElementById("player-name").textContent = gameData.player.name;
document.getElementById("player-score").textContent = gameData.player.score;
document.getElementById("player-mistakes").textContent = gameData.player.mistakes;

// Startknop functionaliteit
document.getElementById("start-btn").addEventListener("click", () => {
  const select = document.getElementById("duration-select");
  gameData.durationMinutes = parseInt(select.value, 10);
  gameData.timeRemaining = gameData.durationMinutes * 60; // seconden

  document.getElementById("setup-container").style.display = "none";
  document.getElementById("quiz-container").style.display = "block";

  startTimer();
  loadQuestion();
});

// Timer starten
function startTimer() {
  updateTimerDisplay();
  gameData.timerInterval = setInterval(() => {
    gameData.timeRemaining--;

    if (gameData.timeRemaining <= 0) {
      clearInterval(gameData.timerInterval);
      endQuiz("⏰ De tijd is om!");
    }

    updateTimerDisplay();
  }, 1000);
}

// Tijdweergave bijwerken
function updateTimerDisplay() {
  const minutes = Math.floor(gameData.timeRemaining / 60);
  const seconds = gameData.timeRemaining % 60;
  document.getElementById("time-remaining").textContent = 
    `${minutes}:${seconds.toString().padStart(2, "0")}`;
}

// Vraag laden
async function loadQuestion() {
  if (gameData.gameEnded) return;

  try {
    if (gameData.questionCount >= gameData.maxQuestions) return;

    let randomId;
    do {
      randomId = Math.floor(Math.random() * gameData.maxQuestions) + 1;
    } while (gameData.askedQuestions.has(randomId));
    gameData.askedQuestions.add(randomId);

    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/GetById/${randomId}`);
    if (!res.ok) return;

    const q = await res.json();
    if (!q || !q.questionText) return;

    const sanitizedQuestion = q.questionText.replace(/\s+/gu, " ").trim();
    gameData.currentQuestionId = q.id;
    gameData.questionCount++;

    document.getElementById("quiz-question").textContent = sanitizedQuestion;
    const answerContainer = document.getElementById("answer-options");
    answerContainer.innerHTML = "";

    q.options.forEach(opt => {
      const sanitizedAnswer = opt.awnserText.replace(/\s+/gu, " ").trim();

      const btn = document.createElement("button");
      btn.className = "btn btn-outline-primary";
      btn.textContent = sanitizedAnswer;

      btn.addEventListener("click", async () => {
        if (!gameData.gameEnded) await checkAnswer(q.id, opt.option);
      });

      answerContainer.appendChild(btn);
    });

  } catch (err) {
    console.error("Fout bij ophalen van vraag:", err);
  }
}

// Antwoord checken
async function checkAnswer(questionId, answerOption) {
  try {
    if (gameData.gameEnded) return;

    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/${questionId}/${answerOption}`);
    if (!res.ok) throw new Error("Netwerkfout bij checkAnswer");

    const text = await res.text();
    const isCorrect = text.trim() === "true";
    const resultDiv = document.getElementById("result"); // feedback links onder

    if (isCorrect) {
      gameData.player.score++;
      document.getElementById("player-score").textContent = gameData.player.score;
      resultDiv.innerHTML = `<p class="text-success fw-bold text-center">✅ Correct!</p>`;
    } else {
      gameData.player.mistakes++;
      document.getElementById("player-mistakes").textContent = gameData.player.mistakes;
      resultDiv.innerHTML = `<p class="text-danger fw-bold text-center">❌ Fout!</p>`;
    }

    setTimeout(() => {
      if (!gameData.gameEnded) loadQuestion();
    }, 1000);

  } catch (err) {
    console.error("Fout bij checkAnswer:", err);
  }
}

// Quiz stoppen (timer of home)
function endQuiz(customMsg = "🎉 Oefenen gestopt!") {
  if (gameData.gameEnded) return;

  gameData.gameEnded = true;
  clearInterval(gameData.timerInterval);

  const msg = `${customMsg}\n\nAantal vragen: ${gameData.questionCount}\nGoed: ${gameData.player.score}\nFout: ${gameData.player.mistakes}`;
  alert(msg);
  window.location.href = "home.html";
}

// Terugknop Home
document.getElementById("back-btn").addEventListener("click", () => {
  if (confirm("Wil je stoppen met oefenen?")) {
    endQuiz();
  }
});
