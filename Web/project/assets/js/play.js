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

let isLoading = false; // voorkomt dubbele loadQuestion-aanroepen

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

// Vraag laden met foutafhandeling
async function loadQuestion() {
  if (isLoading || gameData.gameEnded) return;
  isLoading = true;

  try {
    if (gameData.questionCount >= gameData.maxQuestions) {
      console.log("Max aantal vragen bereikt.");
      endQuiz("✅ Alle vragen zijn beantwoord!");
      return;
    }

    let randomId;
    let tries = 0;
    do {
      randomId = Math.floor(Math.random() * gameData.maxQuestions) + 1;
      tries++;
      if (tries > gameData.maxQuestions) {
        console.error("Geen nieuwe vragen meer beschikbaar.");
        endQuiz("✅ Alle vragen beantwoord!");
        return;
      }
    } while (gameData.askedQuestions.has(randomId));
    gameData.askedQuestions.add(randomId);

    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/GetById/${randomId}`);
    if (!res.ok) {
      console.warn("Fout bij ophalen van vraag, probeer opnieuw...");
      setTimeout(loadQuestion, 1000);
      return;
    }

    const q = await res.json();
    if (!q || !q.questionText) {
      console.warn("Lege vraag ontvangen, opnieuw proberen...");
      setTimeout(loadQuestion, 1000);
      return;
    }

    const sanitizedQuestion = q.questionText.replace(/\s+/gu, " ").trim();
    gameData.currentQuestionId = q.id;
    gameData.questionCount++;

    // Vraag tonen
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
    // Herstelpoging bij onverwachte fout
    setTimeout(loadQuestion, 2000);
  } finally {
    isLoading = false;
  }
}

// Antwoord checken met 3 seconden pauze
async function checkAnswer(questionId, answerOption) {
  try {
    if (gameData.gameEnded) return;

    // Alle antwoordknoppen tijdelijk uitschakelen
    const buttons = document.querySelectorAll("#answer-options button");
    buttons.forEach(btn => btn.disabled = true);

    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/${questionId}/${answerOption}`);
    if (!res.ok) throw new Error("Netwerkfout bij checkAnswer");

    const text = await res.text();
    const isCorrect = text.trim() === "true";
    const resultDiv = document.getElementById("result");

    if (isCorrect) {
      gameData.player.score++;
      document.getElementById("player-score").textContent = gameData.player.score;
      resultDiv.innerHTML = `<p class="text-success fw-bold text-center">✅ Correct!</p>`;
    } else {
      gameData.player.mistakes++;
      document.getElementById("player-mistakes").textContent = gameData.player.mistakes;
      resultDiv.innerHTML = `<p class="text-danger fw-bold text-center">❌ Fout!</p>`;
    }

    // Na 3 seconden volgende vraag laden
    setTimeout(() => {
      if (!gameData.gameEnded) {
        resultDiv.innerHTML = "";
        loadQuestion();
      }
    }, 3000);

  } catch (err) {
    console.error("Fout bij checkAnswer:", err);
    // Bij netwerkfout gewoon volgende vraag proberen
    setTimeout(loadQuestion, 2000);
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
