document.getElementById("addQuestion").addEventListener("click", async () => {
  const questionText = document.getElementById("questionText").value;
  const optionA = document.getElementById("optionA").value;
  const optionB = document.getElementById("optionB").value;
  const optionC = document.getElementById("optionC").value;
  const optionD = document.getElementById("optionD").value;
  const correctAnswer = document.getElementById("correctAnswer").value.toUpperCase();

  if (!questionText || !optionA || !optionB || !optionC || !optionD || !["A","B","C","D"].includes(correctAnswer)) {
    document.getElementById("result").innerHTML = "❌ Vul alle velden correct in.";
    document.getElementById("result").className = "text-danger fw-bold text-center";
    return;
  }

  try {
    const res = await fetch("https://localhost:7112/api/Quetion/Add", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        questionText,
        optionA,
        optionB,
        optionC,
        optionD,
        correctAnswer
      })
    });

    if (res.ok) {
      document.getElementById("result").innerHTML = "✅ Vraag succesvol toegevoegd!";
      document.getElementById("result").className = "text-success fw-bold text-center";
      
      // velden leegmaken
      document.getElementById("questionText").value = "";
      document.getElementById("optionA").value = "";
      document.getElementById("optionB").value = "";
      document.getElementById("optionC").value = "";
      document.getElementById("optionD").value = "";
      document.getElementById("correctAnswer").value = "";
    } else {
      document.getElementById("result").innerHTML = "❌ Fout bij toevoegen.";
      document.getElementById("result").className = "text-danger fw-bold text-center";
    }
  } catch (err) {
    console.error("Error:", err);
    document.getElementById("result").innerHTML = "⚠️ Serverfout.";
    document.getElementById("result").className = "text-danger fw-bold text-center";
  }
});
