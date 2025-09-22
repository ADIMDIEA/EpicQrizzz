const gameData = {
  player: {
    name: "Jij",
    avatar: localStorage.getItem('epicqrizzz-avatar') || "🙂",
    score: 0
  },
  opponent: {
    name: "MedStudent42",
    avatar: "👩‍⚕️",
    score: 0
  },
  currentQuestion: 1,
  winningScore: 10,
  timePerQuestion: 30,
  timerInterval: null
};

const timerElement = document.getElementById('timer');

document.getElementById('player-avatar').textContent = gameData.player.avatar;
document.getElementById('player-name').textContent = gameData.player.name;
document.getElementById('player-score').textContent = gameData.player.score;

document.getElementById('opponent-avatar').textContent = gameData.opponent.avatar;
document.getElementById('opponent-name').textContent = gameData.opponent.name;
document.getElementById('opponent-score').textContent = gameData.opponent.score;

function startTimer(callback) {
  let timeLeft = gameData.timePerQuestion;
  timerElement.textContent = timeLeft + 's';
  clearInterval(gameData.timerInterval);
  gameData.timerInterval = setInterval(() => {
    timeLeft--;
    timerElement.textContent = timeLeft + 's';
    if (timeLeft <= 0) {
      clearInterval(gameData.timerInterval);
      callback();
    }
  }, 1000);
}

async function showQuestion(id) {
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/GetById/${id}`);
    if (!res.ok) throw new Error("Netwerkfout");
    const q = await res.json();

    document.getElementById('quiz-question').textContent = q.questionText;
    const answerContainer = document.getElementById('answer-options');
    answerContainer.innerHTML = "";

    // Timer starten
    startTimer(() => {
      gameData.opponent.score++;
      document.getElementById('opponent-score').textContent = gameData.opponent.score;
      if (gameData.player.score >= gameData.winningScore || gameData.opponent.score >= gameData.winningScore) {
        endGame();
      } else {
        gameData.currentQuestion++;
        showQuestion(gameData.currentQuestion);
      }
    });

    // Dynamisch knoppen maken op basis van q.options
    q.options.forEach(opt => {
      const btn = document.createElement('button');
      btn.textContent = opt.awnserText;
      btn.addEventListener('click', async () => {
        clearInterval(gameData.timerInterval);
        try {
          console.log(id, opt.id);
          const checkRes = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/${id}/${opt.id}`);
          if (!checkRes.ok) throw new Error("Netwerkfout bij checkAnswer");
          const isCorrect = await checkRes.json();

          if (isCorrect) {
            gameData.player.score++;
            document.getElementById('player-score').textContent = gameData.player.score;
          } else {
            gameData.opponent.score++;
            document.getElementById('opponent-score').textContent = gameData.opponent.score;
          }

          if (gameData.player.score >= gameData.winningScore || gameData.opponent.score >= gameData.winningScore) {
            endGame();
          } else {
            gameData.currentQuestion++;
            showQuestion(gameData.currentQuestion);
          }
        } catch(err) {
          console.error("Fout bij checkAnswer:", err);
          alert("Fout bij controleren van antwoord.");
        }
      });
      answerContainer.appendChild(btn);
    });
  } catch (err) {
    console.error("Fout bij ophalen van vraag:", err);
    document.getElementById('quiz-question').textContent = "Kon vraag niet laden.";
  }
}

function endGame() {
  clearInterval(gameData.timerInterval);
  let message;
  if (gameData.player.score >= gameData.winningScore) {
    message = "Gefeliciteerd! Je hebt gewonnen 🏆";
  } else if (gameData.opponent.score >= gameData.winningScore) {
    message = "Helaas, je hebt verloren ❌";
  }
  alert(message);
  window.location.href = 'home.html';
}

document.getElementById('back-btn').addEventListener('click', () => {
  if (confirm("Weet je zeker dat je wilt stoppen? Je verliest deze ronde.")) {
    clearInterval(gameData.timerInterval);
    gameData.opponent.score++;
    document.getElementById('opponent-score').textContent = gameData.opponent.score;
    endGame();
  }
});

// Start met de eerste vraag
showQuestion(gameData.currentQuestion);
