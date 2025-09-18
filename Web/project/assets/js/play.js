document.addEventListener("DOMContentLoaded", () => {
  const backendBase = "http://joost.assenbergh.nl:5291/api/Quetion";

  const playerAvatar = document.getElementById("player-avatar");
  const playerScoreElem = document.getElementById("player-score");
  const opponentScoreElem = document.getElementById("opponent-score");
  const quizQuestion = document.getElementById("quiz-question");
  const answerOptions = document.getElementById("answer-options");
  const timerElem = document.getElementById("timer");

  let playerScore = 0;
  let opponentScore = 0;
  let timer = 10;
  let timerInterval;

  const savedAvatar = localStorage.getItem("epicqrizzz-avatar");
  if (savedAvatar) playerAvatar.textContent = savedAvatar;

  async function loadQuestion(id = 1) {
    try {
      const res = await fetch(`${backendBase}/GetById/${id}`);
      if (!res.ok) throw new Error("Vraag laden mislukt");
      const q = await res.json();

      quizQuestion.textContent = q.text;
      answerOptions.innerHTML = "";
      q.answers.forEach((ans) => {
        const btn = document.createElement("button");
        btn.textContent = ans.text;
        btn.addEventListener("click", () => checkAnswer(q.id, ans.id));
        answerOptions.appendChild(btn);
      });

      resetTimer();
    } catch (err) {
      quizQuestion.textContent = "Kon vraag niet laden!";
    }
  }

  async function checkAnswer(qId, ansId) {
    try {
      const res = await fetch(`${backendBase}/CheckAwnser/${qId}/${ansId}`);
      const correct = await res.json();
      if (correct) {
        playerScore++;
        playerScoreElem.textContent = `Score: ${playerScore}`;
      } else {
        opponentScore++;
        opponentScoreElem.textContent = `Score: ${opponentScore}`;
      }
      loadQuestion(qId + 1);
    } catch {
      alert("Fout bij het controleren van antwoord!");
    }
  }

  function resetTimer() {
    clearInterval(timerInterval);
    timer = 10;
    timerElem.textContent = timer;
    timerInterval = setInterval(() => {
      timer--;
      timerElem.textContent = timer;
      if (timer <= 0) {
        clearInterval(timerInterval);
        opponentScore++;
        opponentScoreElem.textContent = `Score: ${opponentScore}`;
        loadQuestion(Math.floor(Math.random() * 5) + 1);
      }
    }, 1000);
  }

  loadQuestion();
});
