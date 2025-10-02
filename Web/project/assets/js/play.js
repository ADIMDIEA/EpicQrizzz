const gameData = {
  player: {
    name: "Jij",
    avatar: localStorage.getItem("epicqrizzz-avatar") || "🙂",
    score: 0
  },
  opponent: {
    name: "Bot",
    avatar: "🤖",
    score: 0
  },
  currentQuestion: 1,
  totalQuestions: null, // wordt dynamisch bepaald
};

document.getElementById("player-avatar").textContent = gameData.player.avatar;
document.getElementById("player-name").textContent = gameData.player.name;
document.getElementById("player-score").textContent = gameData.player.score;

document.getElementById("opponent-avatar").textContent = gameData.opponent.avatar;
document.getElementById("opponent-name").textContent = gameData.opponent.name;
document.getElementById("opponent-score").textContent = gameData.opponent.score;

async function showQuestion(id) {
  try {
    const res = await fetch(`http://joost.assenbergh.nl:5291/api/quetion/GetById/${id}`);
    if (!res.ok) {
      endGame();
      return;
    }

    const q = await res.json();
    if (!q || !q.questionText) {
      endGame();
      return;
    }

    document.getElementById("quiz-question").textContent = q.questionText;
    const answerContainer = document.getElementById("answer-options");
    answerContainer.innerHTML = "";
    document.getElementById("result").innerHTML = "";

    q.options.forEach(opt => {
      const btn = document.createElement("button");
      btn.className = "btn btn-outline-primary";
      btn.textContent = opt.awnserText;

      btn.addEventListener("click", async () => {
        try {
          const checkRes = await fetch(
            `http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/${id}/${opt.id}`
          );
          if (!checkRes.ok) throw new Error("Netwerkfout bij checkAnswer");

          const isCorrect = await checkRes.json();
          const resultDiv = document.getElementById("result");

          if (isCorrect) {
            gameData.player.score++;
            document.getElementById("player-score").textContent = gameData.player.score;
            resultDiv.innerHTML = `<p class="text-success fw-bold">✅ Correct!</p>`;
          } else {
            gameData.opponent.score++;
            document.getElementById("opponent-score").textContent = gameData.opponent.score;
            resultDiv.innerHTML = `<p class="text-danger fw-bold">❌ Fout!</p>`;
          }

          setTimeout(() => {
            gameData.currentQuestion++;
            showQuestion(gameData.currentQuestion);
          }, 1000);

        } catch (err) {
          console.error("Fout bij checkAnswer:", err);
          alert("Fout bij controleren van antwoord.");
        }
      });

      answerContainer.appendChild(btn);
    });
  } catch (err) {
    console.error("Fout bij ophalen van vraag:", err);
    endGame();
  }
}

function endGame() {
  let message = `🎉 Quiz afgelopen!\n\nJouw score: ${gameData.player.score}\nBot score: ${gameData.opponent.score}`;
  if (gameData.player.score > gameData.opponent.score) {
    message += "\n\nGefeliciteerd! Je hebt gewonnen 🏆";
  } else if (gameData.player.score < gameData.opponent.score) {
    message += "\n\nHelaas, de bot heeft gewonnen 🤖";
  } else {
    message += "\n\nGelijkspel! ⚖️";
  }
  alert(message);
  window.location.href = "home.html";
}

document.getElementById("back-btn").addEventListener("click", () => {
  if (confirm("Weet je zeker dat je wilt stoppen? Je verliest deze ronde.")) {
    endGame();
  }
});

// Start quiz
showQuestion(gameData.currentQuestion);
